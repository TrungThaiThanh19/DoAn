using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers
{
    public class EmployeeController : Controller
    {
        public IActionResult Dashboard() // Action này sẽ hiển thị trang lựa chọn POS/Online
        {
            // Kiểm tra phân quyền: Đảm bảo chỉ nhân viên mới có thể truy cập trang này
            var roleName = HttpContext.Session.GetString("RoleName");
            if (roleName != "nhanvien")
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập trang này.";
                return RedirectToAction("Login", "TaiKhoan");
            }
            return View(); // Trả về View Views/Employee/Dashboard.cshtml
        }
    }
}
