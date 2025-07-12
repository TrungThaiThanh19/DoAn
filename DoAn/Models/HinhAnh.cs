using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class HinhAnh
    {
        [Key]
        public Guid ID_HinhAnh { get; set; }
        public string HinhAnhURL { get; set; }

        public Guid ID_ChiTietSanPham { get; set; }
        public SanPhamChiTiet SanPhamChiTiet { get; set; }
    }
}
