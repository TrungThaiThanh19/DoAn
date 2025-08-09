using DoAn.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    public class SanPhamKHController : Controller
    {
        private readonly DoAnDbContext _context;
        public SanPhamKHController(DoAnDbContext context)
        {
            _context = context;
        }
        // GET: SanPhamKHController
        public ActionResult Index()
        {
            var listproduct = _context.SanPhams
                .Include(p => p.GioiTinh)
                .Include(p => p.QuocGia)
                .Include(p => p.ThuongHieu)
                .Include(p => p.SanPhamChiTiets)
                //.Where(p  => p.SanPhamChiTiets.Any(sp => sp.SoLuong > 0))
                .ToList();
            return View(listproduct);
        }

        // GET: SanPhamKHController/Details/5
        public ActionResult Details(Guid id)
        {
            var product = _context.SanPhams
                .Include(p => p.GioiTinh)
                .Include(p => p.QuocGia)
                .Include(p => p.ThuongHieu)
                .Include(p => p.SanPhamChiTiets)
                .ThenInclude(sp => sp.TheTich)
                .FirstOrDefault(p => p.ID_SanPham == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
