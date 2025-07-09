using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class ChiTietDonDatHang
    {
        [Key]
        public Guid IdDdhct { get; set; } // PK
        public Guid IdDdh { get; set; } // FK tới DonDatHang
        public Guid IdCtsp { get; set; } // FK tới SanPhamChiTiet
        public int SoLuong { get; set; } 
        public decimal DonGia { get; set; } 


        // Navigation properties
        public virtual DonDatHang DonDatHang { get; set; }
        public virtual SanPhamChiTiet SanPhamChiTiet { get; set; }
    }
}
