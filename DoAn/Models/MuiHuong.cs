using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class MuiHuong
    {
        [Key]
        public Guid IdMuiHuong { get; set; } // Khóa chính
        public string TenMH { get; set; }
        public string MoTa { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now; // Ngày tạo, mặc định là ngày hiện tại
        public DateTime? NgayCapNhat { get; set; }
        public bool TrangThai { get; set; }

        // Navigation property
        public virtual ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
