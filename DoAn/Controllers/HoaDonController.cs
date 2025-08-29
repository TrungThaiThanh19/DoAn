using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        public HoaDonController(DoAnDbContext context) => _context = context;


        public IActionResult Index(string? loaiHoaDon, int? trangThai)
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

            var list = query
                .OrderByDescending(h => h.NgayTao)
                .AsEnumerable()
                .Select(h =>
                {
                    var lastActor = h.TrangThaiDonHangs?
                        .OrderByDescending(t => t.NgayChuyen)
                        .FirstOrDefault()?.NhanVienDoi;

                    var isOffline = string.Equals(h.LoaiHoaDon?.Trim(), "offline", StringComparison.OrdinalIgnoreCase);
                    var nhanVienTen = isOffline
                        ? (h.NhanVien?.Ten_NhanVien ?? (!string.IsNullOrWhiteSpace(lastActor) ? lastActor : "Không có"))
                        : (!string.IsNullOrWhiteSpace(lastActor) ? lastActor : (h.NhanVien?.Ten_NhanVien ?? "Không có"));

                    // --- dấu hiệu hoàn hàng ---
                    var hasReturnDone = h.TraHangs?.Any(p => p.TrangThai == 3) ?? false; // đã hoàn tiền
                    var hasReturnRequest =
                        (h.TrangThaiDonHangs?.Any(t => t.TrangThai == 6) ?? false) ||    // log "KH bấm hoàn"
                        (h.TraHangs?.Any(t => t.TrangThai == 0 || t.TrangThai == 1 || t.TrangThai == 2) ?? false); // YC/duyệt/đã nhận

                    // --- tiền giảm / tổng tiền như bạn đang làm ---
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
                        TrangThaiText = hasReturnDone ? "Đã hoàn hàng" : GetTrangThaiText(h.TrangThai),
                        HasReturnRequest = hasReturnRequest,
                        HasReturnDone = hasReturnDone
                    };
                })
                .ToList();

            ViewBag.LoaiHoaDon = loaiHoaDon;
            ViewBag.TrangThai = trangThai;
            return View(list);
        }



        // Helper đọc giá gốc theo nhiều tên thuộc tính
        private static decimal? ReadPrice(object? obj, params string[] candidates)
        {
            if (obj == null) return null;
            var t = obj.GetType();
            foreach (var name in candidates)
            {
                var p = t.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                if (p == null) continue;
                var v = p.GetValue(obj);
                if (v == null) continue;
                if (v is decimal d) return d;
                if (decimal.TryParse(v.ToString(), out var parsed)) return parsed;
            }
            return null;
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
                    _ => "Không rõ"
                };

        // ===================== JSON TRẠNG THÁI CHO KHÁCH HÀNG =====================
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
            // Lấy hoá đơn kèm chi tiết + sản phẩm để kiểm tra tồn
            var hoaDon = _context.HoaDons
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(v => v.SanPham)
                .FirstOrDefault(h => h.ID_HoaDon == idHoaDon);

            if (hoaDon == null) return NotFound();

            // Chỉ xử lý đặc biệt khi chuyển 0 -> 1 (giữ hàng bằng cách trừ kho)
            if (hoaDon.TrangThai == 0)
            {
                // Kiểm tra đủ tồn kho cho tất cả dòng
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
                    TempData["Error"] = "Một số sản phẩm không đủ tồn kho: "
                        + string.Join(", ", thieu)
                        + ". Vui lòng nhập thêm hàng hoặc điều chỉnh số lượng.";
                    return RedirectToAction("Details", new { id = idHoaDon });
                }

                // Đủ hàng -> trừ kho + chuyển sang ĐÃ XÁC NHẬN
                foreach (var ct in hoaDon.HoaDonChiTiets)
                {
                    var spct = ct.SanPhamChiTiet
                               ?? _context.SanPhamChiTiets.FirstOrDefault(x => x.ID_SanPhamChiTiet == ct.ID_SanPhamChiTiet);
                    if (spct == null) continue;

                    DecreaseTonKho(spct, ct.SoLuong);
                }

                var old = hoaDon.TrangThai;
                hoaDon.TrangThai = 1; // Đã xác nhận
                hoaDon.NgayCapNhat = DateTime.Now;

                _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
                {
                    ID_TrangThaiDonHang = Guid.NewGuid(),
                    ID_HoaDon = idHoaDon,
                    TrangThai = hoaDon.TrangThai,
                    NgayChuyen = DateTime.Now,
                    NhanVienDoi = User?.Identity?.Name ?? "system",
                    NoiDungDoi = $"Cập nhật: {GetTrangThaiText(old)} -> {GetTrangThaiText(hoaDon.TrangThai)} (đã trừ kho giữ hàng)"
                });

                _context.SaveChanges();
                TempData["Success"] = "Đã xác nhận đơn và trừ kho giữ hàng.";
                return RedirectToAction("Details", new { id = idHoaDon });
            }

            // Các bước khác (1->2->3->4) vẫn giống trước đây
            if (hoaDon.TrangThai < 4)
            {
                var old = hoaDon.TrangThai;
                hoaDon.TrangThai += 1;
                hoaDon.NgayCapNhat = DateTime.Now;

                _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
                {
                    ID_TrangThaiDonHang = Guid.NewGuid(),
                    ID_HoaDon = idHoaDon,
                    TrangThai = hoaDon.TrangThai,
                    NgayChuyen = DateTime.Now,
                    NhanVienDoi = User?.Identity?.Name ?? "system",
                    NoiDungDoi = $"Cập nhật: {GetTrangThaiText(old)} -> {GetTrangThaiText(hoaDon.TrangThai)}"
                });

                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = idHoaDon });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HuyDon(Guid id, string lyDo)
        {
            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy.";
                return RedirectToAction("Details", new { id });
            }

            var hd = _context.HoaDons
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                .FirstOrDefault(h => h.ID_HoaDon == id);

            if (hd == null) return NotFound();

            // CHỈ cho hủy ở 0 (chờ xác nhận) hoặc 1 (đã xác nhận)
            if (hd.TrangThai != 0 && hd.TrangThai != 1)
            {
                TempData["Error"] = "Chỉ hủy đơn ở trạng thái Chờ xác nhận hoặc Đã xác nhận.";
                return RedirectToAction("Details", new { id });
            }

            // Nếu hủy ở bước 1: HOÀN KHO lại (vì đã từng trừ khi xác nhận)
            if (hd.TrangThai == 1)
            {
                foreach (var ct in hd.HoaDonChiTiets)
                {
                    var spct = ct.SanPhamChiTiet;
                    if (spct == null) continue;

                    var t = spct.GetType();
                    var p = t.GetProperty("SoLuongTon")
                          ?? t.GetProperty("SoLuong")
                          ?? t.GetProperty("TonKho")
                          ?? t.GetProperty("SoLuong_TonKho");
                    if (p == null) continue;

                    var cur = Convert.ToInt32(p.GetValue(spct) ?? 0);
                    p.SetValue(spct, cur + ct.SoLuong);
                }
            }
            // Nếu hủy ở bước 0: KHÔNG cộng kho (vì chưa trừ kho)

            // Ghi log
            _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = id,
                TrangThai = 5, // Đã hủy
                NgayChuyen = DateTime.Now,
                NhanVienDoi = User?.Identity?.Name ?? "system",
                NoiDungDoi = $"Hủy đơn ở bước {(hd.TrangThai == 0 ? "Chờ xác nhận" : "Đã xác nhận")}. Lý do: {lyDo.Trim()}"
            });

            // Đổi trạng thái đơn
            hd.TrangThai = 5;
            hd.NgayCapNhat = DateTime.Now;

            _context.SaveChanges();

            TempData["Success"] = "Đã hủy đơn hàng.";
            return RedirectToAction("Details", new { id });
        }


        // ===================== Helpers tồn kho (reflection) =====================
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
    }
}
