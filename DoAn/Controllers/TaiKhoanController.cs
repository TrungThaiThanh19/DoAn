using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
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
        [AllowAnonymous]
        public IActionResult Register()
        {
            var role = HttpContext.Session.GetString("RoleName");
            if (!string.IsNullOrEmpty(role))
                return RedirectBasedOnRole(role);

            return View();
        }

        // POST: /TaiKhoan/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.SoDienThoai))
            {
                model.SoDienThoai = System.Text.RegularExpressions.Regex
                    .Replace(model.SoDienThoai, @"\D", "");   // chỉ còn số
                if (model.SoDienThoai.Length > 10)
                    model.SoDienThoai = model.SoDienThoai[..10];
            }
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

            TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login", "TaiKhoan");
        }

        private string GenerateUniqueCustomerCode()
        {
            return "KH" + DateTime.Now.ToString("yyyyMMddHHmmss") + Guid.NewGuid().ToString().Substring(0, 4).ToUpper();
        }

        // GET: /TaiKhoan/Login
        [AllowAnonymous]
        public IActionResult Login()
        {
            var role = HttpContext.Session.GetString("RoleName");
            if (!string.IsNullOrEmpty(role))
                return RedirectBasedOnRole(role);

            return View();
        }
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var taiKhoan = await _context.TaiKhoans
                .Include(tk => tk.Roles)
                .Include(tk => tk.KhachHangs)
                .Include(tk => tk.NhanViens)
                .FirstOrDefaultAsync(tk => tk.Uername == model.Username);

            if (taiKhoan == null || taiKhoan.Password != model.Password)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // 🔒 Check trạng thái (khách hàng)
            if (taiKhoan.Roles.Ten_Roles == "khachhang")
            {
                var kh = taiKhoan.KhachHangs.FirstOrDefault();
                if (kh != null && kh.TrangThai == 0)
                {
                    ModelState.AddModelError("", "Tài khoản khách hàng đã bị khóa.");
                    return View(model);
                }
            }

            // 🔒 Check trạng thái (nhân viên + admin)
            if (taiKhoan.Roles.Ten_Roles == "nhanvien" || taiKhoan.Roles.Ten_Roles == "admin")
            {
                var nv = taiKhoan.NhanViens.FirstOrDefault();
                if (nv != null && nv.TrangThai == 0)
                {
                    ModelState.AddModelError("", "Tài khoản nhân viên đã bị khóa.");
                    return View(model);
                }
            }

            // ===== TẠO COOKIE LOGIN =====
            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, taiKhoan.ID_TaiKhoan.ToString()),
        new Claim(ClaimTypes.Name, taiKhoan.Uername),
        new Claim(ClaimTypes.Role, taiKhoan.Roles.Ten_Roles)
    };
            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // ===== LƯU SESSION =====
            HttpContext.Session.SetString("UserID", taiKhoan.ID_TaiKhoan.ToString());
            HttpContext.Session.SetString("Username", taiKhoan.Uername);
            HttpContext.Session.SetString("RoleName", taiKhoan.Roles.Ten_Roles);

            // 👉 Nếu là nhân viên thì lưu thêm tên nhân viên
            if (taiKhoan.Roles.Ten_Roles == "nhanvien")
            {
                var nv = taiKhoan.NhanViens.FirstOrDefault();
                if (nv != null)
                    HttpContext.Session.SetString("StaffName", nv.Ten_NhanVien);
            }

            return RedirectBasedOnRole(taiKhoan.Roles.Ten_Roles);
        }



        // GET: /TaiKhoan/Logout
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Bạn đã đăng xuất.";
            return RedirectToAction("Login", "TaiKhoan");
        }

        [AllowAnonymous]
        public IActionResult Denied() => View(); // trang Access Denied

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


        // GET: /TaiKhoan/ForgotPassword
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /TaiKhoan/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            // 1. Tìm trong bảng KhachHang
            var khachHang = await _context.KhachHangs
                .Include(kh => kh.TaiKhoan)
                .FirstOrDefaultAsync(kh =>
                    kh.Email == model.Email &&
                    kh.SoDienThoai == model.SoDienThoai);

            // 2. Nếu không có, tìm trong bảng NhanVien
            var nhanVien = khachHang == null
                ? await _context.NhanViens
                    .Include(nv => nv.TaiKhoan)
                    .FirstOrDefaultAsync(nv =>
                        nv.Email == model.Email &&
                        nv.SoDienThoai == model.SoDienThoai)
                : null;

            if (khachHang == null && nhanVien == null)
            {
                ModelState.AddModelError("", "Thông tin không chính xác. Vui lòng kiểm tra lại.");
                return View(model);
            }

            // 3. Xác nhận mật khẩu mới trùng nhau
            if (model.NewPassword != model.ConfirmPassword)
            {
                ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp.");
                return View(model);
            }

            // 4. Cập nhật mật khẩu (hiện tại đang lưu plain text)
            if (khachHang != null)
                khachHang.TaiKhoan.Password = model.NewPassword;
            else if (nhanVien != null)
                nhanVien.TaiKhoan.Password = model.NewPassword;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login", "TaiKhoan");
        }


    }
}
