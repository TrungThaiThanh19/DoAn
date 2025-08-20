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
                .Include(h => h.TrangThaiDonHangs) // cần lịch sử để lấy người đổi gần nhất
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
                    // Người đổi trạng thái gần nhất từ lịch sử
                    var lastActor = h.TrangThaiDonHangs?
                        .OrderByDescending(t => t.NgayChuyen)
                        .FirstOrDefault()?.NhanVienDoi;

                    var isOffline = string.Equals(h.LoaiHoaDon?.Trim(), "offline",
                                                  StringComparison.OrdinalIgnoreCase);

                    // OFFLINE: ưu tiên nhân viên bán hàng; ONLINE: ưu tiên người đổi gần nhất
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
                        NhanVienTen = nhanVienTen,   // <-- cột "Nhân viên"
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

        // ==== JSON: khách hàng auto refresh trạng thái ====
        [HttpGet]
        [AllowAnonymous] // nếu muốn bắt đăng nhập thì đổi thành [Authorize]
        public IActionResult Status(Guid id)
        {
            var hd = _context.HoaDons.AsNoTracking().FirstOrDefault(x => x.ID_HoaDon == id);
            if (hd == null) return NotFound();

            return Json(new
            {
                ok = true,
                id = hd.ID_HoaDon,
                status = hd.TrangThai,
                text = GetTrangThaiText(hd.TrangThai),
                updatedAt = (hd.NgayCapNhat ?? hd.NgayTao).ToString("yyyy-MM-dd HH:mm:ss")
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

                // Ghi log để biết ai là người đổi (dùng cho cột "Nhân viên")
                _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
                {
                    ID_TrangThaiDonHang = Guid.NewGuid(),
                    ID_HoaDon = idHoaDon,
                    TrangThai = hoaDon.TrangThai,              // trạng thái mới
                    NgayChuyen = DateTime.Now,
                    NhanVienDoi = User?.Identity?.Name ?? "system",
                    NoiDungDoi = $"Cập nhật: {GetTrangThaiText(old)} -> {GetTrangThaiText(hoaDon.TrangThai)}"
                });

                _context.SaveChanges();
            }
            return RedirectToAction("Details", new { id = idHoaDon });
        }

        // === HỦY ĐƠN: chỉ cho phép khi đang "Chờ xác nhận" (0) ===
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

            // Cập nhật trạng thái đơn -> ĐÃ HỦY (5) và log lý do vào TrangThaiDonHang.NoiDungDoi
            var log = new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = id,
                TrangThai = 5, // Đã hủy
                NgayChuyen = DateTime.Now,
                NhanVienDoi = User?.Identity?.Name ?? "system",
                NoiDungDoi = lyDo.Trim()
            };
            _context.TrangThaiDonHangs.Add(log);

            hd.TrangThai = 5;              // chuyển sang ĐÃ HỦY
            hd.NgayCapNhat = DateTime.Now;

            _context.SaveChanges();
            TempData["Success"] = "Đã hủy đơn hàng.";
            return RedirectToAction("Details", new { id });
        }
    }
}
