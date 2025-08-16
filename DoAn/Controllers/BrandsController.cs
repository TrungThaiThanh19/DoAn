using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace DoAn.Controllers
{
    public class BrandsController : Controller
    {
        private readonly DoAnDbContext _context;
        public BrandsController(DoAnDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        public async Task<IActionResult> Index(string keyword)
        {
            var query = _context.ThuongHieus.AsQueryable();

            // Kiểm tra nếu từ khóa tìm kiếm không rỗng
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(q => q.Ten_ThuongHieu.Contains(keyword) || q.Ma_ThuongHieu.Contains(keyword));
            }
            // Sắp xếp theo tên thương hiệu
            var thuongHieus = await query.OrderBy(x => x.Ten_ThuongHieu).ToListAsync();
            ViewBag.Keyword = keyword;

            return View(thuongHieus);
        }


        [HttpGet]
        public async Task<IActionResult> Details(Guid idThuongHieu)
        {
            var thuongHieu = await _context.ThuongHieus.FindAsync(idThuongHieu);
            if (thuongHieu == null)
                return NotFound();

            return View(thuongHieu);
        }

        [HttpGet]
        public async Task<IActionResult> Update(Guid idThuongHieu)
        {
            var thuongHieu = await _context.ThuongHieus.FindAsync(idThuongHieu);
            if (thuongHieu == null)
                return NotFound();

            return View(thuongHieu);
        }

        [HttpPost]
        public async Task<IActionResult> Update(Guid idThuongHieu, string maThuongHieu, string tenThuongHieu, int trangThai)
        {
            var thuongHieu = await _context.ThuongHieus.FindAsync(idThuongHieu);
            if (thuongHieu == null)
                return NotFound();

            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(maThuongHieu))
                ModelState.AddModelError("MaThuongHieu", "Mã thương hiệu không được để trống");

            else if (_context.ThuongHieus.Any(x => x.Ma_ThuongHieu == maThuongHieu && x.ID_ThuongHieu != idThuongHieu))
                ModelState.AddModelError("MaThuongHieu", "Mã thương hiệu đã tồn tại");

            if (string.IsNullOrWhiteSpace(tenThuongHieu))
                ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu không được để trống");

            else if (!Regex.IsMatch(tenThuongHieu, @"^[\p{L}\s]+$"))
                ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu chỉ được chứa chữ cái");

            else if (_context.ThuongHieus.Any(x => x.Ten_ThuongHieu.ToLower() == tenThuongHieu.Trim().ToLower() && x.ID_ThuongHieu != idThuongHieu))
                ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu đã tồn tại");

            if (!ModelState.IsValid)
            {
                ViewBag.MaThuongHieu = maThuongHieu;
                ViewBag.TenThuongHieu = tenThuongHieu;
                ViewBag.TrangThai = trangThai;
                return View(thuongHieu);
            }

            thuongHieu.Ma_ThuongHieu = maThuongHieu;
            thuongHieu.Ten_ThuongHieu = tenThuongHieu.Trim();
            thuongHieu.TrangThai = trangThai;

            _context.ThuongHieus.Update(thuongHieu);
            await _context.SaveChangesAsync();

            return RedirectToAction("Details", new { idThuongHieu = thuongHieu.ID_ThuongHieu });
        }



        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string maThuongHieu, string tenThuongHieu)
        {
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(maThuongHieu))
                ModelState.AddModelError("MaThuongHieu", "Mã thương hiệu không được để trống");

            else if (_context.ThuongHieus.Any(x => x.Ma_ThuongHieu.ToLower() == maThuongHieu.Trim().ToLower()))
                ModelState.AddModelError("MaThuongHieu", "Mã thương hiệu đã tồn tại");

            if (string.IsNullOrWhiteSpace(tenThuongHieu))
                ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu không được để trống");

            else if (!Regex.IsMatch(tenThuongHieu, @"^[\p{L}\s]+$"))
                ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu chỉ được chứa chữ cái");

            else if (_context.ThuongHieus.Any(x => x.Ten_ThuongHieu.ToLower() == tenThuongHieu.Trim().ToLower()))
                ModelState.AddModelError("TenThuongHieu", "Tên thương hiệu đã tồn tại");

            if (!ModelState.IsValid)
            {
                ViewBag.MaThuongHieu = maThuongHieu;
                ViewBag.TenThuongHieu = tenThuongHieu;
                return View();
            }

            var thuongHieu = new ThuongHieu
            {
                ID_ThuongHieu = Guid.NewGuid(),
                Ma_ThuongHieu = maThuongHieu.Trim(),
                Ten_ThuongHieu = tenThuongHieu.Trim(),
                TrangThai = 1
            };

            _context.ThuongHieus.Add(thuongHieu);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }

    }
}