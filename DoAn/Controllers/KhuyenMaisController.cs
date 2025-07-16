using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DoAn.Models;

namespace DoAn.Controllers
{
    public class KhuyenMaisController : Controller
    {
        private readonly DoAnDbContext _context;

        public KhuyenMaisController(DoAnDbContext context)
        {
            _context = context;
        }

        // GET: /KhuyenMais
        public async Task<IActionResult> Index()
        {
            var list = await _context.KhuyenMais
                .OrderByDescending(k => k.NgayBatDau)
                .ToListAsync();

            return View(list);
        }

        // GET: /KhuyenMais/Details/{id}
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                TempData["Error"] = "❌ Không tìm thấy ID khuyến mãi.";
                return RedirectToAction(nameof(Index));
            }

            var km = await _context.KhuyenMais.FindAsync(id);
            if (km == null)
            {
                TempData["Error"] = "❌ Không tồn tại khuyến mãi này.";
                return RedirectToAction(nameof(Index));
            }

            return View(km);
        }

        // GET: /KhuyenMais/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /KhuyenMais/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhuyenMai km)
        {
            // Kiểm tra ngày
            if (km.NgayHetHan < km.NgayBatDau)
            {
                ModelState.AddModelError("NgayHetHan", "⚠️ Ngày hết hạn phải lớn hơn hoặc bằng ngày bắt đầu.");
            }

            // Kiểm tra mã khuyến mãi trùng
            if (_context.KhuyenMais.Any(k => k.Ma_KhuyenMai == km.Ma_KhuyenMai))
            {
                ModelState.AddModelError("Ma_KhuyenMai", "⚠️ Mã khuyến mãi đã tồn tại.");
            }

            if (!ModelState.IsValid)
                return View(km);

            try
            {
                km.ID_KhuyenMai = Guid.NewGuid();
                _context.KhuyenMais.Add(km);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Tạo khuyến mãi thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"❌ Lỗi khi lưu: {ex.Message}");
                return View(km);
            }
        }

        // GET: /KhuyenMais/Edit/{id}
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var km = await _context.KhuyenMais.FindAsync(id);
            if (km == null) return NotFound();

            return View(km);
        }

        // POST: /KhuyenMais/Edit/{id}
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, KhuyenMai km)
        {
            if (id != km.ID_KhuyenMai)
            {
                return NotFound();
            }

            if (km.NgayHetHan < km.NgayBatDau)
            {
                ModelState.AddModelError("NgayHetHan", "⚠️ Ngày hết hạn phải lớn hơn hoặc bằng ngày bắt đầu.");
            }

            if (_context.KhuyenMais.Any(k => k.Ma_KhuyenMai == km.Ma_KhuyenMai && k.ID_KhuyenMai != id))
            {
                ModelState.AddModelError("Ma_KhuyenMai", "⚠️ Mã khuyến mãi đã tồn tại.");
            }

            if (!ModelState.IsValid)
                return View(km);

            try
            {
                _context.Update(km);
                await _context.SaveChangesAsync();
                TempData["Success"] = "✅ Cập nhật thành công.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.KhuyenMais.Any(e => e.ID_KhuyenMai == id))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"❌ Lỗi cập nhật: {ex.Message}");
                return View(km);
            }
        }

        // GET: /KhuyenMais/Delete/{id}
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
                return NotFound();

            var km = await _context.KhuyenMais.FindAsync(id);
            if (km == null)
                return NotFound();

            return View(km);
        }

        // POST: /KhuyenMais/Delete/{id}
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var km = await _context.KhuyenMais.FindAsync(id);
            if (km == null)
                return NotFound();

            try
            {
                _context.KhuyenMais.Remove(km);
                await _context.SaveChangesAsync();
                TempData["Success"] = "🗑️ Xóa khuyến mãi thành công.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Lỗi khi xóa: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
