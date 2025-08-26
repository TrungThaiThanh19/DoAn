using DoAn.Models;

namespace DoAn.Services
{
    public static class PricingExtensions
    {
        private static bool IsPercent(string kieu) =>
            string.Equals(kieu?.Trim(), "percent", StringComparison.OrdinalIgnoreCase);
        private static bool IsFixed(string kieu) =>
            string.Equals(kieu?.Trim(), "fixed", StringComparison.OrdinalIgnoreCase);

        public static bool IsActive(this KhuyenMai km, DateTime now)
        {
            if (km == null || km.TrangThai != 1) return false;
            return now >= km.NgayBatDau && now <= km.NgayHetHan; // dùng NgayHetHan của bạn
        }

        // Chọn khuyến mãi tốt nhất cho 1 SPCT
        public static (decimal finalPrice, KhuyenMai? applied, decimal discount) BestPrice(this SanPhamChiTiet spct, DateTime now)
        {
            var goc = spct.GiaBan;
            decimal best = goc; KhuyenMai? bestKm = null; decimal bestDiscount = 0;

            var kms = spct.ChiTietKhuyenMais?
                        .Where(c => c.KhuyenMai != null && c.KhuyenMai.IsActive(now))
                        .Select(c => c.KhuyenMai) ?? Enumerable.Empty<KhuyenMai>();

            foreach (var km in kms)
            {
                decimal discount = 0;
                if (IsPercent(km.KieuGiamGia))
                {
                    var pct = Math.Clamp(km.GiaTriGiam, 0, 100);
                    discount = goc * (pct / 100m);
                    if (km.GiaTriToiDa > 0 && discount > km.GiaTriToiDa) discount = km.GiaTriToiDa;
                }
                else if (IsFixed(km.KieuGiamGia))
                {
                    discount = Math.Min(goc, Math.Max(0, km.GiaTriGiam));
                }

                var price = goc - discount;
                if (price < best) { best = price; bestKm = km; bestDiscount = discount; }
            }

            return (best, bestKm, bestDiscount);
        }
    }
}