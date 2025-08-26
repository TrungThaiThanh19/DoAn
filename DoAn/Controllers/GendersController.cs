using DoAn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DoAn.Controllers
{
    [Authorize(Roles = "admin")]
    public class GendersController : Controller
    {
        private readonly DoAnDbContext _context;
        public GendersController(DoAnDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.GioiTinhs.AsQueryable();

            // Kiểm tra nếu từ khóa tìm kiếm không rỗng
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(q => q.Ten_GioiTinh.Contains(keyword));
            }
            // Sắp xếp theo tên quốc gia
            var gioiTinhs = await query.OrderBy(x => x.Ten_GioiTinh).ToListAsync();
            ViewBag.Keyword = keyword;

            return View(gioiTinhs);
        }

        [HttpGet]
        public async Task<IActionResult> Details(Guid idGioiTinh)
        {
            var gioiTinh = await _context.GioiTinhs.FindAsync(idGioiTinh);
            if (gioiTinh == null)
            {
                return NotFound();
            }
            return View(gioiTinh);
        }


        [HttpGet]
        public async Task<IActionResult> Update(Guid idGioiTinh)
        {
            var gioiTinh = await _context.GioiTinhs.FindAsync(idGioiTinh);
            if (gioiTinh == null)
                return NotFound();

            return View(gioiTinh);
        }


        [HttpPost]
        public async Task<IActionResult> Update(Guid idGioiTinh, string tenGioiTinh)
        {
            var gioiTinh = await _context.GioiTinhs.FindAsync(idGioiTinh);
            if (gioiTinh == null)
                return NotFound();

            // Xóa lỗi mặc định
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(tenGioiTinh))
                ModelState.AddModelError("Ten_GioiTinh", "Tên giới tính không được để trống");

            // Kiểm tra tên giới tính chỉ được chứa chữ
            else if (!Regex.IsMatch(tenGioiTinh, @"^[a-zA-ZÀ-ỹ\s]+$"))
                ModelState.AddModelError("Ten_GioiTinh", "Tên giới tính chỉ được chứa chữ");

            else if (_context.GioiTinhs.Any(q => q.Ten_GioiTinh == tenGioiTinh && q.ID_GioiTinh != idGioiTinh))
                ModelState.AddModelError("Ten_GioiTinh", "Tên giới tính đã tồn tại");

            if (!ModelState.IsValid)
            {
                ViewBag.Ten_GioiTinh = tenGioiTinh;
                return View(gioiTinh);
            }

            gioiTinh.Ten_GioiTinh = tenGioiTinh;
            _context.GioiTinhs.Update(gioiTinh);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { idGioiTinh = gioiTinh.ID_GioiTinh });
        }


        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string tenGioiTinh)
        {
            // Xóa lỗi mặc định
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(tenGioiTinh))
            {
                ModelState.AddModelError("TenGioiTinh", "Tên giới tính không được để trống");
            }
            // Kiểm tra tên quốc gia chỉ được chứa chữ
            else if (!Regex.IsMatch(tenGioiTinh, @"^[\p{L}\s]+$"))
            {
                ModelState.AddModelError("TenGioiTinh", "Tên giới tính chỉ được chứa chữ cái");
            }
            else if (_context.GioiTinhs.Any(x => x.Ten_GioiTinh.ToLower() == tenGioiTinh.Trim().ToLower()))
            {
                ModelState.AddModelError("TenGioiTinh", "Tên giới tính đã tồn tại");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.TenGioiTinh = tenGioiTinh;
                return View();
            }

            var gioiTinh = new GioiTinh
            {
                ID_GioiTinh = Guid.NewGuid(),
                Ten_GioiTinh = tenGioiTinh.Trim()
            };

            _context.GioiTinhs.Add(gioiTinh);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}