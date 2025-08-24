using DoAn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace DoAn.Controllers
{
    [Authorize(Roles = "khachhang")]
    public class DonHangController : Controller
    {
        private readonly DoAnDbContext _db;
        public DonHangController(DoAnDbContext db) => _db = db;

        // Map trạng thái
        // 0: chờ xác nhận, 1: đã xác nhận, 2: đang vận chuyển, 3: đã thanh toán,
        // 4: hoàn thành, 5: đã hủy, 6: KH yêu cầu hoàn (log), 7: hoàn hàng thành công
        private const int TT_HUY = 5;
        private const int TT_DA_THANH_TOAN = 3;
        private const int TT_HOAN_HANG_THANH_CONG = 7;

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
                .Include(h => h.TrangThaiDonHangs)
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

            if (hd.TrangThai >= 2 || hd.TrangThai == TT_HUY || hd.TrangThai == TT_HOAN_HANG_THANH_CONG)
            {
                TempData["Error"] = "Đơn đã sang giai đoạn xử lý, không thể hủy.";
                return RedirectToAction("Track", new { id });
            }

            // Cộng trả kho
            var lines = await _db.HoaDonChiTiets
                .Include(ct => ct.SanPhamChiTiet)
                .Where(ct => ct.ID_HoaDon == id)
                .ToListAsync();

            foreach (var ct in lines)
            {
                if (ct.SanPhamChiTiet == null) continue;

                // tên thuộc tính lưu tồn: SoLuong (mặc định)
                ct.SanPhamChiTiet.SoLuong += ct.SoLuong;
                if (ct.SanPhamChiTiet.SoLuong > 0 && ct.SanPhamChiTiet.TrangThai == 0)
                    ct.SanPhamChiTiet.TrangThai = 1;
            }

            hd.TrangThai = TT_HUY;
            hd.NgayCapNhat = DateTime.Now;

            // log lý do hủy
            _db.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = hd.ID_HoaDon,
                TrangThai = TT_HUY,
                NgayChuyen = DateTime.Now,
                NoiDungDoi = string.IsNullOrWhiteSpace(lyDo) ? "Khách hàng tự hủy đơn hàng." : lyDo,
                NhanVienDoi = "Khách hàng"
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã hủy đơn hàng thành công.";
            return RedirectToAction("Track", new { id });
        }

        // ===== KH yêu cầu hoàn hàng (log = 6), KHÔNG đổi trạng thái đơn =====
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

            if (hd.TrangThai != TT_DA_THANH_TOAN)
            {
                TempData["Error"] = "Chỉ có thể yêu cầu hoàn hàng khi đơn đang ở trạng thái 'Đã thanh toán'.";
                return RedirectToAction("Track", new { id });
            }

            var daGui = await _db.TrangThaiDonHangs.AsNoTracking()
                            .AnyAsync(t => t.ID_HoaDon == id && t.TrangThai == 6);
            if (daGui)
            {
                TempData["Success"] = "Bạn đã gửi yêu cầu hoàn hàng trước đó. Shop sẽ liên hệ sớm.";
                return RedirectToAction("Track", new { id });
            }

            _db.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = hd.ID_HoaDon,
                TrangThai = 6, // KH yêu cầu hoàn
                NgayChuyen = DateTime.Now,
                NoiDungDoi = string.IsNullOrWhiteSpace(lyDo) ? "Khách hàng yêu cầu hoàn hàng." : lyDo,
                NhanVienDoi = "Khách hàng"
            });

            hd.NgayCapNhat = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã gửi yêu cầu hoàn hàng. Shop sẽ liên hệ và sắp xếp đơn vị vận chuyển.";
            return RedirectToAction("Track", new { id });
        }

        // ===== ADMIN/NHÂN VIÊN CHẤP NHẬN HOÀN HÀNG -> TrangThai = 7, cộng trả kho =====
        [HttpPost]
        [Authorize(Roles = "admin,nhanvien")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AcceptReturn(Guid id, string? ghiChu)
        {
            var hd = await _db.HoaDons
                .Include(h => h.HoaDonChiTiets)
                .ThenInclude(ct => ct.SanPhamChiTiet)
                .FirstOrDefaultAsync(h => h.ID_HoaDon == id);

            if (hd == null) return NotFound();

            // Chỉ nên cho accept khi đơn đã thanh toán hoặc đang xử lý tương đương
            if (hd.TrangThai == TT_HUY || hd.TrangThai == TT_HOAN_HANG_THANH_CONG)
            {
                TempData["Error"] = "Đơn đã hủy hoặc đã hoàn xong.";
                return RedirectToAction("Track", new { id });
            }

            // Cộng trả kho
            foreach (var ct in hd.HoaDonChiTiets)
            {
                if (ct.SanPhamChiTiet == null) continue;
                ct.SanPhamChiTiet.SoLuong += ct.SoLuong;
                if (ct.SanPhamChiTiet.SoLuong > 0 && ct.SanPhamChiTiet.TrangThai == 0)
                    ct.SanPhamChiTiet.TrangThai = 1;
            }

            // Set trạng thái hoàn thành hoàn hàng
            hd.TrangThai = TT_HOAN_HANG_THANH_CONG;
            hd.NgayCapNhat = DateTime.Now;

            // Log
            _db.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = hd.ID_HoaDon,
                TrangThai = TT_HOAN_HANG_THANH_CONG,
                NgayChuyen = DateTime.Now,
                NoiDungDoi = string.IsNullOrWhiteSpace(ghiChu) ? "Đã chấp nhận hoàn hàng." : ghiChu,
                NhanVienDoi = User?.Identity?.Name ?? "Nhân viên"
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật: Hoàn hàng thành công.";
            return RedirectToAction("Track", new { id });
        }

        // (Tùy chọn) Từ chối hoàn: chỉ ghi log, không đổi trạng thái đơn
        [HttpPost]
        [Authorize(Roles = "admin,nhanvien")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectReturn(Guid id, string? ghiChu)
        {
            var hd = await _db.HoaDons.FirstOrDefaultAsync(h => h.ID_HoaDon == id);
            if (hd == null) return NotFound();

            _db.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = hd.ID_HoaDon,
                TrangThai = 6, // vẫn dùng channel "yêu cầu hoàn" để lần theo
                NgayChuyen = DateTime.Now,
                NoiDungDoi = string.IsNullOrWhiteSpace(ghiChu) ? "Yêu cầu hoàn bị từ chối." : ghiChu,
                NhanVienDoi = User?.Identity?.Name ?? "Nhân viên"
            });

            hd.NgayCapNhat = DateTime.Now;
            await _db.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật: Từ chối yêu cầu hoàn hàng.";
            return RedirectToAction("Track", new { id });
        }

        // ==== API nhỏ cho trang Giỏ hàng hiển thị toast đơn bị hủy (giữ nguyên) ====
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> RecentCanceled()
        {
            Guid khId;
            try { khId = await GetKhachHangIdAsync(); }
            catch { return Json(new { ok = false }); }

            var latest = await _db.HoaDons.AsNoTracking()
                .Where(h => h.ID_KhachHang == khId && h.TrangThai == TT_HUY)
                .OrderByDescending(h => h.NgayCapNhat ?? h.NgayTao)
                .Select(h => new { h.ID_HoaDon, h.Ma_HoaDon, At = (h.NgayCapNhat ?? h.NgayTao) })
                .FirstOrDefaultAsync();

            if (latest == null) return Json(new { ok = false });

            var reason = await _db.TrangThaiDonHangs.AsNoTracking()
                .Where(t => t.ID_HoaDon == latest.ID_HoaDon && t.TrangThai == TT_HUY)
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
    }
}
