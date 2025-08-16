using DoAn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    [Authorize(Roles = "admin")]
    public class VolumesController : Controller
    {
        private readonly DoAnDbContext _context;
        public VolumesController(DoAnDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.TheTichs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                // Chuẩn hóa keyword
                keyword = keyword.Trim().ToLower();

                query = query.Where(t =>
                    t.Ma_TheTich.ToLower().Contains(keyword) ||
                    t.DonVi.ToLower().Contains(keyword) ||
                    t.GiaTri.ToString().Replace(".", ",").Contains(keyword) ||
                    t.GiaTri.ToString().Contains(keyword)
                );
            }

            var result = await query.OrderBy(t => t.GiaTri).ToListAsync();
            ViewBag.Keyword = keyword;
            return View(result);
        }



        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var theTich = await _context.TheTichs.FindAsync(id);
            if (theTich == null) return NotFound();
            return View(theTich);
        }


        [HttpGet]
        public async Task<IActionResult> Update(Guid id)
        {
            var theTich = await _context.TheTichs.FindAsync(id);
            if (theTich == null) return NotFound();
            return View(theTich);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid id, string maTheTich, string giaTri, string donVi, int trangThai)
        {
            var theTich = await _context.TheTichs.FindAsync(id);
            if (theTich == null) return NotFound();

            ModelState.Clear();
            decimal giaTriParse = 0;

            if (string.IsNullOrWhiteSpace(maTheTich))
                ModelState.AddModelError("MaTheTich", "Mã thể tích không được để trống");

            else if (_context.TheTichs.Any(t => t.Ma_TheTich == maTheTich && t.ID_TheTich != id))
                ModelState.AddModelError("MaTheTich", "Mã thể tích đã tồn tại");

            if (string.IsNullOrWhiteSpace(giaTri))
                ModelState.AddModelError("GiaTri", "Giá trị không được để trống");

            else if (giaTri.Contains('.'))
                ModelState.AddModelError("GiaTri", "Chỉ sử dụng dấu phẩy (,) cho số thập phân");

            else if (!decimal.TryParse(giaTri, out giaTriParse))
                ModelState.AddModelError("GiaTri", "Giá trị không hợp lệ");

            else if (giaTriParse <= 0)
                ModelState.AddModelError("GiaTri", "Giá trị phải > 0");

            if (string.IsNullOrWhiteSpace(donVi))
                ModelState.AddModelError("DonVi", "Đơn vị không được để trống");

            if (ModelState.IsValid && _context.TheTichs.Any(t =>
                t.GiaTri == giaTriParse &&
                t.DonVi.ToLower() == donVi.ToLower() &&
                t.ID_TheTich != id))
            {
                ModelState.AddModelError("GiaTri", "Giá trị này đã tồn tại với đơn vị tương ứng");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MaTheTich = maTheTich;
                ViewBag.GiaTri = giaTri;
                ViewBag.DonVi = donVi;
                ViewBag.TrangThai = trangThai;
                return View(theTich);
            }

            theTich.Ma_TheTich = maTheTich.Trim();
            theTich.GiaTri = giaTriParse;
            theTich.DonVi = donVi.Trim();
            theTich.TrangThai = trangThai;

            _context.Update(theTich);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { id = theTich.ID_TheTich });
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(string maTheTich, string giaTri, string donVi)
        {
            ModelState.Clear();
            decimal giaTriParse = 0;

            if (string.IsNullOrWhiteSpace(maTheTich))
                ModelState.AddModelError("MaTheTich", "Mã thể tích không được để trống");

            if (_context.TheTichs.Any(t => t.Ma_TheTich == maTheTich))
                ModelState.AddModelError("MaTheTich", "Mã thể tích đã tồn tại");

            if (string.IsNullOrWhiteSpace(giaTri))
                ModelState.AddModelError("GiaTri", "Giá trị không được để trống");

            else if (giaTri.Contains('.'))
                ModelState.AddModelError("GiaTri", "Chỉ sử dụng dấu phẩy (,) cho số thập phân");

            else if (!decimal.TryParse(giaTri, out giaTriParse))
                ModelState.AddModelError("GiaTri", "Giá trị không hợp lệ");

            else if (giaTriParse <= 0)
                ModelState.AddModelError("GiaTri", "Giá trị phải > 0");

            if (string.IsNullOrWhiteSpace(donVi))
                ModelState.AddModelError("DonVi", "Đơn vị không được để trống");

            if (ModelState.IsValid && _context.TheTichs.Any(t =>
                t.GiaTri == giaTriParse &&
                t.DonVi.ToLower() == donVi.ToLower()))
            {
                ModelState.AddModelError("GiaTri", "Giá trị này đã tồn tại với đơn vị tương ứng");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.MaTheTich = maTheTich;
                ViewBag.GiaTri = giaTri;
                ViewBag.DonVi = donVi;
                return View();
            }

            var newItem = new TheTich
            {
                ID_TheTich = Guid.NewGuid(),
                Ma_TheTich = maTheTich.Trim(),
                GiaTri = giaTriParse,
                DonVi = donVi.Trim(),
                TrangThai = 1
            };

            _context.TheTichs.Add(newItem);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}