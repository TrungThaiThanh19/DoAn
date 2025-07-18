using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class SanPham
    {
        [Key]
        public Guid ID_SanPham { get; set; }
        public string Ma_SanPham { get; set; }
        public string Ten_SanPham { get; set; }
        public string HinhAnh { get; set; }
        public string MoTa { get; set; } // Thời gian lưu hương, hương đầu hương giữa và hương cuối
        public int TrangThai { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; } 

        public Guid ID_ThuongHieu { get; set; }
        public ThuongHieu ThuongHieu { get; set; }

        public Guid ID_MuiHuong { get; set; }
        public MuiHuong MuiHuong { get; set; }

        public Guid ID_QuocGia { get; set; }
        public QuocGia QuocGia { get; set; }
        public ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
