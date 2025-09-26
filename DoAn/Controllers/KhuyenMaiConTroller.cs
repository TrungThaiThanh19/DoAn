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
        private readonly IKhuyenMaiService _kmService; // Service quản lý khuyến mãi (CRUD)
        private readonly DoAnDbContext _db;            // DbContext để truy vấn dữ liệu

        public KhuyenMaiController(IKhuyenMaiService kmService, DoAnDbContext db)
        {
            _kmService = kmService;
            _db = db;
        }

        // ================= Helpers =================

        // Trả về thời gian hiện tại theo giờ VN (UTC+7)
        private static DateTime Now() => DateTime.UtcNow.AddHours(7);

        // Kiểm tra kiểu giảm giá có phải % không
        private static bool IsPercent(string kieu) =>
            string.Equals(kieu?.Trim(), "percent", StringComparison.OrdinalIgnoreCase);

        // Kiểm tra kiểu giảm giá có phải số tiền cố định không
        private static bool IsFixed(string kieu) =>
            string.Equals(kieu?.Trim(), "fixed", StringComparison.OrdinalIgnoreCase);

        // Lấy danh sách ID SPCT theo thương hiệu
        private async Task<HashSet<Guid>> GetSpctIdsByBrandAsync(Guid thuongHieuId)
        {
            var ids = await _db.SanPhamChiTiets
                .AsNoTracking()
                .Where(spct => spct.SanPham.ID_ThuongHieu == thuongHieuId)
                .Select(spct => spct.ID_SanPhamChiTiet)
                .ToListAsync();

            return new HashSet<Guid>(ids);
        }

        // ================= Validate form =================
        private async Task ValidateFormAsync(KhuyenMaiFormVM m)
        {
            // 1. Check mã khuyến mãi trùng
            var trungMa = await _db.KhuyenMais
                .AnyAsync(x => x.Ma_KhuyenMai == m.Ma_KhuyenMai && x.ID_KhuyenMai != m.ID_KhuyenMai);
            if (trungMa)
                ModelState.AddModelError(nameof(m.Ma_KhuyenMai), "Mã khuyến mãi đã tồn tại.");

            // 2. Check ngày bắt đầu < ngày hết hạn
            if (m.NgayHetHan <= m.NgayBatDau)
                ModelState.AddModelError(nameof(m.NgayHetHan), "Ngày hết hạn phải sau ngày bắt đầu.");

            // 3. Check giá trị giảm hợp lệ
            if (IsPercent(m.KieuGiamGia))
            {
                // % chỉ cho phép (0..50]
                if (m.GiaTriGiam <= 0 || m.GiaTriGiam > 50)
                    ModelState.AddModelError(nameof(m.GiaTriGiam), "Giảm % phải trong (0..50].");
            }
            else if (IsFixed(m.KieuGiamGia))
            {
                // Tiền giảm phải > 0
                if (m.GiaTriGiam <= 0)
                    ModelState.AddModelError(nameof(m.GiaTriGiam), "Giảm tiền phải > 0.");
            }

            // 4. Nếu giảm theo tiền, không được lớn hơn giá nhập nhỏ nhất
            if (ModelState.IsValid && IsFixed(m.KieuGiamGia))
            {
                // Xác định SPCT áp dụng: theo thương hiệu hoặc theo danh sách chọn
                var targetIds = m.ThuongHieuId.HasValue
                    ? await GetSpctIdsByBrandAsync(m.ThuongHieuId.Value)
                    : new HashSet<Guid>(m.SanPhamChiTietIds ?? Enumerable.Empty<Guid>());

                if (targetIds.Any())
                {
                    // Lấy giá nhập nhỏ nhất trong nhóm SPCT
                    var minGiaNhap = await _db.SanPhamChiTiets
                        .Where(spct => targetIds.Contains(spct.ID_SanPhamChiTiet))
                        .MinAsync(spct => spct.GiaNhap);

                    // Nếu giảm nhiều hơn giá nhập → báo lỗi
                    if (m.GiaTriGiam > minGiaNhap)
                    {
                        ModelState.AddModelError(nameof(m.GiaTriGiam),
                            $"Số tiền giảm ({m.GiaTriGiam:N0} đ) không được lớn hơn giá nhập thấp nhất ({minGiaNhap:N0} đ).");
                    }
                }
            }
        }

        // ================= Action: Index =================
        public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = 10)
        {
            var now = Now();
            var list = await _kmService.GetAllAsync(q?.Trim());

            // Map dữ liệu sang ViewModel cho view Index
            var all = list.Select(x => new KhuyenMaiIndexItemVM
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

            // Xử lý phân trang
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            var total = all.Count;
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);
            if (page > totalPages && totalPages > 0) page = totalPages;

            var model = all.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Truyền dữ liệu qua ViewBag
            ViewBag.Query = q;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;
            ViewBag.Total = total;
            ViewBag.TotalPages = totalPages;
            return View(model);
        }

        // ================= Action: Create =================
        public async Task<IActionResult> Create()
        {
            // Load danh sách SPCT + thương hiệu cho dropdown
            await LoadSPCTListAsync();
            await LoadBrandListAsync();

            // Trả về form Create với giá trị mặc định
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

            // Nếu chọn thương hiệu → bỏ check SPCT
            if (m.ThuongHieuId.HasValue)
            {
                ModelState.Remove(nameof(m.SanPhamChiTietIds));
            }
            // Nếu không chọn thương hiệu → bắt buộc chọn SPCT
            else if (m.SanPhamChiTietIds == null || !m.SanPhamChiTietIds.Any())
            {
                ModelState.AddModelError(nameof(m.SanPhamChiTietIds),
                    "Hãy chọn thương hiệu hoặc chọn ít nhất một SPCT.");
            }

            // Nếu lỗi → load lại dropdown + hiển thị form
            if (!ModelState.IsValid)
            {
                await LoadSPCTListAsync(m.SanPhamChiTietIds);
                await LoadBrandListAsync(m.ThuongHieuId);
                return View(m);
            }

            // Xác định danh sách SPCT áp dụng
            HashSet<Guid> targetIds = m.ThuongHieuId.HasValue
                ? await GetSpctIdsByBrandAsync(m.ThuongHieuId.Value)
                : new HashSet<Guid>(m.SanPhamChiTietIds ?? Enumerable.Empty<Guid>());

            // Tạo entity KhuyenMai
            var km = new KhuyenMai
            {
                ID_KhuyenMai = m.ID_KhuyenMai ?? Guid.NewGuid(),
                Ten_KhuyenMai = m.Ten_KhuyenMai,
                KieuGiamGia = m.KieuGiamGia,
                GiaTriGiam = m.GiaTriGiam,
                GiaTriToiDa = m.GiaTriToiDa,
                MoTa = m.MoTa ?? "",
                NgayBatDau = m.NgayBatDau,
                NgayHetHan = m.NgayHetHan,
                TrangThai = m.TrangThai
            };

            // Gọi service để lưu DB
            await _kmService.AddAsync(km, targetIds);

            TempData["Success"] = "Tạo khuyến mãi thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ================= Action: Edit =================
        public async Task<IActionResult> Edit(Guid id)
        {
            var km = await _kmService.GetByIdAsync(id);
            if (km == null) return NotFound();

            // Map sang ViewModel để hiển thị trong form Edit
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
                SanPhamChiTietIds = km.ChiTietKhuyenMais.Select(c => c.ID_SanPhamChiTiet).ToList()
            };

            return View(vm);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KhuyenMaiFormVM m)
        {
            await ValidateFormAsync(m);

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

            // Cập nhật dữ liệu
            exist.Ten_KhuyenMai = m.Ten_KhuyenMai;
            exist.KieuGiamGia = m.KieuGiamGia;
            exist.GiaTriGiam = m.GiaTriGiam;
            exist.GiaTriToiDa = m.GiaTriToiDa;
            exist.MoTa = m.MoTa ?? "";
            exist.NgayBatDau = m.NgayBatDau;
            exist.NgayHetHan = m.NgayHetHan;
            exist.TrangThai = m.TrangThai;

            // Xác định SPCT áp dụng
            HashSet<Guid> targetIds = m.ThuongHieuId.HasValue
                ? await GetSpctIdsByBrandAsync(m.ThuongHieuId.Value)
                : new HashSet<Guid>(m.SanPhamChiTietIds ?? Enumerable.Empty<Guid>());

            await _kmService.UpdateAsync(exist, targetIds);

            TempData["Success"] = "Cập nhật khuyến mãi thành công.";
            return RedirectToAction(nameof(Index));
        }

        // ================= AJAX: Lọc SPCT theo thương hiệu =================
        [HttpGet]
        public async Task<IActionResult> SpctByBrand(Guid? brandId)
        {
            var q = _db.SanPhamChiTiets
                .AsNoTracking()
                .Include(s => s.SanPham)
                .Include(s => s.TheTich)
                .AsQueryable();

            // Nếu có chọn thương hiệu → filter theo thương hiệu
            if (brandId.HasValue)
                q = q.Where(s => s.SanPham.ID_ThuongHieu == brandId.Value);

            // Trả về danh sách SPCT dạng JSON
            var items = await q
                .OrderByDescending(s => s.NgayTao)
                .Select(s => new
                {
                    id = s.ID_SanPhamChiTiet,
                    text = s.SanPham.Ten_SanPham + " - " + s.TheTich.GiaTri + s.TheTich.DonVi +
                           $" (Giá: {s.GiaBan:N0} đ)"
                })
                .ToListAsync();

            return Json(items);
        }

        // ================= Action: Toggle =================
        [HttpPost]
        public async Task<IActionResult> Toggle(Guid id)
        {
            await _kmService.ToggleAsync(id); // Bật/tắt khuyến mãi
            return RedirectToAction(nameof(Index));
        }

        // ================= Action: Delete =================
        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _kmService.DeleteAsync(id); // Xóa khuyến mãi
            TempData["Success"] = "Đã xóa khuyến mãi.";
            return RedirectToAction(nameof(Index));
        }

        // ================= Load dropdown helpers =================
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

            // Gán vào ViewBag để view dùng MultiSelectList
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

        // ================= Action: Details =================
        public async Task<IActionResult> Details(Guid id)
        {
            var km = await _db.KhuyenMais
                .Include(k => k.ChiTietKhuyenMais)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(spct => spct.SanPham)
                .Include(k => k.ChiTietKhuyenMais)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(spct => spct.TheTich)
                .FirstOrDefaultAsync(k => k.ID_KhuyenMai == id);

            if (km == null) return NotFound();

            return View(km); // Trả về view hiển thị chi tiết KM
        }
    }
}
