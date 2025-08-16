using System;

namespace DoAn.ViewModels
{
    public class HoaDonViewModel
    {
        public Guid ID_HoaDon { get; set; }
        public string Ma_HoaDon { get; set; }
        public string HoTen { get; set; }
        public string NhanVienTen { get; set; }
        public string LoaiHoaDon { get; set; }
        public DateTime NgayTao { get; set; }
        public decimal TienGiam { get; set; }
        public decimal TongTienSauGiam { get; set; }
        public int TrangThai { get; set; }

        public string TrangThaiText { get; set; }
    }
}
