using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class VaiTro
    {
        [Key]
        public Guid IdVaiTro { get; set; }

        public string TenVaiTro { get; set; }

        public string MoTa { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        // Navigation property
        public ICollection<TaiKhoan> TaiKhoans { get; set; }
        public ICollection<VaiTro_PhanQuyen> VaiTroPhanQuyens { get; set; }
    }
}
