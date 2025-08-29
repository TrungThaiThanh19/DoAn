// Controllers/KhuyenMaiController.cs
using DoAn.IService;
using DoAn.Models;
using DoAn.Service.IService;
using DoAn.ViewModel;
using DoAn.ViewModels.KhuyenMaiVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    public class KhuyenMaiController : Controller
    {
        private readonly IKhuyenMaiService _kmService;
        private readonly DoAnDbContext _db;

        public KhuyenMaiController(IKhuyenMaiService kmService, DoAnDbContext db)
        {
            _kmService = kmService;
            _db = db;
        }

        private static DateTime Now() => DateTime.UtcNow.AddHours(7);
        private static bool IsPercent(string kieu) =>
            string.Equals(kieu?.Trim(), "percent", StringComparison.OrdinalIgnoreCase);
        private static bool IsFixed(string kieu) =>
            string.Equals(kieu?.Trim(), "fixed", StringComparison.OrdinalIgnoreCase);

        // ===== LIST =====
        public async Task<IActionResult> Index(string? q)
        {
            var now = Now();
            var list = await _kmService.GetAllAsync(q?.Trim());

            var model = list.Select(x => new KhuyenMaiIndexItemVM
            {
                ID_KhuyenMai = x.ID_KhuyenMai,
                Ma_KhuyenMai = x.Ma_KhuyenMai,
                Ten_KhuyenMai = x.Ten_KhuyenMai,
                KieuGiamGia = x.KieuGiamGia,
                GiaTriGiam = x.GiaTriGiam,
                GiaTriToiDa = x.GiaTriToiDa,
                NgayBatDau = x.NgayBatDau,
                NgayHetHan = x.NgayHetHan,
                TrangThai = x.TrangThai,
                SoSPCT = x.ChiTietKhuyenMais?.Count ?? 0,
                DangHoatDong = x.TrangThai == 1 && now >= x.NgayBatDau && now <= x.NgayHetHan
            })
            .OrderByDescending(i => i.NgayBatDau)
            .ToList();

            ViewBag.Query = q;
            return View(model);
        }

        // ===== CREATE =====
        public async Task<IActionResult> Create()
        {
            await LoadSPCTListAsync();
            await LoadBrandListAsync(); // nạp brand
            return View(new KhuyenMaiFormVM
            {
                NgayBatDau = Now(),
                NgayHetHan = Now().AddDays(7),
                KieuGiamGia = "percent",
                GiaTriGiam = 10,
                GiaTriToiDa = 0,
                TrangThai = 1
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhuyenMaiFormVM m)
        {
            await ValidateFormAsync(m);

            // Nếu không chọn brand và cũng không chọn SPCT => báo lỗi
            if (m.ThuongHieuId == null && (m.SanPhamChiTietIds == null || !m.SanPhamChiTietIds.Any()))
                ModelState.AddModelError(nameof(m.SanPhamChiTietIds), "Hãy chọn thương hiệu hoặc chọn ít nhất một SPCT.");

            if (!ModelState.IsValid)
            {
                await LoadSPCTListAsync(m.SanPhamChiTietIds);
                await LoadBrandListAsync(m.ThuongHieuId);
                return View(m);
            }

            // Gom SPCT theo brand + chọn tay
            var targetIds = new HashSet<Guid>(m.SanPhamChiTietIds ?? Enumerable.Empty<Guid>());
            if (m.ThuongHieuId.HasValue)
            {
                var byBrand = await _db.SanPhamChiTiets
                    .AsNoTracking()
                    .Include(spct => spct.SanPham)
                    .Where(spct => spct.SanPham.ID_ThuongHieu == m.ThuongHieuId.Value)
                    .Select(spct => spct.ID_SanPhamChiTiet)
                    .ToListAsync();
                foreach (var id in byBrand) targetIds.Add(id);
            }

            var km = new KhuyenMai
            {
                ID_KhuyenMai = m.ID_KhuyenMai ?? Guid.NewGuid(),
                Ma_KhuyenMai = m.Ma_KhuyenMai.Trim(),
                Ten_KhuyenMai = m.Ten_KhuyenMai,
                KieuGiamGia = m.KieuGiamGia,
                GiaTriGiam = m.GiaTriGiam,
                GiaTriToiDa = m.GiaTriToiDa,
                MoTa = m.MoTa ?? "",
                NgayBatDau = m.NgayBatDau,
                NgayHetHan = m.NgayHetHan,
                TrangThai = m.TrangThai
            };

            await _kmService.AddAsync(km, targetIds);

            TempData["Success"] = "Tạo khuyến mãi thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ===== EDIT =====
        public async Task<IActionResult> Edit(Guid id)
        {
            var km = await _kmService.GetByIdAsync(id);
            if (km == null) return NotFound();

            var vm = new KhuyenMaiFormVM
            {
                ID_KhuyenMai = km.ID_KhuyenMai,
                Ma_KhuyenMai = km.Ma_KhuyenMai,
                Ten_KhuyenMai = km.Ten_KhuyenMai,
                KieuGiamGia = km.KieuGiamGia,
                GiaTriGiam = km.GiaTriGiam,
                GiaTriToiDa = km.GiaTriToiDa,
                MoTa = km.MoTa,
                NgayBatDau = km.NgayBatDau,
                NgayHetHan = km.NgayHetHan,
                TrangThai = km.TrangThai,
                SanPhamChiTietIds = km.ChiTietKhuyenMais?.Select(c => c.ID_SanPhamChiTiet).ToList() ?? new(),
                ThuongHieuId = null // default
            };

            await LoadSPCTListAsync(vm.SanPhamChiTietIds);
            await LoadBrandListAsync(vm.ThuongHieuId);
            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KhuyenMaiFormVM m)
        {
            await ValidateFormAsync(m);

            if (m.ThuongHieuId == null && (m.SanPhamChiTietIds == null || !m.SanPhamChiTietIds.Any()))
                ModelState.AddModelError(nameof(m.SanPhamChiTietIds), "Hãy chọn thương hiệu hoặc chọn ít nhất một SPCT.");

            if (!ModelState.IsValid)
            {
                await LoadSPCTListAsync(m.SanPhamChiTietIds);
                await LoadBrandListAsync(m.ThuongHieuId);
                return View(m);
            }

            var exist = await _kmService.GetByIdAsync(m.ID_KhuyenMai!.Value);
            if (exist == null) return NotFound();

            exist.Ma_KhuyenMai = m.Ma_KhuyenMai.Trim();
            exist.Ten_KhuyenMai = m.Ten_KhuyenMai;
            exist.KieuGiamGia = m.KieuGiamGia;
            exist.GiaTriGiam = m.GiaTriGiam;
            exist.GiaTriToiDa = m.GiaTriToiDa;
            exist.MoTa = m.MoTa ?? "";
            exist.NgayBatDau = m.NgayBatDau;
            exist.NgayHetHan = m.NgayHetHan;
            exist.TrangThai = m.TrangThai;

            var targetIds = new HashSet<Guid>(m.SanPhamChiTietIds ?? Enumerable.Empty<Guid>());
            if (m.ThuongHieuId.HasValue)
            {
                var byBrand = await _db.SanPhamChiTiets
                    .AsNoTracking()
                    .Include(spct => spct.SanPham)
                    .Where(spct => spct.SanPham.ID_ThuongHieu == m.ThuongHieuId.Value)
                    .Select(spct => spct.ID_SanPhamChiTiet)
                    .ToListAsync();
                foreach (var id in byBrand) targetIds.Add(id);
            }

            await _kmService.UpdateAsync(exist, targetIds);

            TempData["Success"] = "Cập nhật khuyến mãi thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ===== TOGGLE =====
        [HttpPost]
        public async Task<IActionResult> Toggle(Guid id)
        {
            await _kmService.ToggleAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ===== DELETE =====
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _kmService.DeleteAsync(id);
            TempData["Success"] = "Đã xóa khuyến mãi.";
            return RedirectToAction(nameof(Index));
        }

        // ================= Helpers =================
        private async Task LoadSPCTListAsync(IEnumerable<Guid>? selected = null)
        {
            var spcts = await _db.SanPhamChiTiets
                .AsNoTracking()
                .Include(s => s.SanPham)
                .Include(s => s.TheTich)
                .OrderByDescending(s => s.NgayTao)
                .Select(s => new
                {
                    s.ID_SanPhamChiTiet,
                    Ten = s.SanPham.Ten_SanPham + " - " + s.TheTich.GiaTri + s.TheTich.DonVi + $" (Giá: {s.GiaBan:N0} đ)"
                })
                .ToListAsync();

            var selectedIds = (selected ?? Enumerable.Empty<Guid>()).ToArray();

            ViewBag.SanPhamChiTietList = new MultiSelectList(
                spcts,
                "ID_SanPhamChiTiet",
                "Ten",
                selectedIds
            );
        }

        private async Task LoadBrandListAsync(Guid? selected = null)
        {
            var brands = await _db.ThuongHieus
                .AsNoTracking()
                .OrderBy(x => x.Ten_ThuongHieu)
                .Select(x => new { x.ID_ThuongHieu, x.Ten_ThuongHieu })
                .ToListAsync();

            ViewBag.BrandList = new SelectList(brands, "ID_ThuongHieu", "Ten_ThuongHieu", selected);
        }

        private async Task ValidateFormAsync(KhuyenMaiFormVM m)
        {
            var trungMa = await _db.KhuyenMais
                .AnyAsync(x => x.Ma_KhuyenMai == m.Ma_KhuyenMai && x.ID_KhuyenMai != m.ID_KhuyenMai);
            if (trungMa)
                ModelState.AddModelError(nameof(m.Ma_KhuyenMai), "Mã khuyến mãi đã tồn tại.");

            if (m.NgayHetHan <= m.NgayBatDau)
                ModelState.AddModelError(nameof(m.NgayHetHan), "Ngày hết hạn phải sau ngày bắt đầu.");

            if (IsPercent(m.KieuGiamGia))
            {
                if (m.GiaTriGiam <= 0 || m.GiaTriGiam > 100)
                    ModelState.AddModelError(nameof(m.GiaTriGiam), "Giảm % phải trong (0..100].");
            }
            else if (IsFixed(m.KieuGiamGia))
            {
                if (m.GiaTriGiam <= 0)
                    ModelState.AddModelError(nameof(m.GiaTriGiam), "Giảm tiền phải > 0.");
            }
        }
    }
}