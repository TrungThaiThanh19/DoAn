using DoAn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DoAn.Controllers
{
    [Authorize(Roles = "admin")]
    public class CountriesController : Controller
    {
        private readonly DoAnDbContext _context;
        public CountriesController(DoAnDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.QuocGias.AsQueryable();

            // Kiểm tra nếu từ khóa tìm kiếm không rỗng
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(q => q.Ten_QuocGia.Contains(keyword));
            }
            // Sắp xếp theo tên quốc gia
            var quocGias = await query.OrderBy(x => x.Ten_QuocGia).ToListAsync();
            ViewBag.Keyword = keyword;

            return View(quocGias);
        }


        [HttpGet]
        public async Task<IActionResult> Details(Guid idQuocGia)
        {
            var quocGia = await _context.QuocGias.FindAsync(idQuocGia);
            if (quocGia == null)
            {
                return NotFound();
            }
            return View(quocGia);
        }

        [HttpGet]
        public async Task<IActionResult> Update(Guid idQuocGia)
        {
            var quocGia = await _context.QuocGias.FindAsync(idQuocGia);
            if (quocGia == null)
                return NotFound();

            return View(quocGia);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid idQuocGia, string tenQuocGia)
        {
            var quocGia = await _context.QuocGias.FindAsync(idQuocGia);
            if (quocGia == null)
                return NotFound();

            // Xóa lỗi mặc định
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(tenQuocGia))
                ModelState.AddModelError("Ten_QuocGia", "Tên quốc gia không được để trống");

            // Kiểm tra tên quốc gia chỉ được chứa chữ
            else if (!Regex.IsMatch(tenQuocGia, @"^[a-zA-ZÀ-ỹ\s]+$"))
                ModelState.AddModelError("Ten_QuocGia", "Tên quốc gia chỉ được chứa chữ");

            else if (_context.QuocGias.Any(q => q.Ten_QuocGia == tenQuocGia && q.ID_QuocGia != idQuocGia))
                ModelState.AddModelError("Ten_QuocGia", "Tên quốc gia đã tồn tại");

            if (!ModelState.IsValid)
            {
                ViewBag.Ten_QuocGia = tenQuocGia;
                return View(quocGia);
            }

            quocGia.Ten_QuocGia = tenQuocGia;
            _context.QuocGias.Update(quocGia);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { idQuocGia = quocGia.ID_QuocGia });
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string tenQuocGia)
        {
            // Xóa lỗi mặc định
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(tenQuocGia))
            {
                ModelState.AddModelError("TenQuocGia", "Tên quốc gia không được để trống");
            }
            // Kiểm tra tên quốc gia chỉ được chứa chữ
            else if (!Regex.IsMatch(tenQuocGia, @"^[\p{L}\s]+$"))
            {
                ModelState.AddModelError("TenQuocGia", "Tên quốc gia chỉ được chứa chữ cái");
            }
            else if (_context.QuocGias.Any(x => x.Ten_QuocGia.ToLower() == tenQuocGia.Trim().ToLower()))
            {
                ModelState.AddModelError("TenQuocGia", "Tên quốc gia đã tồn tại");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TenQuocGia = tenQuocGia;
                return View();
            }

            var quocGia = new QuocGia
            {
                ID_QuocGia = Guid.NewGuid(),
                Ten_QuocGia = tenQuocGia.Trim()
            };

            _context.QuocGias.Add(quocGia);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}