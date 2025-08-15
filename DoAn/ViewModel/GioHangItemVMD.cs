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
    }
}
