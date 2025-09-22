using DoAn.Models;
using DoAn.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Controllers
{
    [Authorize(Roles = "admin,nhanvien")]
    public class LichSuTraHangController : Controller
    {
        private readonly DoAnDbContext _db;

        public LichSuTraHangController(DoAnDbContext db)
        {
            _db = db;
        }

        public async Task<IActionResult> Index(Guid? hoaDonId, int? trangThai)
        {
            // Nếu không có hoaDonId, hiển thị tất cả lịch sử hoàn hàng
            if (hoaDonId == null || hoaDonId == Guid.Empty)
            {
                return await IndexAll(trangThai);
            }

            try
            {
                // Kiểm tra hóa đơn có tồn tại và lấy thông tin
                var hoaDon = await _db.HoaDons
                    .Include(h => h.KhachHang)
                    .FirstOrDefaultAsync(h => h.ID_HoaDon == hoaDonId);

                if (hoaDon == null)
                {
                    ViewBag.ErrorMessage = $"Không tìm thấy hóa đơn với ID: {hoaDonId}";
                    return View(new List<LichSuTraHangVM>());
                }

                // Truyền thông tin hóa đơn vào ViewBag
                ViewBag.HoaDonInfo = new
                {
                    MaHoaDon = hoaDon.Ma_HoaDon,
                    KhachHang = hoaDon.HoTen ?? hoaDon.KhachHang?.Ten_KhachHang ?? "Khách lẻ",
                    NgayTao = hoaDon.NgayTao
                };

                // Lấy danh sách phiếu trả hàng, chỉ hiển thị khi trạng thái = 3 (đã hoàn tiền)
                var phieuHoanList = await _db.QuanLyTraHangs
                    .Where(th => th.ID_HoaDon == hoaDonId )
                    .Include(th => th.ChiTietTraHangs)
                        .ThenInclude(ct => ct.SanPhamChiTiet)
                            .ThenInclude(spct => spct.SanPham)
                    .Include(th => th.ChiTietTraHangs)
                        .ThenInclude(ct => ct.SanPhamChiTiet)
                            .ThenInclude(spct => spct.TheTich)
                    .OrderByDescending(th => th.NgayTao)
                    .ToListAsync();

                // Map sang ViewModel
                var result = phieuHoanList.Select(phieu => new LichSuTraHangVM
                {
                    ID_TraHang = phieu.ID_TraHang,
                    LyDo = phieu.LyDo ?? "Không có lý do",
                    GhiChu = phieu.GhiChu ?? "",
                    NhanVienXuLy = phieu.NhanVienXuLy ?? "Không rõ",
                    NgayTao = phieu.NgayTao,
                    TrangThai = phieu.TrangThai,
                    TongTienHoan = phieu.TongTienHoan,
                    ID_HoaDon = phieu.ID_HoaDon,
                    ChiTietTraHangs = phieu.ChiTietTraHangs?.Select(ct => new LichSuTraHangChiTietVM
                    {
                        TenSanPham = ct.SanPhamChiTiet?.SanPham?.Ten_SanPham ?? "Không rõ sản phẩm",
                        TheTich = ct.SanPhamChiTiet?.TheTich != null
                            ? $"{ct.SanPhamChiTiet.TheTich.GiaTri}{ct.SanPhamChiTiet.TheTich.DonVi}"
                            : "Không rõ",
                        SoLuong = ct.SoLuong,
                        TienHoan = ct.TienHoan
                    }).ToList() ?? new List<LichSuTraHangChiTietVM>()
                }).ToList();

                return View(result);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi lấy dữ liệu: " + ex.Message;
                return View(new List<LichSuTraHangVM>());
            }
        }

        // Hiển thị tất cả lịch sử hoàn hàng
        private async Task<IActionResult> IndexAll(int? trangThai = null)
        {
            try
            {
                // Lấy tất cả phiếu hoàn hàng với các trạng thái: yêu cầu (0), đã duyệt (1), đã nhận hàng (2), đã hoàn tiền (3), từ chối (9)
                var query = _db.QuanLyTraHangs
                    .Where(th => (th.TrangThai >= 0 && th.TrangThai <= 3) || th.TrangThai == 9|| th.TrangThai == 6); // Hiển thị trạng thái 0, 1, 2, 3, 9
                
                // Filter theo trạng thái nếu có
                if (trangThai.HasValue)
                {
                    query = query.Where(th => th.TrangThai == trangThai.Value);
                }
                
                var phieuHoanList = await query
                    .Include(th => th.HoaDon)
                        .ThenInclude(h => h.KhachHang)
                    .Include(th => th.ChiTietTraHangs)
                        .ThenInclude(ct => ct.SanPhamChiTiet)
                            .ThenInclude(spct => spct.SanPham)
                    .Include(th => th.ChiTietTraHangs)
                        .ThenInclude(ct => ct.SanPhamChiTiet)
                            .ThenInclude(spct => spct.TheTich)
                    .OrderByDescending(th => th.NgayTao)
                    .ToListAsync();

                // Map sang ViewModel
                var result = phieuHoanList.Select(phieu => new LichSuTraHangVM
                {
                    ID_TraHang = phieu.ID_TraHang,
                    LyDo = phieu.LyDo ?? "Không có lý do",
                    GhiChu = phieu.GhiChu ?? "",
                    NhanVienXuLy = phieu.NhanVienXuLy ?? "Không rõ",
                    NgayTao = phieu.NgayTao,
                    TrangThai = phieu.TrangThai,
                    TongTienHoan = phieu.TongTienHoan,
                    ID_HoaDon = phieu.ID_HoaDon,
                    MaHoaDon = phieu.HoaDon?.Ma_HoaDon ?? "Không rõ",
                    TenKhachHang = phieu.HoaDon?.HoTen ?? phieu.HoaDon?.KhachHang?.Ten_KhachHang ?? "Khách lẻ",
                    ChiTietTraHangs = phieu.ChiTietTraHangs?.Select(ct => new LichSuTraHangChiTietVM
                    {
                        TenSanPham = ct.SanPhamChiTiet?.SanPham?.Ten_SanPham ?? "Không rõ sản phẩm",
                        TheTich = ct.SanPhamChiTiet?.TheTich != null
                            ? $"{ct.SanPhamChiTiet.TheTich.GiaTri}{ct.SanPhamChiTiet.TheTich.DonVi}"
                            : "Không rõ",
                        SoLuong = ct.SoLuong,
                        TienHoan = ct.TienHoan
                    }).ToList() ?? new List<LichSuTraHangChiTietVM>()
                }).ToList();

                // Thêm thông tin hóa đơn vào ViewBag
                ViewBag.IsAllReturns = true;
                ViewBag.CurrentStatus = trangThai;
                ViewBag.TotalReturns = result.Count;
                ViewBag.TotalAmount = result.Sum(r => r.TongTienHoan);
                
                // Thống kê theo trạng thái (lấy từ tất cả dữ liệu, không chỉ filtered)
                var allData = await _db.QuanLyTraHangs
                    .Where(th => (th.TrangThai >= 0 && th.TrangThai <= 3) || th.TrangThai == 9 || th.TrangThai == 6)
                    .ToListAsync();
                    
                ViewBag.YeuCauCount = allData.Count(r => r.TrangThai == 6);
                ViewBag.DaDuyetCount = allData.Count(r => r.TrangThai == 1);
                ViewBag.DaNhanHangCount = allData.Count(r => r.TrangThai == 2);
                ViewBag.DaHoanTienCount = allData.Count(r => r.TrangThai == 3);
                ViewBag.TuChoiCount = allData.Count(r => r.TrangThai == 9);

                return View(result);
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Có lỗi xảy ra khi lấy dữ liệu: " + ex.Message;
                return View(new List<LichSuTraHangVM>());
            }
        }

        // Xem chi tiết 1 phiếu trả hàng
        // Trong LichSuTraHangController
        public async Task<IActionResult> ChiTiet(Guid traHangId)
        {
            var phieu = await _db.QuanLyTraHangs
                .Include(th => th.HoaDon)
                    .ThenInclude(h => h.KhachHang)
                // => include chi tiết hóa đơn để có DonGia
                .Include(th => th.HoaDon)
                    .ThenInclude(h => h.HoaDonChiTiets)
                        .ThenInclude(hct => hct.SanPhamChiTiet)
                            .ThenInclude(spct => spct.SanPham)
                // chi tiết trả hàng + product info
                .Include(th => th.ChiTietTraHangs)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(spct => spct.SanPham)
                .Include(th => th.ChiTietTraHangs)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(spct => spct.TheTich)
                .FirstOrDefaultAsync(th => th.ID_TraHang == traHangId);

            if (phieu == null)
            {
                TempData["Error"] = "Không tìm thấy phiếu hoàn hàng";
                return RedirectToAction("Index", "LichSuTraHang");
            }

            return View(phieu);
        }

    }
}
