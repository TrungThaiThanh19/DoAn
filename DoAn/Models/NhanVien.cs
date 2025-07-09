using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class NhanVien
    {
        [Key]
        public Guid IdNv { get; set; } // Khóa chính
        public Guid? IdTk { get; set; } // Khóa ngoại tới TaiKhoanNhanVien
        public string TenNv { get; set; }
        public string DiaChi { get; set; }
        public string SoCccd { get; set; }
        public bool TrangThai { get; set; }

        // Navigation properties
        public virtual TaiKhoan? TaiKhoan { get; set; } // Tên Navigation Property khớp với tên class
        public virtual ICollection<HoaDon> HoaDons { get; set; }
        
    }
}
