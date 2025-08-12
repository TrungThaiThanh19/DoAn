namespace DoAn.ViewModel
{
    public class ChiTietGioHangVMD
    {
        public Guid ChiTietGioHangId { get; set; }
        public Guid SanPhamChiTietId { get; set; }

        public string TenSanPham { get; set; }
        public string HinhAnh { get; set; }
        public string TheTich { get; set; } // ví dụ "100 ml"

        public decimal DonGia { get; set; } // lấy từ SanPhamChiTiet
        public int SoLuong { get; set; }
        public decimal ThanhTien => DonGia * SoLuong;
    }
}
