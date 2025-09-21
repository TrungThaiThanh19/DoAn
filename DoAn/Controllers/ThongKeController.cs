using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DoAn.IService;
using DoAn.Service;
using DoAn.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DoAn.Controllers
{
    // Chỉ admin và nhân viên mới truy cập được trang thống kê
    [Authorize(Roles = "admin")]
    public class ThongKeController : Controller
    {
        private readonly IStatisticsService _stats;
        public ThongKeController(IStatisticsService stats)
        {
            _stats = stats;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            DateTime? from,
            DateTime? to,
            [FromQuery] int[]? status,
            int topN = 5,
            CancellationToken ct = default)
        {
            // Nếu user chọn from > to => hoán đổi lại để tránh lỗi
            if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
                (from, to) = (to, from);

            // Gọi service thống kê để lấy dữ liệu
            var vm = await _stats.BuildDashboardAsync(from, to, status, topN, ct);

            // Trả về View (Views/ThongKe/Index.cshtml) với dữ liệu vm
            return View(vm);
        }

        /// <summary>
        /// API JSON trả về doanh thu theo ngày.
        /// Dùng cho Ajax/Chart (frontend vẽ biểu đồ đường).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RevenueDaily(
            DateTime? from,
            DateTime? to,
            [FromQuery] int[]? status,
            CancellationToken ct = default)
        {
            if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
                (from, to) = (to, from);

            var vm = await _stats.BuildDashboardAsync(from, to, status, topN: 5, ct);

            // Trả dữ liệu dạng JSON { time: "yyyy-MM-dd", amount: số tiền }
            return Json(vm.RevenueDaily.Select(x => new
            {
                time = x.Time.ToString("yyyy-MM-dd"),
                amount = x.Amount
            }));
        }

        /// <summary>
        /// API JSON trả về doanh thu theo tháng (năm hiện tại).
        /// Dùng cho Ajax/Chart (frontend vẽ biểu đồ cột).
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> RevenueMonthly(
            [FromQuery] int[]? status,
            CancellationToken ct = default)
        {
            var vm = await _stats.BuildDashboardAsync(null, null, status, topN: 5, ct);

            return Json(vm.RevenueMonthly.Select(x => new
            {
                time = x.Time.ToString("yyyy-MM"),
                amount = x.Amount
            }));
        }

        /// <summary>
        /// API JSON trả về danh sách Top sản phẩm bán chạy.
        /// Dùng cho Ajax/Chart hoặc bảng động ở frontend.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> TopProducts(
            DateTime? from,
            DateTime? to,
            [FromQuery] int[]? status,
            int topN = 5,
            CancellationToken ct = default)
        {
            if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
                (from, to) = (to, from);

            var vm = await _stats.BuildDashboardAsync(from, to, status, topN, ct);

            // Trả JSON với các trường cơ bản để hiển thị
            return Json(vm.TopProducts.Select(p => new
            {
                id = p.ID_SanPhamChiTiet,
                ten = p.TenSanPham,
                theTich = p.TheTich,
                soLuong = p.SoLuong,
                doanhThu = p.DoanhThu
            }));
        }

        /// <summary>
        /// Xuất KPI tổng quan ra file CSV để tải về.
        /// File gồm các chỉ số: Doanh thu, Lợi nhuận, Số đơn hoàn tất,
        /// KH mới, Giảm giá, Phụ thu, Hoàn tiền trả hàng.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> ExportKpiCsv(
    DateTime? from,
    DateTime? to,
    [FromQuery] int[]? status,
    CancellationToken ct = default)
        {
            if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
                (from, to) = (to, from);

            var vm = await _stats.BuildDashboardAsync(from, to, status, topN: 5, ct);

            // Áp công thức mới: Lợi nhuận = Tổng tiền - Giá nhập - Chi phí vận chuyển
            var loiNhuan = vm.KPI.DoanhThu - vm.KPI.TongGiaNhap - vm.KPI.ChiPhiVanChuyen;

            var sb = new StringBuilder();
            sb.AppendLine("ChiSo,GiáTrị");
            sb.AppendLine($"DoanhThu,{vm.KPI.DoanhThu}");
            sb.AppendLine($"TongGiaNhap,{vm.KPI.TongGiaNhap}");
            sb.AppendLine($"ChiPhiVanChuyen,{vm.KPI.ChiPhiVanChuyen}");
            sb.AppendLine($"LoiNhuan,{loiNhuan}"); // <-- thay vì LoiNhuanGop

            sb.AppendLine($"DonHoanTat,{vm.KPI.DonHoanTat}");
            sb.AppendLine($"KhachHangMoi,{vm.KPI.KhachHangMoi}");
            sb.AppendLine($"GiamGiaKM_Voucher,{vm.KPI.GiamGiaKM_Voucher}");
            sb.AppendLine($"PhuThu,{vm.KPI.PhuThu}");
            sb.AppendLine($"HoanTienTraHang,{vm.KPI.HoanTienTraHang}");

            var fileName = $"kpi_{(from?.ToString("yyyyMMdd") ?? "auto")}_{(to?.ToString("yyyyMMdd") ?? "auto")}.csv";
            return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", fileName);
        }
    }
}