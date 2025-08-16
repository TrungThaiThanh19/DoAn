using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using DoAn.ViewModels;

namespace DoAn.Controllers
{
    public class TaiKhoanController : Controller
    {
        private readonly DoAnDbContext _context;
        public TaiKhoanController(DoAnDbContext context)
        {
            _context = context;
        }

        // GET: /TaiKhoan/Register
        public IActionResult Register()
        {
            var role = HttpContext.Session.GetString("RoleName");
            if (!string.IsNullOrEmpty(role))
                return RedirectBasedOnRole(role);

            return View();
        }

        // POST: /TaiKhoan/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // Kiểm tra username trùng
            if (await _context.TaiKhoans.AnyAsync(tk => tk.Uername == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
                return View(model);
            }

            // Xác nhận mật khẩu
            if (model.Password != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            // Vai trò khách hàng
            var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Ten_Roles == "khachhang");
            if (customerRole == null)
            {
                ModelState.AddModelError("", "Lỗi hệ thống: Không tìm thấy vai trò khách hàng.");
                return View(model);
            }

            // (Hiện đang dùng plain text)
            var newTaiKhoan = new TaiKhoan
            {
                ID_TaiKhoan = Guid.NewGuid(),
                Uername = model.Username,
                Password = model.Password,
                ID_Roles = customerRole.ID_Roles,
                Roles = customerRole
            };
            _context.TaiKhoans.Add(newTaiKhoan);

            var newKhachHang = new KhachHang
            {
                ID_KhachHang = Guid.NewGuid(),
                Ma_KhachHang = GenerateUniqueCustomerCode(),
                Ten_KhachHang = model.TenKhachHang,
                GioiTinh = model.GioiTinh,
                SoDienThoai = model.SoDienThoai,
                NgaySinh = model.NgaySinh,
                Email = model.Email,
                NgayTao = DateTime.Now,
                TrangThai = 1,
                ID_TaiKhoan = newTaiKhoan.ID_TaiKhoan,
                TaiKhoan = newTaiKhoan
            };
            _context.KhachHangs.Add(newKhachHang);

            await _context.SaveChangesAsync();

            // Thông báo cho trang Login
            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login", "TaiKhoan");
        }

        private string GenerateUniqueCustomerCode()
        {
            return "KH" + DateTime.Now.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        }

        // GET: /TaiKhoan/Login
        public IActionResult Login()
        {
            var role = HttpContext.Session.GetString("RoleName");
            if (!string.IsNullOrEmpty(role))
                return RedirectBasedOnRole(role);

            return View();
        }

        // POST: /TaiKhoan/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var taiKhoan = await _context.TaiKhoans
                                         .Include(tk => tk.Roles)
                                         .FirstOrDefaultAsync(tk => tk.Uername == model.Username);

            if (taiKhoan == null || taiKhoan.Password != model.Password)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // Lưu session
            HttpContext.Session.SetString("UserID", taiKhoan.ID_TaiKhoan.ToString());
            HttpContext.Session.SetString("Username", taiKhoan.Uername);
            HttpContext.Session.SetString("RoleName", taiKhoan.Roles.Ten_Roles);

            // KHÔNG set TempData["SuccessMessage"] cho đăng nhập để tránh tồn tại sau restart
            // Nếu muốn thông báo ở trang đích, bạn có thể set một key khác tại đây,
            // rồi hiển thị ở View của Shop/Admin/Home.

            return RedirectBasedOnRole(taiKhoan.Roles.Ten_Roles);
        }

        // GET: /TaiKhoan/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();     // xoá session
            TempData["SuccessMessage"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Login", "TaiKhoan");
        }

        private IActionResult RedirectBasedOnRole(string roleName)
        {
            switch (roleName)
            {
                case "khachhang": return RedirectToAction("Index", "Shop");
                case "nhanvien": return RedirectToAction("Index", "Admin");
                case "admin": return RedirectToAction("Index", "Admin");
                default: return RedirectToAction("Index", "Home");
            }
        }
    }
}
