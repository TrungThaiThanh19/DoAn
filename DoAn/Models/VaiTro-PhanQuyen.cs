using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class VaiTro_PhanQuyen
    {
        [Key]
        public Guid IdVtPq { get; set; }

        public Guid? IdVaiTro { get; set; }
        public VaiTro? VaiTro { get; set; }

        public Guid? IdQuyen { get; set; }
        public PhanQuyen? PhanQuyen { get; set; }

        public bool TrangThai { get; set; }

        public DateTime NgayTao { get; set; }

        public DateTime? NgayCapNhat { get; set; }
    }
}
