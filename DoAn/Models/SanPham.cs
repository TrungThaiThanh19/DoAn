using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models
{
    public class SanPham
    {
        [Key]
        public Guid ID_SanPham { get; set; }
        public string Ma_SanPham { get; set; }
        public string Ten_SanPham { get; set; }
        public string HinhAnh { get; set; }
        public string HuongDau { get; set; }
        public string HuongGiua { get; set; }
        public string HuongCuoi { get; set; }
        public int ThoiGianLuuHuong { get; set; } // Thời gian lưu hương tính bằng phút
        public string MoTa { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }

        [ForeignKey("ID_ThuongHieu")]
        public Guid ID_ThuongHieu { get; set; }
        public ThuongHieu ThuongHieu { get; set; }

        [ForeignKey("ID_GioiTinh")]
        public Guid ID_GioiTinh { get; set; }
        public GioiTinh GioiTinh { get; set; }
        [ForeignKey("ID_QuocGia")]
        public Guid ID_QuocGia { get; set; }
        public QuocGia QuocGia { get; set; }
        public ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
