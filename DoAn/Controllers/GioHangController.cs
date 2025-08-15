using DoAn.Models;
using DoAn.Service.IService;
using DoAn.ViewModel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    public class GioHangController : Controller
    {
        private readonly IGioHangService _cart;
        private readonly DoAnDbContext _db;

        public GioHangController(IGioHangService cart, DoAnDbContext db)
        {
            _cart = cart;
            _db = db;
        }

        private async Task<Guid> GetKhachHangIdAsync()
        {
            var taiKhoanIdStr = HttpContext.Session.GetString("UserID");
            if (string.IsNullOrEmpty(taiKhoanIdStr))
                throw new Exception("Bạn cần đăng nhập.");

            var taiKhoanId = Guid.Parse(taiKhoanIdStr);

            var kh = await _db.KhachHangs.FirstOrDefaultAsync(k => k.ID_TaiKhoan == taiKhoanId);
            if (kh == null)
            {
                kh = new KhachHang
                {
                    ID_KhachHang = Guid.NewGuid(),
                    ID_TaiKhoan = taiKhoanId,
                    Ma_KhachHang = "KH" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Ten_KhachHang = HttpContext.Session.GetString("Username") ?? "Khách hàng",
                    Email = null,
                    SoDienThoai = null,
                    GioiTinh = "Khác",
                    NgaySinh = null,
                    NgayTao = DateTime.Now,
                    TrangThai = 1
                };
                _db.KhachHangs.Add(kh);
                await _db.SaveChangesAsync();
            }
            return kh.ID_KhachHang;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var khId = await GetKhachHangIdAsync();
                var vm = await _cart.GetCartAsync(khId);
                return View(vm);
            }
            catch { return RedirectToAction("Login", "TaiKhoan"); }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Guid sanPhamChiTietId, int soLuong = 1, string? returnUrl = null)
        {
            var khId = await GetKhachHangIdAsync();
            await _cart.AddItemAsync(khId, sanPhamChiTietId, soLuong);
            if (!string.IsNullOrWhiteSpace(returnUrl)) return LocalRedirect(returnUrl);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(Guid chiTietGioHangId, int soLuong)
        {
            var khId = await GetKhachHangIdAsync();
            await _cart.UpdateItemAsync(khId, chiTietGioHangId, soLuong);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(Guid chiTietGioHangId)
        {
            var khId = await GetKhachHangIdAsync();
            await _cart.RemoveItemAsync(khId, chiTietGioHangId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var khId = await GetKhachHangIdAsync();
            await _cart.ClearAsync(khId);
            return RedirectToAction(nameof(Index));
        }

        // ==== AJAX: clamp & trả qty ====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAjax(Guid chiTietGioHangId, int soLuong)
        {
            var khId = await GetKhachHangIdAsync();
            await _cart.UpdateItemAsync(khId, chiTietGioHangId, soLuong);

            var cart = await _cart.GetCartAsync(khId);
            var item = cart.Items.FirstOrDefault(x => x.ChiTietGioHangId == chiTietGioHangId);

            return Json(new
            {
                ok = item != null,
                qty = item?.SoLuong ?? 0,
                lineTotal = item?.ThanhTien ?? 0m,
                subtotal = cart.Subtotal
            });
        }

        // ==== Tương thích JS cũ ====
        [HttpGet("/Cart/Add")]
        public async Task<IActionResult> CartAddByProduct(Guid id)
        {
            try
            {
                var khId = await GetKhachHangIdAsync();
                var sp = await _db.SanPhams.Include(s => s.SanPhamChiTiets)
                                           .FirstOrDefaultAsync(s => s.ID_SanPham == id);
                if (sp == null) return BadRequest();
                var ct = sp.SanPhamChiTiets.Where(c => c.SoLuong > 0).OrderBy(c => c.GiaBan).FirstOrDefault()
                         ?? sp.SanPhamChiTiets.OrderBy(c => c.GiaBan).FirstOrDefault();
                if (ct == null) return BadRequest();

                await _cart.AddItemAsync(khId, ct.ID_SanPhamChiTiet, 1);
                return Ok();
            }
            catch { return Unauthorized(); }
        }

        [HttpGet("/Cart/AddDetail")]
        public async Task<IActionResult> CartAddByVariant(Guid id)
        {
            try
            {
                var khId = await GetKhachHangIdAsync();
                await _cart.AddItemAsync(khId, id, 1);
                return Ok();
            }
            catch { return Unauthorized(); }
        }
    }
}
