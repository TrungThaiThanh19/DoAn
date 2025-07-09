using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class PhuongThucThanhToan
    {
        [Key]
        public Guid IdPhuongThucThanhToan { get; set; } // PK
        public string TenPT { get; set; }
        public string MoTa { get; set; }
        public DateTime NgayTao { get; set; }= DateTime.Now; // Ngày tạo, mặc định là ngày hiện tại

        public virtual ICollection<HoaDon> HoaDons { get; set; }
        public virtual ICollection<DonDatHang> DonDatHangs { get; set; }
    }
}
