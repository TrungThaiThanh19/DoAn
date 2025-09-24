using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DoAn.IService;
using DoAn.Models;
using DoAn.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    [Authorize(Roles = "admin,nhanvien")]
    public class HoaDonController : Controller
    {
        private readonly DoAnDbContext _context;
        private readonly IHoaDonService _hoaDonService;
        public HoaDonController(DoAnDbContext context) => _context = context;

        // ===================== Helper lấy nhân viên =====================
        private NhanVien? GetCurrentNhanVien()
        {
            var userIdStr = HttpContext.Session.GetString("UserID");
            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var idTaiKhoan))
                return _context.NhanViens.FirstOrDefault(nv => nv.ID_TaiKhoan == idTaiKhoan);

            var username = User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
                return _context.NhanViens.Include(nv => nv.TaiKhoan)
                    .FirstOrDefault(nv => nv.TaiKhoan.Uername == username);

            return null;
        }

        private string GetCurrentNhanVienName()
        {
            var nv = GetCurrentNhanVien();
            return nv?.Ten_NhanVien ?? (User?.Identity?.Name ?? "system");
        }

        // ===================== DANH SÁCH =====================
        public IActionResult Index(string? loaiHoaDon, int? trangThai, int page = 1, int pageSize = 10)
        {
            var query = _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.TrangThaiDonHangs)
                .Include(h => h.TraHangs)
                .Include(h => h.HoaDonChiTiets)
                .AsQueryable();

            if (!string.IsNullOrEmpty(loaiHoaDon))
                query = query.Where(h => h.LoaiHoaDon.ToLower() == loaiHoaDon.ToLower());

            if (trangThai.HasValue)
                query = query.Where(h => h.TrangThai == trangThai.Value);

            int totalItems = query.Count();

            var data = query
                .OrderByDescending(h => h.NgayTao)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var list = data.Select(h =>
            {
                var lastActor = h.TrangThaiDonHangs?
                    .OrderByDescending(t => t.NgayChuyen)
                    .FirstOrDefault()?.NhanVienDoi;

                var nhanVienTen = h.NhanVien?.Ten_NhanVien
                   ?? (!string.IsNullOrWhiteSpace(lastActor) ? lastActor : "Không có");

                var hasReturnDone = h.TraHangs?.Any(p => p.TrangThai == 3) ?? false;
                var hasReturnApproved = h.TraHangs?.Any(p => p.TrangThai == 1) ?? false;
                var hasReturnReceived = h.TraHangs?.Any(p => p.TrangThai == 2) ?? false;
                var hasReturnRequest =
                    (h.TrangThaiDonHangs?.Any(t => t.TrangThai == 6) ?? false) ||
                    (h.TraHangs?.Any(t => t.TrangThai == 0) ?? false);

                string trangThaiText;
                if (hasReturnDone) trangThaiText = "Đã hoàn hàng";
                else if (hasReturnReceived) trangThaiText = "Đã nhận hàng hoàn";
                else if (hasReturnApproved) trangThaiText = "Đã duyệt hoàn hàng";
                else if (hasReturnRequest) trangThaiText = "Có yêu cầu hoàn hàng";
                else trangThaiText = GetTrangThaiText(h.TrangThai);

                var ship = h.PhuThu ?? 0m;
                var tongHang_Goc = h.HoaDonChiTiets?.Sum(ct => ct.SoLuong * ct.DonGia) ?? 0m;
                var tongSauGiam = h.TongTienSauGiam != 0m ? h.TongTienSauGiam : (tongHang_Goc + ship);
                var tienGiam = Math.Max(0m, (tongHang_Goc + ship) - tongSauGiam);

                return new HoaDonViewModel
                {
                    ID_HoaDon = h.ID_HoaDon,
                    Ma_HoaDon = h.Ma_HoaDon,
                    HoTen = h.HoTen ?? "Khách lẻ",
                    NhanVienTen = nhanVienTen,
                    LoaiHoaDon = h.LoaiHoaDon,
                    NgayTao = h.NgayTao,
                    PhuThu = ship,
                    TongTienSauGiam = tongSauGiam,
                    TienGiam = tienGiam,
                    TrangThai = h.TrangThai,
                    TrangThaiText = trangThaiText,
                    HasReturnRequest = hasReturnRequest || hasReturnApproved || hasReturnReceived,
                    HasReturnDone = hasReturnDone
                };
            }).ToList();

            ViewBag.LoaiHoaDon = loaiHoaDon;
            ViewBag.TrangThai = trangThai;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.TotalItems = totalItems;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            return View(list);
        }

        // ===================== TRẠNG THÁI JSON =====================
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Status(Guid id)
        {
            var hd = _context.HoaDons.AsNoTracking().FirstOrDefault(x => x.ID_HoaDon == id);
            if (hd == null) return NotFound();

            var hasReturnDone = _context.QuanLyTraHangs.AsNoTracking()
                .Any(p => p.ID_HoaDon == id && p.TrangThai == 3);

            TrangThaiDonHang? cancelLog = null;
            if (hd.TrangThai == 5)
            {
                cancelLog = _context.TrangThaiDonHangs.AsNoTracking()
                    .Where(t => t.ID_HoaDon == id && t.TrangThai == 5)
                    .OrderByDescending(t => t.NgayChuyen)
                    .FirstOrDefault();
            }

            return Json(new
            {
                ok = true,
                id = hd.ID_HoaDon,
                status = hd.TrangThai,
                text = hasReturnDone ? "Đã hoàn hàng" : GetTrangThaiText(hd.TrangThai),
                updatedAt = (hd.NgayCapNhat ?? hd.NgayTao).ToString("yyyy-MM-dd HH:mm:ss"),
                cancel = hd.TrangThai == 5,
                cancelReason = cancelLog?.NoiDungDoi,
                cancelBy = cancelLog?.NhanVienDoi,
                cancelAt = cancelLog?.NgayChuyen.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        // ===================== CHI TIẾT =====================
        public IActionResult Details(Guid id)
        {
            var hoaDon = _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(v => v.SanPham)
                .Include(h => h.TraHangs).ThenInclude(t => t.ChiTietTraHangs)
                .Include(h => h.TrangThaiDonHangs)
                .FirstOrDefault(h => h.ID_HoaDon == id);

            if (hoaDon == null) return NotFound();
            return View(hoaDon);
        }

        // ===================== CẬP NHẬT TRẠNG THÁI =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CapNhatTrangThai(Guid idHoaDon)
        {
            var hoaDon = _context.HoaDons
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(v => v.SanPham)
                .FirstOrDefault(h => h.ID_HoaDon == idHoaDon);

            if (hoaDon == null) return NotFound();

            var old = hoaDon.TrangThai;
            var nvName = GetCurrentNhanVienName();

            // OFFLINE -> thành công
            if (hoaDon.LoaiHoaDon.Equals("Offline", StringComparison.OrdinalIgnoreCase))
            {
                hoaDon.TrangThai = 4;
                hoaDon.NgayCapNhat = DateTime.Now;
                hoaDon.ID_NhanVien = GetCurrentNhanVien()?.ID_NhanVien;

                _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
                {
                    ID_TrangThaiDonHang = Guid.NewGuid(),
                    ID_HoaDon = idHoaDon,
                    TrangThai = 4,
                    NgayChuyen = DateTime.Now,
                    NhanVienDoi = nvName,
                    NoiDungDoi = "Đơn Offline -> Thanh toán trực tiếp -> Thành công"
                });

                _context.SaveChanges();
                TempData["Success"] = "Đơn Offline đã thành công.";
                return RedirectToAction("Details", new { id = idHoaDon });
            }

            // ONLINE
            if (hoaDon.TrangThai == 0)
            {
                // check kho
                var thieu = new List<string>();
                foreach (var ct in hoaDon.HoaDonChiTiets)
                {
                    var spct = ct.SanPhamChiTiet
                               ?? _context.SanPhamChiTiets.FirstOrDefault(x => x.ID_SanPhamChiTiet == ct.ID_SanPhamChiTiet);
                    if (spct == null) continue;

                    var con = ReadTonKho(spct);
                    if (con < ct.SoLuong)
                    {
                        var ten = spct.SanPham?.Ten_SanPham ?? $"SP#{spct.ID_SanPhamChiTiet.ToString()[..8]}";
                        thieu.Add($"{ten} (cần {ct.SoLuong}, còn {con})");
                    }
                }
                if (thieu.Any())
                {
                    TempData["Error"] = "Không đủ hàng: " + string.Join(", ", thieu);
                    return RedirectToAction("Details", new { id = idHoaDon });
                }

                foreach (var ct in hoaDon.HoaDonChiTiets)
                {
                    var spct = ct.SanPhamChiTiet
                               ?? _context.SanPhamChiTiets.FirstOrDefault(x => x.ID_SanPhamChiTiet == ct.ID_SanPhamChiTiet);
                    if (spct != null) DecreaseTonKho(spct, ct.SoLuong);
                }

                hoaDon.TrangThai = 1;
                hoaDon.NgayCapNhat = DateTime.Now;
                hoaDon.ID_NhanVien = GetCurrentNhanVien()?.ID_NhanVien;

                _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
                {
                    ID_TrangThaiDonHang = Guid.NewGuid(),
                    ID_HoaDon = idHoaDon,
                    TrangThai = 1,
                    NgayChuyen = DateTime.Now,
                    NhanVienDoi = nvName,
                    NoiDungDoi = $"Cập nhật: {GetTrangThaiText(old)} -> {GetTrangThaiText(1)}"
                });

                _context.SaveChanges();
                TempData["Success"] = "Đã xác nhận đơn.";
                return RedirectToAction("Details", new { id = idHoaDon });
            }

            if (hoaDon.TrangThai < 4)
            {
                hoaDon.TrangThai += 1;
                hoaDon.NgayCapNhat = DateTime.Now;
                hoaDon.ID_NhanVien = GetCurrentNhanVien()?.ID_NhanVien;

                _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
                {
                    ID_TrangThaiDonHang = Guid.NewGuid(),
                    ID_HoaDon = idHoaDon,
                    TrangThai = hoaDon.TrangThai,
                    NgayChuyen = DateTime.Now,
                    NhanVienDoi = nvName,
                    NoiDungDoi = $"Cập nhật: {GetTrangThaiText(old)} -> {GetTrangThaiText(hoaDon.TrangThai)}"
                });

                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = idHoaDon });
        }

        // ===================== GIAO HÀNG KHÔNG THÀNH CÔNG =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult GiaoKhongThanhCong(Guid idHoaDon, string lyDo)
        {
            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["Error"] = "Nhập lý do không giao được.";
                return RedirectToAction("Details", new { id = idHoaDon });
            }

            var hoaDon = _context.HoaDons
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                .FirstOrDefault(h => h.ID_HoaDon == idHoaDon);

            if (hoaDon == null) return NotFound();

            // Chỉ áp dụng cho đơn đang vận chuyển (trạng thái 2)
            if (hoaDon.TrangThai != 2)
            {
                TempData["Error"] = "Chỉ thực hiện khi đơn đang vận chuyển.";
                return RedirectToAction("Details", new { id = idHoaDon });
            }

            // Cập nhật trạng thái riêng cho giao không thành công
            hoaDon.TrangThai = 10;
            hoaDon.NgayCapNhat = DateTime.Now;
            hoaDon.ID_NhanVien = GetCurrentNhanVien()?.ID_NhanVien;

            var nvName = GetCurrentNhanVienName();
            _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = idHoaDon,
                TrangThai = 10,
                NgayChuyen = DateTime.Now,
                NhanVienDoi = nvName,
                NoiDungDoi = $"Giao hàng không thành công. Lý do: {lyDo.Trim()}"
            });

            foreach (var ct in hoaDon.HoaDonChiTiets)
            {
                var spct = ct.SanPhamChiTiet;
                if (spct != null) WriteTonKho(spct, ReadTonKho(spct) + ct.SoLuong);
            }
            _context.SaveChanges();
            TempData["Success"] = "Đã đánh dấu giao hàng không thành công!";
            return RedirectToAction("Details", new { id = idHoaDon });
        }

        // ===================== HỦY ĐƠN =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HuyDon(Guid id, string lyDo)
        {
            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["Error"] = "Nhập lý do hủy.";
                return RedirectToAction("Details", new { id });
            }

            var hd = _context.HoaDons
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                .FirstOrDefault(h => h.ID_HoaDon == id);

            if (hd == null) return NotFound();
            if (hd.TrangThai != 0 && hd.TrangThai != 1)
            {
                TempData["Error"] = "Chỉ hủy ở trạng thái Chờ xác nhận hoặc Đã xác nhận.";
                return RedirectToAction("Details", new { id });
            }

            if (hd.TrangThai == 1)
            {
                foreach (var ct in hd.HoaDonChiTiets)
                {
                    var spct = ct.SanPhamChiTiet;
                    if (spct != null) WriteTonKho(spct, ReadTonKho(spct) + ct.SoLuong);
                }
            }

            var nvName = GetCurrentNhanVienName();
            hd.TrangThai = 5;
            hd.NgayCapNhat = DateTime.Now;
            hd.ID_NhanVien = GetCurrentNhanVien()?.ID_NhanVien;

            _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = id,
                TrangThai = 5,
                NgayChuyen = DateTime.Now,
                NhanVienDoi = nvName,
                NoiDungDoi = $"Hủy đơn. Lý do: {lyDo.Trim()}"
            });

            _context.SaveChanges();
            TempData["Success"] = "Đã hủy đơn.";
            return RedirectToAction("Details", new { id });
        }

        // ===================== TẠO OFFLINE =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateOffline(HoaDon model)
        {
            if (model == null)
            {
                TempData["Error"] = "Dữ liệu không hợp lệ.";
                return RedirectToAction("Index");
            }

            model.ID_HoaDon = Guid.NewGuid();
            model.Ma_HoaDon = GenerateMaHoaDon();
            model.NgayTao = DateTime.Now;
            model.NgayCapNhat = DateTime.Now;
            model.ID_NhanVien = GetCurrentNhanVien()?.ID_NhanVien;

            if (model.LoaiHoaDon.Equals("Offline", StringComparison.OrdinalIgnoreCase))
            {
                model.TrangThai = 4;
                _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
                {
                    ID_TrangThaiDonHang = Guid.NewGuid(),
                    ID_HoaDon = model.ID_HoaDon,
                    TrangThai = 4,
                    NgayChuyen = DateTime.Now,
                    NhanVienDoi = GetCurrentNhanVienName(),
                    NoiDungDoi = "Tạo đơn Offline -> Thành công"
                });
            }
            else
            {
                model.TrangThai = 0;
            }

            _context.HoaDons.Add(model);
            _context.SaveChanges();
            TempData["Success"] = "Tạo hóa đơn thành công.";
            return RedirectToAction("Details", new { id = model.ID_HoaDon });
        }

        // ===================== Helpers =====================
        private static PropertyInfo? QtyProp(Type t) =>
            t.GetProperty("SoLuongTon")
            ?? t.GetProperty("SoLuong")
            ?? t.GetProperty("TonKho")
            ?? t.GetProperty("SoLuong_TonKho");

        private static int ReadTonKho(object spct)
        {
            var p = QtyProp(spct.GetType());
            return p == null ? 0 : Convert.ToInt32(p.GetValue(spct) ?? 0);
        }

        private static void WriteTonKho(object spct, int value)
        {
            var p = QtyProp(spct.GetType());
            if (p != null) p.SetValue(spct, value);
        }

        private static void DecreaseTonKho(object spct, int qty)
        {
            var cur = ReadTonKho(spct);
            WriteTonKho(spct, Math.Max(0, cur - qty));
        }

        private string GenerateMaHoaDon()
        {
            string prefix = "HD";
            string datePart = DateTime.Now.ToString("yyyyMMdd");
            int countToday = _context.HoaDons.Count(h => h.NgayTao.Date == DateTime.Today);
            string numberPart = (countToday + 1).ToString("D3");
            return $"{prefix}{datePart}-{numberPart}";
        }

        private string GetTrangThaiText(int trangThai) =>
            trangThai switch
            {
                0 => "Chờ xác nhận",
                1 => "Đã xác nhận",
                2 => "Đang vận chuyển",
                3 => "Đã thanh toán",
                4 => "Thành công",
                5 => "Đã hủy",

                10=> "Giao hàng không thành công",
                _ => "Không rõ"
            };
    }
}
