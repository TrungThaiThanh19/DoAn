using DoAn.Models;
using DoAn.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    public class HoaDonController : Controller
    {
        private readonly DoAnDbContext _context;
        public HoaDonController(DoAnDbContext context)
        {
            _context = context;
        }
        // GET: HoaDonController

        public IActionResult Index(string? loaiHoaDon, int? trangThai)
        {
            var query = _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .AsQueryable();

            // Lọc theo loại hóa đơn
            if (!string.IsNullOrEmpty(loaiHoaDon))
            {
                query = query.Where(h => h.LoaiHoaDon.ToLower() == loaiHoaDon.ToLower());
            }

            // Lọc theo trạng thái
            if (trangThai.HasValue)
            {
                query = query.Where(h => h.TrangThai == trangThai.Value);
            }

            // Chỉ EF xử lý đến đây → chuyển sang xử lý C#
            var result = query
                .OrderByDescending(h => h.NgayTao)
                .AsEnumerable() // Chuyển sang xử lý trong bộ nhớ
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

        private string GetTrangThaiText(int trangThai)
        {
            return trangThai switch
            {
                0 => "Chờ xác nhận",
                1 => "Xác nhận",
                2 => "Vận chuyển",
                3 => "Thanh toán",
                4 => "Hoàn thành",
                5 => "Hủy",
                _ => "Không rõ"
            };
        }

        public IActionResult Details(Guid id)
        {
            var hoaDon = _context.HoaDons
                .Include(h => h.KhachHang)
                .Include(h => h.NhanVien)
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                    .ThenInclude(v=>v.SanPham)
                .FirstOrDefault(h => h.ID_HoaDon == id);

            if (hoaDon == null) return NotFound();

            return View(hoaDon);
        }

        // ===============================
        // CẬP NHẬT TRẠNG THÁI (POST FORM)
        // ===============================
        [HttpPost]
        public IActionResult CapNhatTrangThai(Guid idHoaDon)
        {
            var hoaDon = _context.HoaDons.FirstOrDefault(h => h.ID_HoaDon == idHoaDon);
            if (hoaDon == null) return NotFound();

            // Tăng trạng thái lên 1 nếu chưa hoàn tất
            if (hoaDon.TrangThai < 4)
            {
                hoaDon.TrangThai += 1;
                hoaDon.NgayCapNhat = DateTime.Now;

                // Ghi lại lịch sử nếu có bảng trạng thái (tuỳ bạn)
                // _context.TrangThaiDonHangs.Add(new TrangThaiDonHang { ... });

                _context.SaveChanges();
            }

            return RedirectToAction("Details", new { id = idHoaDon });
        }
    }
}
