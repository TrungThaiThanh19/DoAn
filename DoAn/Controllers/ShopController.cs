using DoAn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    public class ShopController : Controller
    {
        private readonly DoAnDbContext _context;
        private const int PageSize = 8;

        public ShopController(DoAnDbContext context) => _context = context;

        // Danh sách sản phẩm (kèm KM để tính giá hiển thị)
        public async Task<IActionResult> Index(string? search, Guid? thuongHieuId, Guid? gioiTinhId, int page = 1)
        {
            if (page < 1) page = 1;

            var query = _context.SanPhams
                .AsNoTracking()
                .Include(sp => sp.ThuongHieu)
                .Include(sp => sp.GioiTinh)
                .Include(sp => sp.SanPhamChiTiets).ThenInclude(ct => ct.TheTich)
                .Include(sp => sp.SanPhamChiTiets)
                    .ThenInclude(ct => ct.ChiTietKhuyenMais)
                        .ThenInclude(ctkm => ctkm.KhuyenMai)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(sp => sp.Ten_SanPham.Contains(search));
            if (thuongHieuId.HasValue)
                query = query.Where(sp => sp.ID_ThuongHieu == thuongHieuId);
            if (gioiTinhId.HasValue)
                query = query.Where(sp => sp.ID_GioiTinh == gioiTinhId);

            query = query
                .OrderByDescending(sp => sp.SanPhamChiTiets.Any(ct => ct.SoLuong > 0))
                .ThenByDescending(sp => sp.NgayTao);

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * PageSize).Take(PageSize).ToListAsync();

            ViewBag.ThuongHieuList = new SelectList(await _context.ThuongHieus.AsNoTracking().ToListAsync(),
                "ID_ThuongHieu", "Ten_ThuongHieu", thuongHieuId);
            ViewBag.GioiTinhList = new SelectList(await _context.GioiTinhs.AsNoTracking().ToListAsync(),
                "ID_GioiTinh", "Ten_GioiTinh", gioiTinhId);

            ViewBag.Search = search;
            ViewBag.ThuongHieuId = thuongHieuId;
            ViewBag.GioiTinhId = gioiTinhId;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);

            return View(items);
        }

        // Chi tiết (include KM để tính giá từng biến thể)
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _context.SanPhams
                .AsNoTracking()
                .Include(sp => sp.ThuongHieu)
                .Include(sp => sp.GioiTinh)
                .Include(sp => sp.QuocGia)
                .Include(sp => sp.SanPhamChiTiets).ThenInclude(ct => ct.TheTich)
                .Include(sp => sp.SanPhamChiTiets)
                    .ThenInclude(ct => ct.ChiTietKhuyenMais)
                        .ThenInclude(ctkm => ctkm.KhuyenMai)
                .FirstOrDefaultAsync(sp => sp.ID_SanPham == id);

            if (product == null) return NotFound();
            return View(product);
        }
    }
}