using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace DoAn.Controllers
{
    public class VouchersController : Controller
    {
        private readonly DoAnDbContext _context;

        public VouchersController(DoAnDbContext context)
        {
            _context = context;
        }

        // GET: Vouchers
        public async Task<IActionResult> Index()
        {
            var vouchers = await _context.Vouchers
                .Include(v => v.TaiKhoan)
                .ToListAsync();
            return View(vouchers);
        }

        // GET: Vouchers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.Vouchers
                .Include(v => v.TaiKhoan)
                .FirstOrDefaultAsync(v => v.ID_Voucher == id);

            return voucher == null ? NotFound() : View(voucher);
        }

        // GET: Vouchers/Create
        public IActionResult Create()
        {
            LoadTaiKhoanDropdown();
            return View();
        }

        // POST: Vouchers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Ma_Voucher,Ten_Voucher,NgayTao,NgayHetHan,KieuGiamGia,GiaTriGiam,GiaTriToiThieu,GiaTriToiDa,SoLuong,TrangThai,MoTa,ID_TaiKhoan")] Voucher voucher)
        {
            // Validate nghiệp vụ
            if (voucher.NgayTao >= voucher.NgayHetHan)
            {
                ModelState.AddModelError(nameof(voucher.NgayHetHan), "⚠️ Ngày hết hạn phải lớn hơn ngày tạo.");
            }

            if (voucher.GiaTriGiam > voucher.GiaTriToiDa)
            {
                ModelState.AddModelError(nameof(voucher.GiaTriGiam), "⚠️ Giá trị giảm không được lớn hơn giá trị tối đa.");
            }

            if (voucher.GiaTriToiThieu > voucher.GiaTriToiDa)
            {
                ModelState.AddModelError(nameof(voucher.GiaTriToiThieu), "⚠️ Giá trị tối thiểu không được lớn hơn giá trị tối đa.");
            }

            if (ModelState.IsValid)
            {
                voucher.ID_Voucher = Guid.NewGuid();
                _context.Add(voucher);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            LoadTaiKhoanDropdown(voucher.ID_TaiKhoan);
            return View(voucher);
        }

        // GET: Vouchers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null) return NotFound();

            LoadTaiKhoanDropdown(voucher.ID_TaiKhoan);
            return View(voucher);
        }

        // POST: Vouchers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("ID_Voucher,Ma_Voucher,Ten_Voucher,NgayTao,NgayHetHan,KieuGiamGia,GiaTriGiam,GiaTriToiThieu,GiaTriToiDa,SoLuong,TrangThai,MoTa,ID_TaiKhoan")] Voucher voucher)
        {
            if (id != voucher.ID_Voucher) return NotFound();

            if (voucher.NgayTao >= voucher.NgayHetHan)
                ModelState.AddModelError("", "⚠️ Ngày hết hạn phải lớn hơn ngày tạo.");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.ID_Voucher)) return NotFound();
                    throw;
                }
            }

            LoadTaiKhoanDropdown(voucher.ID_TaiKhoan);
            return View(voucher);
        }

        // GET: Vouchers/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null) return NotFound();

            var voucher = await _context.Vouchers
                .Include(v => v.TaiKhoan)
                .FirstOrDefaultAsync(v => v.ID_Voucher == id);

            return voucher == null ? NotFound() : View(voucher);
        }

        // POST: Vouchers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }

            // ✅ Không xóa, chỉ cập nhật trạng thái thành "Hết hạn"
            voucher.TrangThai = (int)Voucher.TrangThaiVoucher.HetHan;

            _context.Update(voucher);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private void LoadTaiKhoanDropdown(Guid? selectedId = null)
        {
            var taiKhoans = _context.TaiKhoans
                .Select(t => new { t.ID_TaiKhoan, t.Uername })
                .ToList();

            ViewData["ID_TaiKhoan"] = new SelectList(taiKhoans, "ID_TaiKhoan", "Username", selectedId);
        }

        private bool VoucherExists(Guid id)
        {
            return _context.Vouchers.Any(v => v.ID_Voucher == id);
        }
    }
}