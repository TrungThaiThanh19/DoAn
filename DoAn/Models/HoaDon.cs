using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class HoaDon
    {
        [Key]
        public Guid IdHd { get; set; } // Khóa chính
        public Guid? IdNv { get; set; } // Khóa ngoại tới NhanVien
        public Guid? ID_VCH { get; set; } // Khóa ngoại tới Voucher (nếu có)
        public decimal TongTien { get; set; }
        public string SdtKh { get; set; }
        public string TenKh { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now; // Ngày tạo, mặc định là ngày hiện tại

        // Navigation properties
        public virtual NhanVien? NhanVien { get; set; }
        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } // Sẽ tạo ChiTietHoaDon mới
        public virtual Voucher? Voucher { get; set; } // Sẽ tạo Voucher mới nếu có
    }
}

