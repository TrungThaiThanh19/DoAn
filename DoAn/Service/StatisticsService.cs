using DoAn.IService;
using DoAn.Models;
using DoAn.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Service
{
    public class StatisticsService : IStatisticsService
    {
        private readonly DoAnDbContext _db;
        public StatisticsService(DoAnDbContext db) => _db = db;

        public async Task<StatsDashboardVM> BuildDashboardAsync(
            DateTime? from = null,
            DateTime? to = null,
            int[]? completedStatuses = null,
            int topN = 5,
            CancellationToken ct = default)
        {
            // Nếu caller không truyền, mặc định coi 4 là hoàn tất (có thể thêm 3 nếu cần)
            var completed = (completedStatuses != null && completedStatuses.Length > 0
                                ? completedStatuses
                                : new[] { 4 })
                                .ToHashSet();

            var start = (from ?? DateTime.Today.AddDays(-6)).Date;
            var end = ((to ?? DateTime.Today).Date).AddDays(1).AddTicks(-1);

            // ===== Orders & Lines trong cửa sổ thời gian và trạng thái hoàn tất =====
            var ordersQ = _db.HoaDons.AsNoTracking()
                .Where(h => h.NgayTao >= start && h.NgayTao <= end && completed.Contains(h.TrangThai));

            var orderDetailsQ = _db.HoaDonChiTiets.AsNoTracking()
                .Where(d => completed.Contains(d.HoaDon.TrangThai) &&
                            d.HoaDon.NgayTao >= start && d.HoaDon.NgayTao <= end)
                .Include(d => d.SanPhamChiTiet).ThenInclude(s => s.SanPham).ThenInclude(p => p.ThuongHieu)
                .Include(d => d.SanPhamChiTiet).ThenInclude(s => s.SanPham).ThenInclude(p => p.GioiTinh)
                .Include(d => d.SanPhamChiTiet).ThenInclude(s => s.TheTich)
                .Include(d => d.HoaDon);

            // ===== KPI =====
            var orders = await ordersQ
                .Select(h => new
                {
                    h.NgayTao,
                    h.TongTienTruocGiam,
                    h.TongTienSauGiam,
                    h.PhuThu
                })
                .ToListAsync(ct);

            // Doanh thu ròng ≈ TongTienSauGiam + PhuThu (nếu TongTienSauGiam đã gồm phụ thu, đổi lại = TongTienSauGiam)
            var doanhThu = orders.Sum(h => h.TongTienSauGiam + (h.PhuThu ?? 0m));
            var giamGia = orders.Sum(h => h.TongTienTruocGiam - h.TongTienSauGiam);
            var phuThu = orders.Sum(h => (h.PhuThu ?? 0m));
            var donHoanTat = orders.Count;

            // Lợi nhuận gộp xấp xỉ từ dòng HĐ
            var lines = await orderDetailsQ
                .Select(d => new
                {
                    d.SoLuong,
                    DonGiaBan = d.DonGia,
                    GiaNhap = d.SanPhamChiTiet.GiaNhap
                })
                .ToListAsync(ct);

            var loiNhuanGop = lines.Sum(x => (x.DonGiaBan - x.GiaNhap) * x.SoLuong);

            // Hoàn tiền trả hàng (placeholder)
            decimal hoanTienTraHang = 0m;

            // ===== KH mới theo tháng (năm hiện tại) — 2 bước để tránh lỗi dịch LINQ =====
            var year = DateTime.Today.Year;

            var khMoiRaw = await _db.KhachHangs.AsNoTracking()
                .Where(k => k.NgayTao.Year == year)
                .GroupBy(k => new { k.NgayTao.Year, k.NgayTao.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync(ct); // materialize trước

            var khMoiTheoThang = khMoiRaw
                .Select(x => new RevenuePointVM
                {
                    Time = new DateTime(x.Year, x.Month, 1),
                    Amount = x.Count
                })
                .OrderBy(x => x.Time)
                .ToList();

            // ===== Doanh thu theo ngày (trong khoảng) — đang dùng 'orders' đã về bộ nhớ nên OK =====
            var revenueDaily = orders
                .GroupBy(h => h.NgayTao.Date)
                .Select(g => new RevenuePointVM
                {
                    Time = g.Key,
                    Amount = g.Sum(x => x.TongTienSauGiam + (x.PhuThu ?? 0m))
                })
                .OrderBy(x => x.Time)
                .ToList();

            // ===== Doanh thu theo tháng (năm hiện tại) — 2 bước để chắc chắn =====
            var revenueMonthlyRaw = await _db.HoaDons.AsNoTracking()
                .Where(h => h.NgayTao.Year == year && completed.Contains(h.TrangThai))
                .GroupBy(h => new { h.NgayTao.Year, h.NgayTao.Month })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Amount = g.Sum(x => x.TongTienSauGiam + (x.PhuThu ?? 0m))
                })
                .ToListAsync(ct);

            var revenueMonthly = revenueMonthlyRaw
                .Select(x => new RevenuePointVM
                {
                    Time = new DateTime(x.Year, x.Month, 1),
                    Amount = x.Amount
                })
                .OrderBy(x => x.Time)
                .ToList();

            // ===== Top SP =====
            var topProducts = await orderDetailsQ
                .GroupBy(d => new
                {
                    d.ID_SanPhamChiTiet,
                    d.SanPhamChiTiet.SanPham.Ten_SanPham,
                    TheTich = d.SanPhamChiTiet.TheTich.GiaTri + " " + d.SanPhamChiTiet.TheTich.DonVi
                })
                .Select(g => new TopProductVM
                {
                    ID_SanPhamChiTiet = g.Key.ID_SanPhamChiTiet,
                    TenSanPham = g.Key.Ten_SanPham,
                    TheTich = g.Key.TheTich,
                    SoLuong = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.DonGia * x.SoLuong)
                })
                .OrderByDescending(x => x.SoLuong)
                .Take(topN)
                .ToListAsync(ct);

            // ===== Theo thương hiệu =====
            var salesByBrand = await orderDetailsQ
                .GroupBy(d => d.SanPhamChiTiet.SanPham.ThuongHieu.Ten_ThuongHieu)
                .Select(g => new BrandSalesVM
                {
                    TenThuongHieu = g.Key,
                    SoLuong = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.DonGia * x.SoLuong)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToListAsync(ct);

            // ===== Theo giới tính =====
            var salesByGender = await orderDetailsQ
                .GroupBy(d => d.SanPhamChiTiet.SanPham.GioiTinh.Ten_GioiTinh)
                .Select(g => new GenderSalesVM
                {
                    TenGioiTinh = g.Key,
                    SoLuong = g.Sum(x => x.SoLuong),
                    DoanhThu = g.Sum(x => x.DonGia * x.SoLuong)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToListAsync(ct);

            // ===== Hiệu suất nhân viên =====
            var salesByStaff = await _db.HoaDons.AsNoTracking()
                .Where(h => h.NgayTao >= start && h.NgayTao <= end && completed.Contains(h.TrangThai))
                .Include(h => h.NhanVien)
                .GroupBy(h => new { h.ID_NhanVien, Ten = h.NhanVien != null ? h.NhanVien.Ten_NhanVien : "(Không gán)" })
                .Select(g => new StaffPerformanceVM
                {
                    ID_NhanVien = g.Key.ID_NhanVien,
                    TenNhanVien = g.Key.Ten,
                    SoDon = g.Count(),
                    DoanhThu = g.Sum(x => x.TongTienSauGiam + (x.PhuThu ?? 0m))
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToListAsync(ct);

            return new StatsDashboardVM
            {
                KPI = new StatsKpiVM
                {
                    DoanhThu = doanhThu,
                    LoiNhuanGop = loiNhuanGop,
                    DonHoanTat = donHoanTat,
                    KhachHangMoi = khMoiTheoThang.Where(x => x.Time.Year == year).Sum(x => (int)x.Amount),
                    GiamGiaKM_Voucher = giamGia,
                    PhuThu = phuThu,
                    HoanTienTraHang = hoanTienTraHang
                },
                RevenueDaily = revenueDaily,
                RevenueMonthly = revenueMonthly,
                TopProducts = topProducts,
                SalesByBrand = salesByBrand,
                SalesByGender = salesByGender,
                SalesByStaff = salesByStaff,
                NewCustomersByMonth = khMoiTheoThang
            };
        }
    }
}