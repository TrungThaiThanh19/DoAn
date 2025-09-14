namespace DoAn.ViewModel
{
    public class KhuyenMaiIndexItemVM
    {
        public Guid ID_KhuyenMai { get; set; }
        public string Ma_KhuyenMai { get; set; }
        public string Ten_KhuyenMai { get; set; }
        public string KieuGiamGia { get; set; }
        public decimal GiaTriGiam { get; set; }
        public decimal GiaTriToiDa { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayHetHan { get; set; }
        public int TrangThai { get; set; }
        public int SoSPCT { get; set; }
        public bool DangHoatDong { get; set; }
    }
}