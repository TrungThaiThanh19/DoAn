using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class PhanQuyen
    {
        [Key]
        public Guid IdPhanQuyen { get; set; }

        public string TenQuyen { get; set; }

        public string MoTa { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }

        public ICollection<VaiTro_PhanQuyen> VaiTroPhanQuyens { get; set; }
    }
}
