// Controllers/KhuyenMaiController.cs
using DoAn.IService;
using DoAn.Models;
using DoAn.Service.IService;
using DoAn.ViewModel;
using DoAn.ViewModels.KhuyenMaiVM;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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

        // Hàm tiện ích: trả về thời gian hiện tại theo giờ VN (UTC+7)
        private static DateTime Now() => DateTime.UtcNow.AddHours(7);

        // Hàm tiện ích: kiểm tra kiểu giảm giá có phải % không
        private static bool IsPercent(string kieu) =>
            string.Equals(kieu?.Trim(), "percent", StringComparison.OrdinalIgnoreCase);

        // Hàm tiện ích: kiểm tra kiểu giảm giá có phải số tiền cố định không
        private static bool IsFixed(string kieu) =>
            string.Equals(kieu?.Trim(), "fixed", StringComparison.OrdinalIgnoreCase);

        // ===== LIST =====
        // Trang danh sách khuyến mãi
        public async Task<IActionResult> Index(string? q)
        {
            var now = Now();
            var list = await _kmService.GetAllAsync(q?.Trim());

            // Ánh xạ dữ liệu từ model ra ViewModel để hiển thị danh sách
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
                SoSPCT = x.ChiTietKhuyenMais?.Count ?? 0, // số SPCT đang áp dụng
                DangHoatDong = x.TrangThai == 1 && now >= x.NgayBatDau && now <= x.NgayHetHan // check còn hiệu lực
            })
            .OrderByDescending(i => i.NgayBatDau)
            .ToList();

            ViewBag.Query = q;
            return View(model);
        }

        // ===== CREATE =====
        // GET: form tạo khuyến mãi
        public async Task<IActionResult> Create()
        {
            await LoadSPCTListAsync(); // load toàn bộ SPCT
            await LoadBrandListAsync(); // load danh sách thương hiệu

            // Trả về form với giá trị mặc định
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

        // POST: tạo khuyến mãi
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhuyenMaiFormVM m)
        {
            await ValidateFormAsync(m); // validate các rule cơ bản

            // Nếu chọn thương hiệu => không bắt buộc chọn SPCT
            if (m.ThuongHieuId.HasValue)
            {
                ModelState.Remove(nameof(m.SanPhamChiTietIds));
            }
            else if (m.SanPhamChiTietIds == null || !m.SanPhamChiTietIds.Any())
            {
                // Nếu không chọn thương hiệu thì phải có ít nhất 1 SPCT
                ModelState.AddModelError(nameof(m.SanPhamChiTietIds),
                    "Hãy chọn thương hiệu hoặc chọn ít nhất một SPCT.");
            }

            if (!ModelState.IsValid)
            {
                // reload lại dữ liệu cho combobox khi form có lỗi
                await LoadSPCTListAsync(m.SanPhamChiTietIds);
                await LoadBrandListAsync(m.ThuongHieuId);
                return View(m);
            }

            // Xác định danh sách SPCT được áp dụng
            HashSet<Guid> targetIds = m.ThuongHieuId.HasValue
                ? await GetSpctIdsByBrandAsync(m.ThuongHieuId.Value) // nếu chọn thương hiệu => lấy toàn bộ SPCT thuộc brand đó
                : new HashSet<Guid>(m.SanPhamChiTietIds ?? Enumerable.Empty<Guid>());

            // Tạo entity mới
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

            await _kmService.AddAsync(km, targetIds); // gọi service để thêm

            TempData["Success"] = "Tạo khuyến mãi thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ===== EDIT =====
        // GET: form sửa khuyến mãi
        public async Task<IActionResult> Edit(Guid id)
        {
            var km = await _kmService.GetByIdAsync(id);
            if (km == null) return NotFound();

            // Đổ dữ liệu ra form VM
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
                ThuongHieuId = null
            };

            await LoadSPCTListAsync(vm.SanPhamChiTietIds);
            await LoadBrandListAsync(vm.ThuongHieuId);
            return View(vm);
        }

        // POST: cập nhật khuyến mãi
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KhuyenMaiFormVM m)
        {
            await ValidateFormAsync(m);

            // Điều kiện tối thiểu giống Create
            if (m.ThuongHieuId.HasValue)
            {
                ModelState.Remove(nameof(m.SanPhamChiTietIds));
            }
            else if (m.SanPhamChiTietIds == null || !m.SanPhamChiTietIds.Any())
            {
                ModelState.AddModelError(nameof(m.SanPhamChiTietIds),
                    "Hãy chọn thương hiệu hoặc chọn ít nhất một SPCT.");
            }

            if (!ModelState.IsValid)
            {
                await LoadSPCTListAsync(m.SanPhamChiTietIds);
                await LoadBrandListAsync(m.ThuongHieuId);
                return View(m);
            }

            var exist = await _kmService.GetByIdAsync(m.ID_KhuyenMai!.Value);
            if (exist == null) return NotFound();

            // cập nhật entity
            exist.Ma_KhuyenMai = m.Ma_KhuyenMai.Trim();
            exist.Ten_KhuyenMai = m.Ten_KhuyenMai;
            exist.KieuGiamGia = m.KieuGiamGia;
            exist.GiaTriGiam = m.GiaTriGiam;
            exist.GiaTriToiDa = m.GiaTriToiDa;
            exist.MoTa = m.MoTa ?? "";
            exist.NgayBatDau = m.NgayBatDau;
            exist.NgayHetHan = m.NgayHetHan;
            exist.TrangThai = m.TrangThai;

            // Xác định lại phạm vi SPCT
            HashSet<Guid> targetIds = m.ThuongHieuId.HasValue
                ? await GetSpctIdsByBrandAsync(m.ThuongHieuId.Value)
                : new HashSet<Guid>(m.SanPhamChiTietIds ?? Enumerable.Empty<Guid>());

            await _kmService.UpdateAsync(exist, targetIds);

            TempData["Success"] = "Cập nhật khuyến mãi thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ======= AJAX API: Lọc SPCT theo thương hiệu (dùng cho View để load SPCT động) =======
        [HttpGet]
        public async Task<IActionResult> SpctByBrand(Guid? brandId)
        {
            var q = _db.SanPhamChiTiets
                .AsNoTracking()
                .Include(s => s.SanPham)
                .Include(s => s.TheTich)
                .AsQueryable();

            if (brandId.HasValue)
                q = q.Where(s => s.SanPham.ID_ThuongHieu == brandId.Value);

            var items = await q
                .OrderByDescending(s => s.NgayTao)
                .Select(s => new
                {
                    id = s.ID_SanPhamChiTiet,
                    text = s.SanPham.Ten_SanPham + " - " + s.TheTich.GiaTri + s.TheTich.DonVi +
                           $" (Giá: {s.GiaBan:N0} đ)"
                })
                .ToListAsync();

            return Json(items); // trả JSON để client-side JS nạp vào combobox SPCT
        }

        // ===== TOGGLE =====
        // Bật / tắt trạng thái khuyến mãi
        [HttpPost]
        public async Task<IActionResult> Toggle(Guid id)
        {
            await _kmService.ToggleAsync(id);
            return RedirectToAction(nameof(Index));
        }

        // ===== DELETE =====
        // Xóa khuyến mãi
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _kmService.DeleteAsync(id);
            TempData["Success"] = "Đã xóa khuyến mãi.";
            return RedirectToAction(nameof(Index));
        }

        // ================= Helpers =================

        // Lấy toàn bộ SPCT của một thương hiệu (dùng khi chọn brand)
        private async Task<HashSet<Guid>> GetSpctIdsByBrandAsync(Guid thuongHieuId)
        {
            var ids = await _db.SanPhamChiTiets
                .AsNoTracking()
                .Where(spct => spct.SanPham.ID_ThuongHieu == thuongHieuId)
                .Select(spct => spct.ID_SanPhamChiTiet)
                .ToListAsync();

            return new HashSet<Guid>(ids);
        }

        // Load danh sách SPCT ra ViewBag để binding vào MultiSelectList
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

        // Load danh sách thương hiệu ra combobox
        private async Task LoadBrandListAsync(Guid? selected = null)
        {
            var brands = await _db.ThuongHieus
                .AsNoTracking()
                .OrderBy(x => x.Ten_ThuongHieu)
                .Select(x => new { x.ID_ThuongHieu, x.Ten_ThuongHieu })
                .ToListAsync();

            ViewBag.BrandList = new SelectList(brands, "ID_ThuongHieu", "Ten_ThuongHieu", selected);
        }

        // Validate dữ liệu nhập form khuyến mãi
        private async Task ValidateFormAsync(KhuyenMaiFormVM m)
        {
            // Check mã trùng
            var trungMa = await _db.KhuyenMais
                .AnyAsync(x => x.Ma_KhuyenMai == m.Ma_KhuyenMai && x.ID_KhuyenMai != m.ID_KhuyenMai);
            if (trungMa)
                ModelState.AddModelError(nameof(m.Ma_KhuyenMai), "Mã khuyến mãi đã tồn tại.");

            // Check ngày hợp lệ
            if (m.NgayHetHan <= m.NgayBatDau)
                ModelState.AddModelError(nameof(m.NgayHetHan), "Ngày hết hạn phải sau ngày bắt đầu.");

            // Check logic giảm giá
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