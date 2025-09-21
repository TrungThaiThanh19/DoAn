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
                    .ThenInclude(tr => tr.ChiTietTraHangs)
                .Include(h => h.NhanVien)
                .AsQueryable();

            // Chỉ lấy đơn hàng trạng thái thành công
            q = q.Where(h => h.TrangThai == 4);

            if (from.HasValue) q = q.Where(h => h.NgayTao >= from.Value.Date);
            if (to.HasValue) q = q.Where(h => h.NgayTao < to.Value.Date.AddDays(1)); // < ngày+1

            var hoaDons = await q.AsNoTracking().ToListAsync(ct);

            // ------- TÍNH TOÁN CHUẨN (KHÔNG TRỪ THUẾ) -------
            decimal revenueGross = hoaDons.Sum(hd => hd.TongTienSauGiam)
                                 + hoaDons.Sum(hd => ((decimal?)hd.PhuThu ?? 0m));

            decimal revenueItemsGross = hoaDons.Sum(hd =>
                hd.HoaDonChiTiets.Sum(ct => ct.SoLuong * ct.DonGia)
            );

            decimal cogsGross = hoaDons.Sum(hd =>
                hd.HoaDonChiTiets.Sum(ct => ct.SoLuong * (((decimal?)ct.SanPhamChiTiet?.GiaNhap) ?? 0m))
            );

            decimal refundRevenue = hoaDons.Sum(hd =>
                hd.TraHangs.Sum(tr => tr.ChiTietTraHangs.Sum(r => ((decimal?)r.TienHoan ?? 0m)))
            );

            decimal cogsRatio = revenueItemsGross > 0m ? (cogsGross / revenueItemsGross) : 0m;
            decimal refundCOGS = refundRevenue * cogsRatio;

            decimal revenueNet = revenueGross - refundRevenue;
            decimal cogsNet = cogsGross - refundCOGS;

            decimal shipCostPaidByShop = 0m;

            decimal giamGia = hoaDons.Sum(hd => hd.TongTienTruocGiam - hd.TongTienSauGiam);

            var vm = new StatsDashboardVM
            {
                KPI = new StatsKpiVM
                {
                    DoanhThu = revenueNet,
                    TongGiaNhap = cogsNet,
                    ChiPhiVanChuyen = shipCostPaidByShop,
                    HoanTienTraHang = refundRevenue,
                    DonHoanTat = hoaDons.Count,
                    KhachHangMoi = await _db.KhachHangs.CountAsync(x => x.NgayTao.Year == DateTime.Now.Year, ct),
                    GiamGiaKM_Voucher = giamGia,
                    PhuThu = hoaDons.Sum(hd => ((decimal?)hd.PhuThu ?? 0m))
                }
            };

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
                    DoanhThu = g.Sum(h => h.TongTienSauGiam)
                })
                .OrderByDescending(x => x.DoanhThu)
                .ToList();

            // ===== Doanh thu theo NGÀY =====
            var fromDate = (from?.Date) ?? DateTime.Today;
            var toDateExcl = (to?.Date ?? DateTime.Today).AddDays(1);

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