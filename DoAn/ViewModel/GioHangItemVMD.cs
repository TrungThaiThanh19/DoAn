using System;

namespace DoAn.ViewModel
{
    public class GioHangItemVMD
    {
        public Guid ChiTietGioHangId { get; set; }
        public Guid SanPhamChiTietId { get; set; }

        public string TenSanPham { get; set; } = "";
        public string? ThuongHieu { get; set; }
        public string? TheTich { get; set; }
        public string? HinhAnh { get; set; }

        public decimal DonGia { get; set; }
        public int SoLuong { get; set; }
        public decimal ThanhTien { get; set; }

        // tồn kho của biến thể (để clamp số lượng)
        public int TonKho { get; set; }

        // ===== THÊM MỚI =====
        // Danh sách khuyến mãi áp dụng (nếu bạn cần lấy chi tiết để tính toán)
        public ICollection<DoAn.Models.ChiTietKhuyenMai>? ChiTietKhuyenMais { get; set; }

        // Giá đã áp dụng khuyến mãi (nếu có)
        public decimal GiaSauKhuyenMai { get; set; }

        // % giảm giá (0 nếu không có KM)
        public int GiamPhanTram { get; set; }
    }
}