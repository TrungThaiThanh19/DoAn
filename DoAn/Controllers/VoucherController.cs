using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    public class VoucherController : Controller
    {
        private readonly DoAnDbContext _context;
        public VoucherController(DoAnDbContext context)
        {
            _context = context;
        }

        // Danh sách voucher
        public async Task<IActionResult> Index()
        {
            var list = await _context.Vouchers
                .Include(v => v.TaiKhoan)
                .ToListAsync();
            return View(list);
        }

        // Tạo mới
        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Voucher model)
        {
            if (ModelState.IsValid)
            {
                // chuẩn hóa mã
                model.Ma_Voucher = model.Ma_Voucher.Trim().ToUpper();
                model.ID_Voucher = Guid.NewGuid();
                model.NgayTao = DateTime.Now;
                model.TrangThai = 1;

                _context.Vouchers.Add(model);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tạo voucher thành công!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // Sửa voucher
        public async Task<IActionResult> Edit(Guid id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();
            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Voucher model)
        {
            if (id != model.ID_Voucher) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(model);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật voucher thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    return NotFound();
                }
            }
            return View(model);
        }

        // Xóa voucher
        public async Task<IActionResult> Delete(Guid id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();
            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Xóa voucher thành công!";
            return RedirectToAction(nameof(Index));
        }
    }
}

