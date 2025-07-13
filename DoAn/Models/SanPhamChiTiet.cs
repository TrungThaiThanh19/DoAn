using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class SanPhamChiTiet
    {
        [Key]
        public Guid ID_SanPhamChiTiet { get; set; }
        public decimal GiaBan { get; set; }
        public int SoLuong { get; set; }
        public int TrangThai { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        public Guid ID_TheTich { get; set; }
        public TheTich TheTich { get; set; }

        public Guid ID_GioiTinh { get; set; }
        public GioiTinh GioiTinh { get; set; }
        public ICollection<ChiTietKhuyenMai> ChiTietKhuyenMais { get; set; }

        public ICollection<HinhAnh> HinhAnhs { get; set; }
        public ICollection<ChiTietTraHang> ChiTietTraHangs { get; set; }
        public ICollection<HoaDonChiTiet> HoaDonChiTiets { get; set; }
    }
}
