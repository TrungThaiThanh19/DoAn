using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class ChiTietGioHang
    {
        [Key]
        public Guid ID_ChiTietGioHang { get; set; }
        public int SoLuong { get; set; }

        public Guid ID_SanPhamChiTiet { get; set; }
        public SanPhamChiTiet SanPhamChiTiet { get; set; }

        public Guid ID_GioHang { get; set; }
        public GioHang GioHang { get; set; }
    }

}
