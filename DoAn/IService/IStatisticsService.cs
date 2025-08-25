using DoAn.ViewModel;

namespace DoAn.IService
{
    public interface IStatisticsService
    {
        Task<StatsDashboardVM> BuildDashboardAsync(
            DateTime? from = null,
            DateTime? to = null,
            int[]? completedStatuses = null, // truyền null => mặc định {4}
            int topN = 5,
            CancellationToken ct = default);
    }
}