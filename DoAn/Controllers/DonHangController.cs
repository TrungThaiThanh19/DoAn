using DoAn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAn.Controllers
{
    [Authorize(Roles = "khachhang")]
    public class DonHangController : Controller
    {
        private readonly DoAnDbContext _db;
        public DonHangController(DoAnDbContext db) => _db = db;

        private async Task<Guid?> TryGetTaiKhoanIdAsync()
        {
            var idStr = User.FindFirstValue("TaiKhoanId")
                        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? HttpContext.Session.GetString("TaiKhoanId")
                        ?? HttpContext.Session.GetString("UserID");

            if (Guid.TryParse(idStr, out var g)) return g;

            var username = HttpContext.Session.GetString("Username") ?? User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                var tk = await _db.TaiKhoans.AsNoTracking()
                              .FirstOrDefaultAsync(t => t.Uername == username);
                if (tk != null) return tk.ID_TaiKhoan;
            }
            return null;
        }

        private async Task<Guid> GetKhachHangIdAsync()
        {
            var taiKhoanId = await TryGetTaiKhoanIdAsync();
            if (taiKhoanId == null) throw new Exception("Không tìm thấy tài khoản đăng nhập.");

            var kh = await _db.KhachHangs.AsNoTracking()
                         .FirstOrDefaultAsync(k => k.ID_TaiKhoan == taiKhoanId.Value);
            if (kh == null) throw new Exception("Không tìm thấy khách hàng tương ứng.");

            return kh.ID_KhachHang;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            Guid khId;
            try { khId = await GetKhachHangIdAsync(); }
            catch
            {
                TempData["Error"] = "Vui lòng đăng nhập lại để xem đơn hàng.";
                return RedirectToAction("Login", "TaiKhoan");
            }

            var list = await _db.HoaDons
                .AsNoTracking()
                .Where(h => h.ID_KhachHang == khId)
                .OrderByDescending(h => h.NgayTao)
                .ToListAsync();

            return View(list);
        }

        [HttpGet]
        public async Task<IActionResult> Track(Guid id)
        {
            Guid khId;
            try { khId = await GetKhachHangIdAsync(); }
            catch
            {
                TempData["Error"] = "Vui lòng đăng nhập lại để xem đơn hàng của bạn.";
                return RedirectToAction("Login", "TaiKhoan");
            }

            var hd = await _db.HoaDons
                .AsNoTracking()
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                    .ThenInclude(v => v.SanPham)
                .Include(h => h.TrangThaiDonHangs) // <-- THÊM: để hiện lý do hủy
                .FirstOrDefaultAsync(h => h.ID_HoaDon == id && h.ID_KhachHang == khId);

            if (hd == null) return NotFound();
            return View(hd);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Huy(Guid id, string? lyDo)
        {
            Guid khId;
            try { khId = await GetKhachHangIdAsync(); }
            catch
            {
                TempData["Error"] = "Vui lòng đăng nhập lại để hủy đơn.";
                return RedirectToAction("Login", "TaiKhoan");
            }

            var hd = await _db.HoaDons.FirstOrDefaultAsync(h => h.ID_HoaDon == id && h.ID_KhachHang == khId);
            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền.";
                return RedirectToAction("Index");
            }

            if (hd.TrangThai >= 2 || hd.TrangThai == 5 || hd.TrangThai == 4)
            {
                TempData["Error"] = "Đơn đã sang giai đoạn xử lý, không thể hủy.";
                return RedirectToAction("Track", new { id });
            }

            // HOÀN KHO: cộng trả số lượng cho từng biến thể sản phẩm
            var lines = await _db.HoaDonChiTiets
                .Include(ct => ct.SanPhamChiTiet)
                .Where(ct => ct.ID_HoaDon == id)
                .ToListAsync();

            foreach (var ct in lines)
            {
                if (ct.SanPhamChiTiet == null) continue;

                var spct = ct.SanPhamChiTiet;
                var t = spct.GetType();
                var p = t.GetProperty("SoLuongTon")
                        ?? t.GetProperty("SoLuong")
                        ?? t.GetProperty("TonKho")
                        ?? t.GetProperty("SoLuong_TonKho");
                if (p != null)
                {
                    var cur = Convert.ToInt32(p.GetValue(spct) ?? 0);
                    p.SetValue(spct, cur + ct.SoLuong);
                }
            }

            // (giữ nguyên logic cũ) đổi trạng thái
            hd.TrangThai = 5;
            hd.NgayCapNhat = DateTime.Now;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã hủy đơn hàng thành công.";
            return RedirectToAction("Track", new { id });
        }

        // KHÁCH HÀNG NHẬN THÔNG BÁO ĐƠN BỊ HỦY (dành cho trang Giỏ hàng)
        [HttpGet]
        [AllowAnonymous] // cho phép fetch mà không bị 302 về trang login
        public async Task<IActionResult> RecentCanceled()
        {
            Guid khId;
            try { khId = await GetKhachHangIdAsync(); }
            catch { return Json(new { ok = false }); }

            // lấy đơn đã hủy mới nhất của KH
            var latest = await _db.HoaDons.AsNoTracking()
                .Where(h => h.ID_KhachHang == khId && h.TrangThai == 5)
                .OrderByDescending(h => h.NgayCapNhat ?? h.NgayTao)
                .Select(h => new { h.ID_HoaDon, h.Ma_HoaDon, At = (h.NgayCapNhat ?? h.NgayTao) })
                .FirstOrDefaultAsync();

            if (latest == null) return Json(new { ok = false });

            // lấy lý do hủy (nếu có)
            var reason = await _db.TrangThaiDonHangs.AsNoTracking()
                .Where(t => t.ID_HoaDon == latest.ID_HoaDon && t.TrangThai == 5)
                .OrderByDescending(t => t.NgayChuyen)
                .Select(t => t.NoiDungDoi)
                .FirstOrDefaultAsync();

            return Json(new
            {
                ok = true,
                id = latest.ID_HoaDon,
                ma = latest.Ma_HoaDon,
                at = latest.At.ToString("dd/MM/yyyy HH:mm"),
                reason = string.IsNullOrWhiteSpace(reason) ? null : reason
            });
        }
        // ===== HOÀN HÀNG NGAY (KH) — chỉ cho phép khi TrangThai == 3 (ĐÃ THANH TOÁN) =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HoanHang(Guid id, string? lyDo)
        {
            Guid khId;
            try { khId = await GetKhachHangIdAsync(); }
            catch
            {
                TempData["Error"] = "Vui lòng đăng nhập lại để yêu cầu hoàn hàng.";
                return RedirectToAction("Login", "TaiKhoan");
            }

            var hd = await _db.HoaDons
                .Include(h => h.HoaDonChiTiets)
                .Include(h => h.TrangThaiDonHangs)
                .FirstOrDefaultAsync(h => h.ID_HoaDon == id && h.ID_KhachHang == khId);

            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền.";
                return RedirectToAction("Index");
            }

            // Chỉ cho phép yêu cầu hoàn khi đơn đã thanh toán (3). Không đổi trạng thái đơn.
            if (hd.TrangThai != 3)
            {
                TempData["Error"] = "Chỉ có thể yêu cầu hoàn hàng khi đơn đang ở trạng thái 'Đã thanh toán'.";
                return RedirectToAction("Track", new { id });
            }

            // Tránh gửi trùng nhiều lần
            var daGui = await _db.TrangThaiDonHangs.AsNoTracking()
                            .AnyAsync(t => t.ID_HoaDon == id && t.TrangThai == 6);
            if (daGui)
            {
                TempData["Success"] = "Bạn đã gửi yêu cầu hoàn hàng trước đó. Shop sẽ liên hệ sớm.";
                return RedirectToAction("Track", new { id });
            }

            // GHI NHẬN YÊU CẦU HOÀN HÀNG -> chỉ thêm log (TrangThaiDonHang = 6). KHÔNG đổi hd.TrangThai, KHÔNG cộng kho.
            _db.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = hd.ID_HoaDon,
                TrangThai = 6, // 6 = KH yêu cầu hoàn hàng (để shop thấy)
                NgayChuyen = DateTime.Now,
                NoiDungDoi = string.IsNullOrWhiteSpace(lyDo) ? "Khách hàng yêu cầu hoàn hàng." : lyDo,
                NhanVienDoi = "Khách hàng"
            });

            hd.NgayCapNhat = DateTime.Now; // chỉ cập nhật mốc thời gian cho dễ lọc bên Admin
            await _db.SaveChangesAsync();

            // TODO (tuỳ bạn): tại đây có thể push SignalR / gửi email / tạo thông báo admin nếu đã có hạ tầng
            TempData["Success"] = "Đã gửi yêu cầu hoàn hàng. Shop sẽ liên hệ và sắp xếp đơn vị vận chuyển.";
            return RedirectToAction("Track", new { id });
        }

    }
}
