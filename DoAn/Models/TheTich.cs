using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class TheTich
    {
        [Key]
        public Guid IdTheTich { get; set; } // Khóa chính
        public string TheTichs { get; set; } // Thể tích, ví dụ: "50ml", "100ml"
        public DateTime NgayTao { get; set; }
        public DateTime? NgayCapNhat { get; set; }
        public bool TrangThai { get; set; }

        // Navigation property
        public virtual ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}

