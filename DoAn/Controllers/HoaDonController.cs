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
                .AsQueryable();

            if (!string.IsNullOrEmpty(loaiHoaDon))
                query = query.Where(h => h.LoaiHoaDon.ToLower() == loaiHoaDon.ToLower());

            if (trangThai.HasValue)
                query = query.Where(h => h.TrangThai == trangThai.Value);

            var result = query
                .OrderByDescending(h => h.NgayTao)
                .AsEnumerable()
                .Select(h => new HoaDonViewModel
                {
                    ID_HoaDon = h.ID_HoaDon,
                    Ma_HoaDon = h.Ma_HoaDon,
                    HoTen = h.HoTen ?? "Khách lẻ",
                    NhanVienTen = h.NhanVien != null ? h.NhanVien.Ten_NhanVien : "Không có",
                    LoaiHoaDon = h.LoaiHoaDon,
                    NgayTao = h.NgayTao,
                    TongTienSauGiam = h.TongTienSauGiam,
                    TienGiam = h.TongTienTruocGiam - h.TongTienSauGiam,
                    TrangThai = h.TrangThai,
                    TrangThaiText = GetTrangThaiText(h.TrangThai)
                }).ToList();

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
                .FirstOrDefault(h => h.ID_HoaDon == id);

            if (hoaDon == null) return NotFound();
            return View(hoaDon);
        }

        [HttpPost]
        public IActionResult CapNhatTrangThai(Guid idHoaDon)
        {
            var hoaDon = _context.HoaDons.FirstOrDefault(h => h.ID_HoaDon == idHoaDon);
            if (hoaDon == null) return NotFound();

            if (hoaDon.TrangThai < 4)
            {
                hoaDon.TrangThai += 1;
                hoaDon.NgayCapNhat = DateTime.Now;
                _context.SaveChanges();
            }
            return RedirectToAction("Details", new { id = idHoaDon });
        }
    }
}
