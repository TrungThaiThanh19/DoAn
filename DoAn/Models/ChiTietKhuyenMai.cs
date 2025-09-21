using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class ChiTietKhuyenMai
    {
        [Key]
        public Guid ID_ChiTietKhuyenMai { get; set; }
        public decimal GiaSauGiam { get; set; }

        public Guid ID_SanPhamChiTiet { get; set; }
        public SanPhamChiTiet SanPhamChiTiet { get; set; }

        public Guid ID_KhuyenMai { get; set; }
        public KhuyenMai KhuyenMai { get; set; }
    }
}
