using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class GioHang
    {
        [Key]
        public Guid ID_GioHang { get; set; }

        public Guid ID_KhachHang { get; set; }
        public KhachHang KhachHang { get; set; }

        public ICollection<ChiTietGioHang> ChiTietGioHangs { get; set; }
    }
}
