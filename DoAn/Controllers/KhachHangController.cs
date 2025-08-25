using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn.Models;
using BCrypt.Net;
using System;
using System.Linq;

namespace DoAn.Controllers
{
    public class KhachHangController : Controller
    {
        private readonly DoAnDbContext _context;

        public KhachHangController(DoAnDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị danh sách khách hàng
        public IActionResult Index(string search)
        {
            IQueryable<KhachHang> query = _context.KhachHangs
                .Include(kh => kh.TaiKhoan);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(kh =>
                    kh.Ten_KhachHang.Contains(search) ||
                    kh.Email.Contains(search) ||
                    kh.SoDienThoai.Contains(search));
            }

            var danhSach = query.ToList();
            return View(danhSach);
        }

        // GET: Edit
        public IActionResult Edit(Guid id)
        {
            var kh = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefault(k => k.ID_KhachHang == id);

            if (kh == null)
                return NotFound();

            return View(kh);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, KhachHang model, string NewPassword)
        {
            // Bỏ qua validation cho các field không cần thiết
            ModelState.Remove("Ma_KhachHang");
            ModelState.Remove("TaiKhoan.Password");
            ModelState.Remove("TaiKhoan.Roles");
            ModelState.Remove("TaiKhoan.NhanViens");
            ModelState.Remove("TaiKhoan.KhachHangs");
            ModelState.Remove("TaiKhoan.Vouchers");
            ModelState.Remove("NewPassword");

            if (!ModelState.IsValid)
                return View(model);

            var kh = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefault(k => k.ID_KhachHang == id);

            if (kh == null || kh.TaiKhoan == null)
                return NotFound();

            // Cập nhật thông tin cá nhân
            kh.Ten_KhachHang = model.Ten_KhachHang;
            kh.Email = model.Email;
            kh.SoDienThoai = model.SoDienThoai;
            kh.GioiTinh = model.GioiTinh;
            kh.NgaySinh = model.NgaySinh;
            kh.TrangThai = model.TrangThai;

            // Cập nhật username (giữ nguyên là Uername)
            kh.TaiKhoan.Uername = model.TaiKhoan.Uername;

            // Chỉ cập nhật mật khẩu nếu có nhập mật khẩu mới
            if (!string.IsNullOrEmpty(NewPassword))
            {
                kh.TaiKhoan.Password = BCrypt.Net.BCrypt.HashPassword(NewPassword);
            }

            try
            {
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                return View(model);
            }
        }

        // GET: Xem chi tiết khách hàng
        public IActionResult Details(Guid id)
        {
            var kh = _context.KhachHangs
                .Include(kh => kh.TaiKhoan)
                .FirstOrDefault(kh => kh.ID_KhachHang == id);

            if (kh == null)
                return NotFound();

            return View(kh);
        }

        // GET: Xác nhận khóa khách hàng
        public IActionResult Khoa(Guid id)
        {
            var kh = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefault(k => k.ID_KhachHang == id);

            if (kh == null)
                return NotFound();

            return View(kh);
        }

        // POST: Khóa khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult KhoaConfirmed(Guid id)
        {
            var kh = _context.KhachHangs.Find(id);
            if (kh == null)
                return NotFound();

            kh.TrangThai = 0; // 0 = bị khóa
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Mở khóa khách hàng
        public IActionResult MoKhoa(Guid id)
        {
            var kh = _context.KhachHangs
                .Include(k => k.TaiKhoan)
                .FirstOrDefault(k => k.ID_KhachHang == id);

            if (kh == null)
                return NotFound();

            return View(kh);
        }

        // POST: Mở khóa khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult MoKhoaConfirmed(Guid id)
        {
            var kh = _context.KhachHangs.Find(id);
            if (kh == null)
                return NotFound();

            kh.TrangThai = 1; // 1 = hoạt động
            _context.SaveChanges();

            return RedirectToAction("Index");
        }

        // GET: Lấy danh sách khách hàng JSON (dùng cho tìm kiếm hoặc API nội bộ)
        [HttpGet]
        public IActionResult GetKhachHang(string search)
        {
            IQueryable<KhachHang> query = _context.KhachHangs
                .Include(kh => kh.TaiKhoan);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(kh =>
                    kh.Ten_KhachHang.Contains(search) ||
                    kh.Email.Contains(search) ||
                    kh.SoDienThoai.Contains(search));
            }

            var data = query.Select(kh => new
            {
                id_KhachHang = kh.ID_KhachHang,
                ten_KhachHang = kh.Ten_KhachHang,
                email = kh.Email,
                soDienThoai = kh.SoDienThoai,
                trangThai = kh.TrangThai,
                uername = kh.TaiKhoan.Uername
            }).ToList();

            return Json(data);
        }
    }
}