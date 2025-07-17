using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models
{
    public class SanPhamChiTiet
    {
        [Key]
        public Guid ID_SanPhamChiTiet { get; set; }
        public int GiaBan { get; set; }
		public int GiaNhap { get; set; }
		public int SoLuong { get; set; }
        public int TrangThai { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }
        public Guid ID_TheTich { get; set; }
        public TheTich TheTich { get; set; }
		public Guid ID_SanPham { get; set; }
        [ForeignKey("ID_SanPham")]
		public SanPham SanPham { get; set; }
		public ICollection<ChiTietKhuyenMai> ChiTietKhuyenMais { get; set; }
        public ICollection<ChiTietTraHang> ChiTietTraHangs { get; set; }
        public ICollection<HoaDonChiTiet> HoaDonChiTiets { get; set; }
    }
}
