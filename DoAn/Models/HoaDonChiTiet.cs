using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models
{
    public class HoaDonChiTiet
    {
        [Key]
        public Guid ID_HoaDonChiTiet { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        
        public Guid ID_HoaDon { get; set; }
        public HoaDon HoaDon { get; set; }
        
        public Guid ID_SanPhamChiTiet { get; set; }
        public SanPhamChiTiet SanPhamChiTiet { get; set; }
    }
}
