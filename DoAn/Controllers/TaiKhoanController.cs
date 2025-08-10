using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System;
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
        public IActionResult Register() // <-- Đổi tên Action từ DangKy thành Register
        {
            // Nếu đã đăng nhập rồi, chuyển hướng về trang phù hợp
            if (HttpContext.Session.GetString("RoleName") != null)
            {
                return RedirectBasedOnRole(HttpContext.Session.GetString("RoleName"));
            }
            return View(); // Tên View sẽ tự động là Register.cshtml
        }

        // POST: /TaiKhoan/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model) // <-- Đổi tên Action từ DangKy thành Register
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                if (await _context.TaiKhoans.AnyAsync(tk => tk.Uername == model.Username))
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại. Vui lòng chọn tên khác.");
                    return View(model);
                }

                // Kiểm tra xác nhận mật khẩu
                if (model.Password != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                    return View(model);
                }

                // 1. Tìm vai trò 'khachhang'
                var customerRole = await _context.Roles.FirstOrDefaultAsync(r => r.Ten_Roles == "khachhang");
                if (customerRole == null)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: Không tìm thấy vai trò khách hàng. Vui lòng liên hệ quản trị viên.");
                    return View(model);
                }

                // 2. Không mã hóa mật khẩu - Lưu mật khẩu plain text
                string plainTextPassword = model.Password;

                // 3. Tạo tài khoản mới
                var newTaiKhoan = new TaiKhoan
                {
                    ID_TaiKhoan = Guid.NewGuid(),
                    Uername = model.Username,
                    Password = plainTextPassword,
                    ID_Roles = customerRole.ID_Roles,
                    Roles = customerRole
                };

                _context.TaiKhoans.Add(newTaiKhoan);

                // 4. Tạo thông tin Khách hàng tương ứng
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

                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login", "TaiKhoan"); // <-- Chuyển hướng về Action Login của TaiKhoanController
            }
            return View(model);
        }

        // Hàm giả định để tạo mã khách hàng duy nhất
        private string GenerateUniqueCustomerCode()
        {
            return "KH" + DateTime.Now.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        }

        // --- ĐĂNG NHẬP (Action Login, View Login.cshtml) ---

        // GET: /TaiKhoan/Login
        public IActionResult Login() // <-- Đổi tên Action từ DangNhap thành Login
        {
            // Nếu đã đăng nhập rồi, chuyển hướng về trang phù hợp
            if (HttpContext.Session.GetString("RoleName") != null)
            {
                return RedirectBasedOnRole(HttpContext.Session.GetString("RoleName"));
            }
            return View(); // Tên View sẽ tự động là Login.cshtml
        }

        // POST: /TaiKhoan/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model) // <-- Đổi tên Action từ DangNhap thành Login
        {
            if (ModelState.IsValid)
            {
                // Tìm tài khoản theo tên đăng nhập và tải thông tin vai trò
                var taiKhoan = await _context.TaiKhoans
                                            .Include(tk => tk.Roles)
                                            .FirstOrDefaultAsync(tk => tk.Uername == model.Username);

                // So sánh trực tiếp mật khẩu dạng văn bản thường
                if (taiKhoan == null || taiKhoan.Password != model.Password)
                {
                    ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                    return View(model);
                }

                // Đăng nhập thành công, lưu thông tin vào Session
                HttpContext.Session.SetString("UserID", taiKhoan.ID_TaiKhoan.ToString());
                HttpContext.Session.SetString("Username", taiKhoan.Uername);
                HttpContext.Session.SetString("RoleName", taiKhoan.Roles.Ten_Roles);

                TempData["SuccessMessage"] = "Đăng nhập thành công!";
                // Chuyển hướng dựa trên vai trò
                return RedirectBasedOnRole(taiKhoan.Roles.Ten_Roles);
            }
            return View(model);
        }

        // --- ĐĂNG XUẤT (Action Logout) ---
        // GET: /TaiKhoan/Logout
        public IActionResult Logout() // <-- Đổi tên Action từ DangXuat thành Logout (nếu bạn muốn)
        {
            HttpContext.Session.Clear(); // Xóa tất cả session
            TempData["SuccessMessage"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Login", "TaiKhoan"); // <-- Chuyển hướng về Action Login của TaiKhoanController
        }

        // Hàm trợ giúp để chuyển hướng dựa trên tên vai trò
        private IActionResult RedirectBasedOnRole(string roleName)
        {
            switch (roleName)
            {
                case "khachhang":
                    return RedirectToAction("Index", "Shop");
                case "nhanvien":
                    return RedirectToAction("Dashboard", "Employee");
                case "admin":
                    return RedirectToAction("Index", "Admin");
                default:
                    return RedirectToAction("Index", "Home");
            }
        }
    }
}