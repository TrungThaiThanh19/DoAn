using DoAn.IService;
using DoAn.Models;

namespace DoAn.Service
{
    public class HoaDonService : IHoaDonService
    {
        private readonly DoAnDbContext _context;
        public HoaDonService(DoAnDbContext context)
        {
            _context = context;
        }

        public string GenerateMaHoaDon()
        {
            string prefix = "HD";
            string datePart = DateTime.Now.ToString("yyyyMMdd");

            int countToday = _context.HoaDons
                .Count(h => h.NgayTao.Date == DateTime.Today);

            int nextNumber = countToday + 1;
            string numberPart = nextNumber.ToString("D3");

            return $"{prefix}{datePart}-{numberPart}";
        }
    }
}
