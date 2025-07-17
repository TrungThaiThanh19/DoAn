using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models
{
    public class SanPham
    {
        [Key]
        public Guid ID_SanPham { get; set; }
        public string TenSanPham { get; set; }
        public string MoTa { get; set; }
		public int ThoiGianLuuHuong { get; set; }
		public string HuongDau { get; set; } 
		public string HuongGiua { get; set; } 
		public string HuongCuoi { get; set; }
		public string HinhAnh { get; set; }
		public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }
		public Guid ID_ThuongHieu { get; set; }
		[ForeignKey("ID_ThuongHieu")]
		public ThuongHieu ThuongHieu { get; set; }
		public Guid ID_QuocGia { get; set; }
		[ForeignKey("ID_QuocGia")]
		public QuocGia QuocGia { get; set; }
		public Guid ID_GioiTinh { get; set; }
		[ForeignKey("ID_GioiTinh")]
		public GioiTinh GioiTinh { get; set; }
		public ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
