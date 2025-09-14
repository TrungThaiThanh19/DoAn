using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class ChiTietTraHang
    {
        [Key]
        public Guid ID_ChiTietTraHang { get; set; }
        public int SoLuong { get; set; }
        public decimal TienHoan { get; set; }

        public Guid ID_ChiTietSanPham { get; set; }
        public SanPhamChiTiet SanPhamChiTiet { get; set; }

        public Guid ID_TraHang { get; set; }
        public QuanLyTraHang TraHang { get; set; }
    }
}
