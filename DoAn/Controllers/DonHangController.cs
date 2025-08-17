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

        // ===== Helpers: lấy TaiKhoanId / KhachHangId hiện tại =====
        private async Task<Guid?> TryGetTaiKhoanIdAsync()
        {
            // 1) Claims
            var idStr = User.FindFirstValue("TaiKhoanId")
                        ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
                        // 2) Session
                        ?? HttpContext.Session.GetString("TaiKhoanId")
                        ?? HttpContext.Session.GetString("UserID");

            if (Guid.TryParse(idStr, out var g)) return g;

            // 3) Tìm theo Username (phòng hờ)
            var username = HttpContext.Session.GetString("Username") ?? User.Identity?.Name;
            if (!string.IsNullOrWhiteSpace(username))
            {
                // cột "Uername" theo model bạn gửi
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

        // ===== Danh sách đơn hàng của tôi =====
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

        // ===== Theo dõi 1 đơn =====
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
                .FirstOrDefaultAsync(h => h.ID_HoaDon == id && h.ID_KhachHang == khId);

            if (hd == null) return NotFound();
            return View(hd);
        }

        // ===== HỦY ĐƠN (chỉ khi còn sớm) =====
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

            // Chính sách: chỉ cho hủy khi trạng thái 0/1 (chờ xác nhận / đã xác nhận)
            if (hd.TrangThai >= 2 || hd.TrangThai == 5 || hd.TrangThai == 4)
            {
                TempData["Error"] = "Đơn đã sang giai đoạn xử lý, không thể hủy.";
                return RedirectToAction("Track", new { id });
            }

            hd.TrangThai = 5; // Hủy
            hd.NgayCapNhat = DateTime.Now;

            // Nếu có cột LyDoHuy thì mở comment:
            // hd.LyDoHuy = lyDo;

            await _db.SaveChangesAsync();
            TempData["Success"] = "Đã hủy đơn hàng thành công.";
            return RedirectToAction("Track", new { id });
        }
    }
}
