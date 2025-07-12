using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class Roles
    {
        [Key]
        public Guid ID_Roles { get; set; }
        public string Ma_Roles { get; set; }
        public string Ten_Roles { get; set; }

        public ICollection<TaiKhoan> TaiKhoans { get; set; }
    }
}
