using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class GioHangChiTiet
    {
        [Key]
        public Guid IdGioHangCt { get; set; } // Khóa chính
        public Guid? IdGioHang { get; set; } // Khóa ngoại tới GioHang
        public Guid? IdCtsp { get; set; } // Khóa ngoại tới SanPhamChiTiet
        public int SoLuong { get; set; }
        public decimal Gia { get; set; }
        public decimal TongTien { get; set; }

        // Navigation properties
        public virtual GioHang? GioHang { get; set; }
        public virtual SanPhamChiTiet? SanPhamChiTiet { get; set; }
    }
}
