using DoAn.Models;
using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers
{
    public class AdminController : Controller
    {
        private readonly DoAnDbContext _context;
        public AdminController(DoAnDbContext context)
        {
            _context = context;
        }
        // GET: AdminController
        public IActionResult Index()
        {
            var roleName = HttpContext.Session.GetString("RoleName");
            if (roleName != "admin" && roleName != "nhanvien") // Cho phép cả admin và nhân viên vào trang này
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang quản trị.";
                return RedirectToAction("Login", "TaiKhoan");
            }
            return View();
        }
    }
}