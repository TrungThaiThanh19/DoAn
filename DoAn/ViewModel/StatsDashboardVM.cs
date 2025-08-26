namespace DoAn.ViewModel
{
    public class StatsDashboardVM
    {
        public StatsKpiVM KPI { get; set; }
        public List<RevenuePointVM> RevenueDaily { get; set; } = new();
        public List<RevenuePointVM> RevenueMonthly { get; set; } = new();
        public List<TopProductVM> TopProducts { get; set; } = new();
        public List<BrandSalesVM> SalesByBrand { get; set; } = new();
        public List<GenderSalesVM> SalesByGender { get; set; } = new();
        public List<StaffPerformanceVM> SalesByStaff { get; set; } = new();
        public List<RevenuePointVM> NewCustomersByMonth { get; set; } = new();
    }
}