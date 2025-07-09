using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models
{
    public class TaiKhoan
    {
        [Key]
        public Guid ID_TK { get; set; }

        [Required]
        [MaxLength(255)]
        public string MatKhau { get; set; }

        [Required]
        public bool TrangThai { get; set; } // Ví dụ: true là hoạt động, false là bị khóa

        [Required]

        public DateTime NgayTao { get; set; } = DateTime.Now; // Ngày tạo, mặc định là ngày hiện tại
        public DateTime? NgayCapNhat { get; set; } // Nullable

   
        public Guid? IdVaiTro { get; set; }
        public VaiTro VaiTro { get; set; } // Navigation property

        // Navigation properties ngược lại (nếu cần)
        public NhanVien? NhanVien { get; set; } // Mối quan hệ 1-1 với NhanVien
        public KhachHang? KhachHang { get; set; } // Mối quan hệ 1-1 với KhachHang
    }
}
