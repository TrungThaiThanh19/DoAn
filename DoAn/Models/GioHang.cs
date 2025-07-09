using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class GioHang
    {
        [Key]
        public Guid IdGioHang { get; set; } // Khóa chính
        public Guid? IdKh { get; set; } // Khóa ngoại tới KhachHang
        public DateTime NgayTao { get; set; }
        public bool TrangThai { get; set; }

        // Navigation property
        public virtual KhachHang KhachHang { get; set; }
        public virtual ICollection<GioHangChiTiet> GioHangChiTiets { get; set; }
    }
}
