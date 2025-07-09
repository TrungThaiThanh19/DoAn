using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers
{
    public class AdminController : Controller
    {
        // GET: AdminController
        public IActionResult Index()
        {
            return View();
        }
    }
}
