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

        // ===================== SỬA 1: Add (POST) kiểm tra tồn kho =====================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(Guid sanPhamChiTietId, int soLuong = 1, string? returnUrl = null)
        {
            var khId = await GetKhachHangIdAsync();

            // Kiểm tra tồn kho biến thể
            var ct = await _db.SanPhamChiTiets.AsNoTracking()
                          .FirstOrDefaultAsync(x => x.ID_SanPhamChiTiet == sanPhamChiTietId);
            if (ct == null) return NotFound();
            if (ct.SoLuong <= 0)
            {
                TempData["CartError"] = "Biến thể đã hết hàng.";
                return RedirectToAction(nameof(Index));
            }

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

        // ===================== SỬA 2: GET /Cart/Add — chỉ thêm khi còn hàng =====================
        [HttpGet("/Cart/Add")]
        public async Task<IActionResult> CartAddByProduct(Guid id)
        {
            try
            {
                var khId = await GetKhachHangIdAsync();
                var sp = await _db.SanPhams.Include(s => s.SanPhamChiTiets)
                                           .FirstOrDefaultAsync(s => s.ID_SanPham == id);
                if (sp == null) return BadRequest(new { message = "Không tìm thấy sản phẩm." });

                // CHỈ CHỌN biến thể CÒN HÀNG
                var ct = sp.SanPhamChiTiets
                           .Where(c => c.SoLuong > 0)
                           .OrderBy(c => c.GiaBan)
                           .FirstOrDefault();

                if (ct == null)
                    return StatusCode(409, new { message = "Sản phẩm đã hết hàng." }); // 409: hết hàng

                await _cart.AddItemAsync(khId, ct.ID_SanPhamChiTiet, 1);

                // Trả count để update badge
                var cart = await _cart.GetCartAsync(khId);
                var count = cart.Items.Sum(x => x.SoLuong);
                return Ok(new { count });
            }
            catch
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập." });
            }
        }

        // ===================== SỬA 3: GET /Cart/AddDetail — kiểm tra tồn kho biến thể =====================
        [HttpGet("/Cart/AddDetail")]
        public async Task<IActionResult> CartAddByVariant(Guid id)
        {
            try
            {
                var khId = await GetKhachHangIdAsync();

                var ct = await _db.SanPhamChiTiets
                                  .AsNoTracking()
                                  .FirstOrDefaultAsync(x => x.ID_SanPhamChiTiet == id);
                if (ct == null) return BadRequest(new { message = "Không tìm thấy biến thể." });

                if (ct.SoLuong <= 0)
                    return StatusCode(409, new { message = "Biến thể đã hết hàng." });

                await _cart.AddItemAsync(khId, id, 1);

                var cart = await _cart.GetCartAsync(khId);
                var count = cart.Items.Sum(x => x.SoLuong);
                return Ok(new { count });
            }
            catch
            {
                return Unauthorized(new { message = "Bạn cần đăng nhập." });
            }
        }

        // ==== Đếm tổng số lượng trong giỏ để hiển thị badge ====
        [HttpGet("/Cart/Count")]
        public async Task<IActionResult> Count()
        {
            try
            {
                var khId = await GetKhachHangIdAsync();
                var cart = await _cart.GetCartAsync(khId);
                var count = cart.Items.Sum(x => x.SoLuong);
                return Json(new { count });
            }
            catch
            {
                return Json(new { count = 0 });
            }
        }
        // ==== Kiểm tra tồn kho trước khi sang bước đặt hàng (Address) ====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateStock(string? lines)
        {
            try
            {
                var khId = await GetKhachHangIdAsync();
                var cart = await _cart.GetCartAsync(khId);

                // Lọc theo các line user đang tick (nếu có), còn không thì kiểm tra toàn bộ giỏ
                HashSet<Guid>? selectedLineIds = null;
                if (!string.IsNullOrWhiteSpace(lines))
                {
                    selectedLineIds = new HashSet<Guid>(
                        lines.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                             .Select(s => Guid.TryParse(s, out var g) ? g : Guid.Empty)
                             .Where(g => g != Guid.Empty)
                    );
                }

                var itemsToCheck = (selectedLineIds == null || selectedLineIds.Count == 0)
                    ? cart.Items
                    : cart.Items.Where(x => selectedLineIds.Contains(x.ChiTietGioHangId)).ToList();

                if (!itemsToCheck.Any())
                    return Json(new { ok = false, message = "Không có sản phẩm nào được chọn." });

                // Lấy tồn kho hiện tại từ DB theo danh sách biến thể trong giỏ
                var variantIds = itemsToCheck.Select(i => i.SanPhamChiTietId).Distinct().ToList();

                var variants = await _db.SanPhamChiTiets
                    .Include(v => v.TheTich)
                    .Include(v => v.SanPham)
                    .Where(v => variantIds.Contains(v.ID_SanPhamChiTiet))
                    .ToListAsync();

                var variantDict = variants.ToDictionary(v => v.ID_SanPhamChiTiet, v => v);

                var problems = new List<object>();

                foreach (var line in itemsToCheck)
                {
                    if (!variantDict.TryGetValue(line.SanPhamChiTietId, out var v))
                    {
                        problems.Add(new
                        {
                            lineId = line.ChiTietGioHangId,
                            requested = line.SoLuong,
                            available = 0,
                            ten = line.TenSanPham,
                            theTich = line.TheTich,
                            reason = "not_found",
                            message = "Sản phẩm không còn tồn tại."
                        });
                        continue;
                    }

                    var available = Math.Max(0, v.SoLuong);
                    if (available <= 0)
                    {
                        problems.Add(new
                        {
                            lineId = line.ChiTietGioHangId,
                            requested = line.SoLuong,
                            available = 0,
                            ten = v.SanPham?.Ten_SanPham ?? line.TenSanPham,
                            theTich = v.TheTich?.DonVi ?? line.TheTich,
                            reason = "oos",
                            message = "Sản phẩm đã hết hàng."
                        });
                    }
                    else if (line.SoLuong > available)
                    {
                        problems.Add(new
                        {
                            lineId = line.ChiTietGioHangId,
                            requested = line.SoLuong,
                            available,
                            ten = v.SanPham?.Ten_SanPham ?? line.TenSanPham,
                            theTich = v.TheTich?.DonVi ?? line.TheTich,
                            reason = "insufficient",
                            message = "Số lượng vượt quá tồn kho."
                        });
                    }
                }

                if (problems.Count > 0)
                {
                    return Json(new
                    {
                        ok = false,
                        problems
                    });
                }

                return Json(new { ok = true });
            }
            catch
            {
                // nếu chưa đăng nhập sẽ bị catch ở đây
                Response.StatusCode = 401;
                return Json(new { ok = false, message = "Bạn cần đăng nhập." });
            }
        }

    }
}
