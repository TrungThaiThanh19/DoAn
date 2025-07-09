using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class ChiTietHoaDon
    {
        [Key]
        public Guid IdHdct { get; set; } // Khóa chính
        public Guid? IdHd { get; set; } // Khóa ngoại tới HoaDon
        public Guid? IdCtsp { get; set; } // Khóa ngoại tới SanPhamChiTiet

        public decimal Gia { get; set; }
        public int SoLuong { get; set; }

        // Navigation properties
        public virtual HoaDon? HoaDon { get; set; }
        public virtual SanPhamChiTiet? SanPhamChiTiet { get; set; }
    }
}
