using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class Roles
    {
        [Key]
        public Guid ID_Roles { get; set; }

        [Required(ErrorMessage = "Mã vai trò không được để trống")]
        [StringLength(50)]
        public string Ma_Roles { get; set; }

        [Required(ErrorMessage = "Tên vai trò không được để trống")]
        [StringLength(100)]
        public string Ten_Roles { get; set; }

        public ICollection<TaiKhoan> TaiKhoans { get; set; }
    }
}
