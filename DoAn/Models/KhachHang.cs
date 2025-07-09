using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class KhachHang
    {
        [Key]
        public Guid IdKh { get; set; } // Khóa chính
        public Guid? IdTk { get; set; } // Khóa ngoại tới TaiKhoanKhachHang
        public string TenKh { get; set; }
        public string Sdt { get; set; }
        public string Email { get; set; }
        public string DiaChi { get; set; }
        public DateTime DateTime { get; set; } = DateTime.Now; // Ngày tạo
        public bool TrangThai { get; set; }

        // Navigation properties
        public virtual TaiKhoan? TaiKhoan { get; set; }
        public virtual ICollection<DonDatHang> DonDatHangs { get; set; }
        public virtual ICollection<GioHang> GioHangs { get; set; }
    }
}
