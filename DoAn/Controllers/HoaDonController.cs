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
                .AsQueryable();

            if (!string.IsNullOrEmpty(loaiHoaDon))
                query = query.Where(h => h.LoaiHoaDon.ToLower() == loaiHoaDon.ToLower());

            if (trangThai.HasValue)
                query = query.Where(h => h.TrangThai == trangThai.Value);

            var result = query
                .OrderByDescending(h => h.NgayTao)
                .AsEnumerable()
                .Select(h =>
                {
                    var lastActor = h.TrangThaiDonHangs?
                        .OrderByDescending(t => t.NgayChuyen)
                        .FirstOrDefault()?.NhanVienDoi;

                    var isOffline = string.Equals(h.LoaiHoaDon?.Trim(), "offline",
                                                  StringComparison.OrdinalIgnoreCase);

                    var nhanVienTen = isOffline
                        ? (h.NhanVien?.Ten_NhanVien
                            ?? (!string.IsNullOrWhiteSpace(lastActor) ? lastActor : "Không có"))
                        : (!string.IsNullOrWhiteSpace(lastActor)
                            ? lastActor
                            : (h.NhanVien?.Ten_NhanVien ?? "Không có"));

                    return new HoaDonViewModel
                    {
                        ID_HoaDon = h.ID_HoaDon,
                        Ma_HoaDon = h.Ma_HoaDon,
                        HoTen = h.HoTen ?? "Khách lẻ",
                        NhanVienTen = nhanVienTen,
                        LoaiHoaDon = h.LoaiHoaDon,
                        NgayTao = h.NgayTao,
                        TongTienSauGiam = h.TongTienSauGiam,
                        TienGiam = h.TongTienTruocGiam - h.TongTienSauGiam,
                        TrangThai = h.TrangThai,
                        TrangThaiText = GetTrangThaiText(h.TrangThai)
                    };
                })
                .ToList();

            ViewBag.LoaiHoaDon = loaiHoaDon;
            ViewBag.TrangThai = trangThai;

            return View(result);
        }

        private string GetTrangThaiText(int trangThai) =>
            trangThai switch
            {
                0 => "Chờ xác nhận",
                1 => "Đã xác nhận",
                2 => "Đang vận chuyển",
                3 => "Đã thanh toán",
                4 => "Hoàn thành",
                5 => "Đã hủy",
                _ => "Không rõ"
            };

        // ==== JSON: khách hàng auto refresh trạng thái (kèm lý do hủy nếu có) ====
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Status(Guid id)
        {
            var hd = _context.HoaDons.AsNoTracking().FirstOrDefault(x => x.ID_HoaDon == id);
            if (hd == null) return NotFound();

            // Lấy bản ghi hủy mới nhất để trả về lý do & người thực hiện
            Models.TrangThaiDonHang? cancelLog = null;
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
                text = GetTrangThaiText(hd.TrangThai),
                updatedAt = (hd.NgayCapNhat ?? hd.NgayTao).ToString("yyyy-MM-dd HH:mm:ss"),
                cancel = hd.TrangThai == 5,
                cancelReason = cancelLog?.NoiDungDoi,
                cancelBy = cancelLog?.NhanVienDoi,
                cancelAt = cancelLog?.NgayChuyen.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        public IActionResult Details(Guid id)
        {
            var hoaDon = _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(v => v.SanPham)
                .Include(h => h.TrangThaiDonHangs)
                .FirstOrDefault(h => h.ID_HoaDon == id);

            if (hoaDon == null) return NotFound();
            return View(hoaDon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CapNhatTrangThai(Guid idHoaDon)
        {
            var hoaDon = _context.HoaDons.FirstOrDefault(h => h.ID_HoaDon == idHoaDon);
            if (hoaDon == null) return NotFound();

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

        // === HỦY ĐƠN: cho phép ở BẤT KỲ trạng thái (trừ khi đã hủy) ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult HuyDon(Guid id, string lyDo)
        {
            if (string.IsNullOrWhiteSpace(lyDo))
            {
                TempData["Error"] = "Vui lòng nhập lý do hủy.";
                return RedirectToAction("Details", new { id });
            }

            var hd = _context.HoaDons.FirstOrDefault(h => h.ID_HoaDon == id);
            if (hd == null) return NotFound();

            if (hd.TrangThai != 0) // 0 = Chờ xác nhận
            {
                TempData["Error"] = "Chỉ hủy đơn ở trạng thái Chờ xác nhận.";
                return RedirectToAction("Details", new { id });
            }

            // HOÀN KHO: cộng trả số lượng cho từng biến thể sản phẩm
            var lines = _context.HoaDonChiTiets
                .Include(ct => ct.SanPhamChiTiet)
                .Where(ct => ct.ID_HoaDon == id)
                .ToList();

            foreach (var ct in lines)
            {
                if (ct.SanPhamChiTiet == null) continue;

                // tìm field tồn kho theo tên phổ biến: SoLuongTon / SoLuong / TonKho / SoLuong_TonKho
                var spct = ct.SanPhamChiTiet;
                var t = spct.GetType();
                var p = t.GetProperty("SoLuongTon")
                        ?? t.GetProperty("SoLuong")
                        ?? t.GetProperty("TonKho")
                        ?? t.GetProperty("SoLuong_TonKho");
                if (p != null)
                {
                    var cur = Convert.ToInt32(p.GetValue(spct) ?? 0);
                    p.SetValue(spct, cur + ct.SoLuong);
                }
            }

            // Log lý do hủy
            var log = new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = id,
                TrangThai = 5,
                NgayChuyen = DateTime.Now,
                NhanVienDoi = User?.Identity?.Name ?? "system",
                NoiDungDoi = lyDo.Trim()
            };
            _context.TrangThaiDonHangs.Add(log);

            // Đổi trạng thái đơn
            hd.TrangThai = 5;
            hd.NgayCapNhat = DateTime.Now;

            _context.SaveChanges();
            TempData["Success"] = "Đã hủy đơn hàng.";
            return RedirectToAction("Details", new { id });
        }


    }
}
