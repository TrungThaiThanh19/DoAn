using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class Voucher
    {
        [Key]
        public Guid ID_Voucher { get; set; }
        public string Ma_Voucher { get; set; }
        public string Ten_Voucher { get; set; }
        public DateTime NgayTao { get; set; }
        public DateTime NgayHetHan { get; set; }
        public string KieuGiamGia { get; set; }
        public decimal GiaTriGiam { get; set; }
        public decimal GiaTriToiThieu { get; set; }  
        public decimal GiaTriToiDa { get; set; }  
        public int SoLuong { get; set; }
        public int TrangThai { get; set; } 
        public string MoTa { get; set; }

        public Guid ID_TaiKhoan { get; set; }
        public TaiKhoan TaiKhoan { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
    }
}
