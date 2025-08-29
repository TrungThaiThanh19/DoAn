using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DoAn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    [Authorize(Roles = "admin,nhanvien")]
    public class QuanLyTraHangController : Controller
    {
        private readonly DoAnDbContext _context;
        public QuanLyTraHangController(DoAnDbContext context) => _context = context;

        // Chuẩn hóa trạng thái phiếu
        private static class ReturnStatus
        {
            public const int YeuCau = 0;
            public const int DaDuyet = 1;
            public const int DaNhanHang = 2;
            public const int DaHoanTien = 3;
            public const int TuChoi = 9;
        }

        private const int KH_YEU_CAU_HOAN_STATUS = 6; // KH bấm "Hoàn hàng"

        // (nếu chưa có view riêng) quay về danh sách Hóa đơn
        [HttpGet]
        public IActionResult Index(int? trangThai) => RedirectToAction("Index", "HoaDon");

        // Details phiếu -> điều hướng về HoaDon/Details cho gọn
        [HttpGet]
        public async Task<IActionResult> Details(Guid id)
        {
            var phieu = await _context.QuanLyTraHangs.AsNoTracking()
                              .FirstOrDefaultAsync(p => p.ID_TraHang == id);
            if (phieu == null) return NotFound();
            return RedirectToAction("Details", "HoaDon", new { id = phieu.ID_HoaDon });
        }

        // ============== TẠO PHIẾU HOÀN TOÀN BỘ (ONE-CLICK) ==============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFull(Guid hoaDonId, string? lyDo)
        {
            var hd = await _context.HoaDons
                .Include(h => h.HoaDonChiTiets).ThenInclude(ct => ct.SanPhamChiTiet)
                .Include(h => h.TraHangs)
                .FirstOrDefaultAsync(h => h.ID_HoaDon == hoaDonId);

            if (hd == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index", "HoaDon");
            }

            // 1) BẮT BUỘC: KH đã gửi yêu cầu hoàn (đã có log trạng thái = 6)
            var hasRequest = await _context.TrangThaiDonHangs.AsNoTracking()
                .AnyAsync(t => t.ID_HoaDon == hoaDonId && t.TrangThai == KH_YEU_CAU_HOAN_STATUS);
            if (!hasRequest)
            {
                TempData["Error"] = "Khách hàng chưa gửi yêu cầu hoàn hàng. Không thể tạo phiếu.";
                return RedirectToAction("Details", "HoaDon", new { id = hoaDonId });
            }

            // 2) Chỉ cho hoàn khi đơn đã thanh toán hoặc hoàn thành
            if (hd.TrangThai != 3 && hd.TrangThai != 4)
            {
                TempData["Error"] = "Đơn chưa đủ điều kiện hoàn hàng.";
                return RedirectToAction("Details", "HoaDon", new { id = hoaDonId });
            }

            // 3) Không tạo thêm nếu đã có phiếu đang xử lý (trừ phiếu đã hoàn tiền xong)
            var dangXuLy = await _context.QuanLyTraHangs
                .Where(p => p.ID_HoaDon == hoaDonId && p.TrangThai != ReturnStatus.TuChoi)
                .OrderByDescending(p => p.NgayTao)
                .FirstOrDefaultAsync();
            if (dangXuLy != null && dangXuLy.TrangThai != ReturnStatus.DaHoanTien)
            {
                TempData["Info"] = "Đơn đã có phiếu hoàn đang xử lý.";
                return RedirectToAction("Details", "HoaDon", new { id = hoaDonId });
            }

            // 4) Tạo dòng hoàn CHO TẤT CẢ sản phẩm còn có thể hoàn
            var subtotal = hd.HoaDonChiTiets.Sum(x => x.SoLuong * x.DonGia);
            var discount = hd.TongTienTruocGiam - hd.TongTienSauGiam;

            var lines = new List<ChiTietTraHang>();
            decimal tongHoan = 0;

            foreach (var ct in hd.HoaDonChiTiets)
            {
                var spct = ct.SanPhamChiTiet
                    ?? await _context.SanPhamChiTiets.FirstOrDefaultAsync(x => x.ID_SanPhamChiTiet == ct.ID_SanPhamChiTiet);
                if (spct == null) continue;

                // số đã hoàn trước (trừ phiếu bị từ chối)
                var daHoan = await _context.ChiTietTraHangs
                    .Include(t => t.TraHang)
                    .Where(t => t.TraHang.ID_HoaDon == hd.ID_HoaDon
                             && t.ID_ChiTietSanPham == spct.ID_SanPhamChiTiet
                             && t.TraHang.TrangThai != ReturnStatus.TuChoi)
                    .SumAsync(t => (int?)t.SoLuong) ?? 0;

                var soTra = Math.Max(0, ct.SoLuong - daHoan); // TRẢ HẾT phần còn lại
                if (soTra <= 0) continue;

                var gross = ct.DonGia * soTra;
                var prorate = subtotal > 0 ? (gross / subtotal) * discount : 0m;
                var tienHoan = decimal.Round(Math.Max(0, gross - prorate), 0);

                lines.Add(new ChiTietTraHang
                {
                    ID_ChiTietTraHang = Guid.NewGuid(),
                    ID_ChiTietSanPham = spct.ID_SanPhamChiTiet,
                    SanPhamChiTiet = spct,
                    SoLuong = soTra,
                    TienHoan = tienHoan
                });
                tongHoan += tienHoan;
            }

            if (!lines.Any())
            {
                TempData["Error"] = "Không còn số lượng để hoàn.";
                return RedirectToAction("Details", "HoaDon", new { id = hoaDonId });
            }

            var phieu = new QuanLyTraHang
            {
                ID_TraHang = Guid.NewGuid(),
                ID_HoaDon = hd.ID_HoaDon,
                LyDo = string.IsNullOrWhiteSpace(lyDo) ? "Hoàn tất cả theo chính sách." : lyDo.Trim(),
                GhiChu = "",
                NhanVienXuLy = User?.Identity?.Name ?? "system",
                NgayTao = DateTime.Now,
                TrangThai = ReturnStatus.DaDuyet, // admin tạo => coi như đã duyệt
                TongTienHoan = tongHoan,
                ChiTietTraHangs = lines
            };

            _context.QuanLyTraHangs.Add(phieu);
            _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = hd.ID_HoaDon,
                TrangThai = hd.TrangThai,
                NgayChuyen = DateTime.Now,
                NhanVienDoi = phieu.NhanVienXuLy,
                NoiDungDoi = $"Tạo phiếu hoàn toàn bộ #{phieu.ID_TraHang.ToString()[..8]}: {tongHoan:N0} VND."
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã tạo phiếu hoàn toàn bộ.";
            return RedirectToAction("Details", "HoaDon", new { id = hoaDonId });
        }

        // ============= NHẬN HOÀN HÀNG (+ KHO) =============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NhanHoanHang(Guid traHangId, string? ghiChu)
        {
            var phieu = await _context.QuanLyTraHangs
                .Include(p => p.HoaDon)
                .Include(p => p.ChiTietTraHangs).ThenInclude(d => d.SanPhamChiTiet)
                .FirstOrDefaultAsync(p => p.ID_TraHang == traHangId);
            if (phieu == null) return NotFound();
            if (phieu.TrangThai >= ReturnStatus.DaNhanHang)
            {
                TempData["Info"] = "Phiếu này đã nhận hàng trước đó.";
                return RedirectToAction("Details", "HoaDon", new { id = phieu.ID_HoaDon });
            }

            foreach (var d in phieu.ChiTietTraHangs)
            {
                if (d.SanPhamChiTiet == null) continue;
                CongTonKho(d.SanPhamChiTiet, d.SoLuong);
            }

            phieu.TrangThai = ReturnStatus.DaNhanHang;
            phieu.NhanVienXuLy = User?.Identity?.Name ?? "system";
            if (!string.IsNullOrWhiteSpace(ghiChu))
                phieu.GhiChu = AppendNote(phieu.GhiChu, ghiChu);

            phieu.HoaDon.NgayCapNhat = DateTime.Now;

            _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = phieu.ID_HoaDon,
                TrangThai = phieu.HoaDon.TrangThai,
                NgayChuyen = DateTime.Now,
                NhanVienDoi = phieu.NhanVienXuLy,
                NoiDungDoi = $"Đã nhận hàng hoàn (phiếu {phieu.ID_TraHang.ToString()[..8]}), cộng tồn kho."
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã nhận hoàn hàng và cộng tồn kho.";
            return RedirectToAction("Details", "HoaDon", new { id = phieu.ID_HoaDon });
        }

        // ============= XÁC NHẬN HOÀN TIỀN =============
        // ============= XÁC NHẬN HOÀN TIỀN =============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> XacNhanHoanTien(Guid traHangId, string? ghiChu)
        {
            var phieu = await _context.QuanLyTraHangs
                .Include(p => p.HoaDon)
                .FirstOrDefaultAsync(p => p.ID_TraHang == traHangId);
            if (phieu == null) return NotFound();

            if (phieu.TrangThai < ReturnStatus.DaNhanHang)
            {
                TempData["Error"] = "Cần nhận hàng hoàn trước khi xác nhận hoàn tiền.";
                return RedirectToAction("Details", "HoaDon", new { id = phieu.ID_HoaDon });
            }

            // 1) Cập nhật phiếu
            phieu.TrangThai = ReturnStatus.DaHoanTien;
            phieu.NhanVienXuLy = User?.Identity?.Name ?? "system";
            if (!string.IsNullOrWhiteSpace(ghiChu))
                phieu.GhiChu = AppendNote(phieu.GhiChu, ghiChu);

            // 2) CHUYỂN TRẠNG THÁI ĐƠN -> "7: Hoàn hàng thành công"
            var hd = phieu.HoaDon!;
            hd.TrangThai = 7; // TT_HOAN_HANG_THANH_CONG
            hd.NgayCapNhat = DateTime.Now;

            // 3) Ghi log rõ ràng (trạng thái 7)
            _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = hd.ID_HoaDon,
                TrangThai = 7,
                NgayChuyen = DateTime.Now,
                NhanVienDoi = phieu.NhanVienXuLy,
                NoiDungDoi = $"Đã hoàn tiền {phieu.TongTienHoan:N0} VND (phiếu {phieu.ID_TraHang.ToString()[..8]})."
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xác nhận hoàn tiền và cập nhật đơn: Hoàn hàng thành công.";
            return RedirectToAction("Details", "HoaDon", new { id = phieu.ID_HoaDon });
        }


        // ============= TỪ CHỐI YÊU CẦU =============
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TuChoi(Guid traHangId, string lyDo)
        {
            var phieu = await _context.QuanLyTraHangs
                .Include(p => p.HoaDon)
                .FirstOrDefaultAsync(p => p.ID_TraHang == traHangId);
            if (phieu == null) return NotFound();

            phieu.TrangThai = ReturnStatus.TuChoi;
            phieu.NhanVienXuLy = User?.Identity?.Name ?? "system";
            phieu.GhiChu = AppendNote(phieu.GhiChu, lyDo);

            _context.TrangThaiDonHangs.Add(new TrangThaiDonHang
            {
                ID_TrangThaiDonHang = Guid.NewGuid(),
                ID_HoaDon = phieu.ID_HoaDon,
                TrangThai = phieu.HoaDon.TrangThai,
                NgayChuyen = DateTime.Now,
                NhanVienDoi = phieu.NhanVienXuLy,
                NoiDungDoi = $"Từ chối yêu cầu hoàn hàng (phiếu {phieu.ID_TraHang.ToString()[..8]}): {lyDo}"
            });

            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã từ chối yêu cầu hoàn hàng.";
            return RedirectToAction("Details", "HoaDon", new { id = phieu.ID_HoaDon });
        }

        // Helpers
        private static string AppendNote(string? oldNote, string add)
            => string.IsNullOrWhiteSpace(oldNote) ? add : (oldNote + " | " + add);

        private static void CongTonKho(object sanPhamChiTiet, int soLuongCong)
        {
            var t = sanPhamChiTiet.GetType();
            var p = t.GetProperty("SoLuongTon")
                  ?? t.GetProperty("SoLuong")
                  ?? t.GetProperty("TonKho")
                  ?? t.GetProperty("SoLuong_TonKho");
            if (p == null) return;

            var cur = Convert.ToInt32(p.GetValue(sanPhamChiTiet) ?? 0);
            p.SetValue(sanPhamChiTiet, cur + soLuongCong);
        }
    }

    // (Giữ ViewModel nếu còn dùng CreateFromOrder)
    public class CreateReturnVM
    {
        public Guid HoaDonId { get; set; }
        public string LyDo { get; set; } = "";
        public Dictionary<Guid, ReturnItemVM> Items { get; set; } = new();
    }
    public class ReturnItemVM
    {
        public Guid HoaDonChiTietId { get; set; }
        public int SoLuongHoan { get; set; }
    }
}