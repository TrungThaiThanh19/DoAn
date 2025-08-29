using DoAn.IService;
using DoAn.Models;
using DoAn.ViewModel;
using DoAn.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace DoAn.Service
{
    public class StatisticsService : IStatisticsService
    {
        private readonly DoAnDbContext _db;
        public StatisticsService(DoAnDbContext db) => _db = db;

        public async Task<StatsDashboardVM> BuildDashboardAsync(
    DateTime? from, DateTime? to, int[]? status, int topN, CancellationToken ct)
        {
            // ==== Query + Include cần thiết (KHÔNG include HoaDonChiTiet từ ChiTietTraHang) ====
            var q = _db.HoaDons
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(spct => spct.SanPham)
                            .ThenInclude(sp => sp.ThuongHieu)
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(spct => spct.SanPham)
                            .ThenInclude(sp => sp.GioiTinh)
                .Include(h => h.HoaDonChiTiets)
                    .ThenInclude(ct => ct.SanPhamChiTiet)
                        .ThenInclude(spct => spct.TheTich)
                .Include(h => h.TraHangs)
                    .ThenInclude(tr => tr.ChiTietTraHangs) // <- đủ rồi
                .Include(h => h.NhanVien)
                .AsQueryable();

            if (from.HasValue) q = q.Where(h => h.NgayTao >= from.Value.Date);
            if (to.HasValue) q = q.Where(h => h.NgayTao < to.Value.Date.AddDays(1)); // < ngày+1

            if (status?.Any() == true) q = q.Where(h => status.Contains(h.TrangThai));

            var hoaDons = await q.AsNoTracking().ToListAsync(ct);

            // ------- TÍNH TOÁN CHUẨN (KHÔNG TRỪ THUẾ) -------
            // Doanh thu gộp = (sau giảm) + phụ thu KH trả
            // (TongTienSauGiam là decimal không-nullable => KHÔNG dùng ??)
            decimal revenueGross = hoaDons.Sum(hd => hd.TongTienSauGiam)
                                 + hoaDons.Sum(hd => ((decimal?)hd.PhuThu ?? 0m)); // an toàn cho cả decimal/decimal?

            // Doanh thu hàng (không gồm phụ thu) để tính tỷ lệ COGS cho hoàn
            decimal revenueItemsGross = hoaDons.Sum(hd =>
                hd.HoaDonChiTiets.Sum(ct => ct.SoLuong * ct.DonGia)
            );

            // COGS gộp
            decimal cogsGross = hoaDons.Sum(hd =>
                hd.HoaDonChiTiets.Sum(ct => ct.SoLuong * (((decimal?)ct.SanPhamChiTiet?.GiaNhap) ?? 0m))
            );

            // Hoàn doanh thu (tiền trả lại KH)
            decimal refundRevenue = hoaDons.Sum(hd =>
                hd.TraHangs.Sum(tr => tr.ChiTietTraHangs.Sum(r => ((decimal?)r.TienHoan ?? 0m)))
            );

            // Hoàn giá vốn: KHÔNG có khóa tới dòng hàng → ước tính theo tỷ lệ COGS
            // ratio = cogsGross / revenueItemsGross (bảo vệ chia cho 0)
            decimal cogsRatio = revenueItemsGross > 0m ? (cogsGross / revenueItemsGross) : 0m;
            decimal refundCOGS = refundRevenue * cogsRatio;

            // Net
            decimal revenueNet = revenueGross - refundRevenue;
            decimal cogsNet = cogsGross - refundCOGS;

            // Phí vận chuyển shop trả (chưa có bảng riêng → 0). KHÔNG lấy PhuThu làm chi phí!
            decimal shipCostPaidByShop = 0m;

            // Giảm giá (hiển thị) — hai trường này nhiều khả năng là decimal không-nullable → KHÔNG dùng ??
            decimal giamGia = hoaDons.Sum(hd => hd.TongTienTruocGiam - hd.TongTienSauGiam);

            var vm = new StatsDashboardVM
            {
                KPI = new StatsKpiVM
                {
                    DoanhThu = revenueNet,                    // thực thu sau hoàn
                    TongGiaNhap = cogsNet,                    // COGS sau hoàn (ước tính theo tỷ lệ)
                    ChiPhiVanChuyen = shipCostPaidByShop,
                    HoanTienTraHang = refundRevenue,
                    DonHoanTat = hoaDons.Count,
                    KhachHangMoi = await _db.KhachHangs.CountAsync(x => x.NgayTao.Year == DateTime.Now.Year, ct),
                    GiamGiaKM_Voucher = giamGia,
                    PhuThu = hoaDons.Sum(hd => ((decimal?)hd.PhuThu ?? 0m))
                    // KHÔNG gán LoiNhuan vì StatsKpiVM của bạn tự tính (read-only)
                }
            };

            // --------- DÒNG HÀNG/ TOP: chỉ dùng field có thật (DonGia, SoLuong) ----------
            var lines = hoaDons.SelectMany(hd => hd.HoaDonChiTiets.Select(ct => new
            {
                Ct = ct,
                Spct = ct.SanPhamChiTiet!,
                Sp = ct.SanPhamChiTiet!.SanPham!,
                BrandName = ct.SanPhamChiTiet!.SanPham!.ThuongHieu!.Ten_ThuongHieu,
                GenderName = ct.SanPhamChiTiet!.SanPham!.GioiTinh!.Ten_GioiTinh,
                TheTichGiaTri = ct.SanPhamChiTiet!.TheTich!.GiaTri,
                TheTichDonVi = ct.SanPhamChiTiet!.TheTich!.DonVi,
                DoanhThuDong = (decimal)ct.SoLuong * ct.DonGia
            })).ToList();

            vm.TopProducts = lines
                .GroupBy(x => x.Spct.ID_SanPhamChiTiet)
                .Select(g => new TopProductVM
                {
                    ID_SanPhamChiTiet = g.Key,
                    TenSanPham = g.First().Sp.Ten_SanPham,
                    TheTich = $"{g.First().TheTichGiaTri:0.##} {g.First().TheTichDonVi}",
                    SoLuong = g.Sum(x => x.Ct.SoLuong),
                    DoanhThu = g.Sum(x => x.DoanhThuDong)
                })
                .OrderByDescending(x => x.DoanhThu).ThenByDescending(x => x.SoLuong)
                .Take(topN)
                .ToList();

            vm.SalesByBrand = lines
                .GroupBy(x => x.BrandName)
                .Select(g => new BrandSalesVM
                {
                    TenThuongHieu = g.Key,
                    SoLuong = g.Sum(x => x.Ct.SoLuong),
                    DoanhThu = g.Sum(x => x.DoanhThuDong)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            vm.SalesByGender = lines
                .GroupBy(x => x.GenderName)
                .Select(g => new GenderSalesVM
                {
                    TenGioiTinh = g.Key,
                    SoLuong = g.Sum(x => x.Ct.SoLuong),
                    DoanhThu = g.Sum(x => x.DoanhThuDong)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            vm.SalesByStaff = hoaDons
                .GroupBy(h => h.NhanVien != null ? h.NhanVien.Ten_NhanVien : "(Chưa gán)")
                .Select(g => new StaffPerformanceVM
                {
                    TenNhanVien = g.Key,
                    SoDon = g.Count(),
                    DoanhThu = g.Sum(h => h.TongTienSauGiam) // decimal không-nullable
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            // ===== Doanh thu theo NGÀY =====
            var fromDate = (from?.Date) ?? DateTime.Today;
            var toDateExcl = (to?.Date ?? DateTime.Today).AddDays(1); // exclusive

            decimal OrderNet(HoaDon hd) =>
                hd.TongTienSauGiam
              + ((decimal?)hd.PhuThu ?? 0m)
              - hd.TraHangs.Sum(tr => tr.ChiTietTraHangs.Sum(r => ((decimal?)r.TienHoan ?? 0m)));

            var daily = new List<RevenuePointVM>();
            for (var d = fromDate; d < toDateExcl; d = d.AddDays(1))
            {
                var amt = hoaDons
                    .Where(h => h.NgayTao.Date == d.Date)
                    .Sum(h => OrderNet(h));

                daily.Add(new RevenuePointVM { Time = d, Amount = amt });
            }
            vm.RevenueDaily = daily;

            // ===== Doanh thu theo THÁNG (năm hiện tại) =====
            int year = DateTime.Now.Year;
            var monthly = new List<RevenuePointVM>();
            for (int m = 1; m <= 12; m++)
            {
                var amt = hoaDons
                    .Where(h => h.NgayTao.Year == year && h.NgayTao.Month == m)
                    .Sum(h => OrderNet(h));

                monthly.Add(new RevenuePointVM { Time = new DateTime(year, m, 1), Amount = amt });
            }
            vm.RevenueMonthly = monthly;
            return vm;
        }
    }
}