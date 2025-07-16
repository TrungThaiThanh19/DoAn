using System.ComponentModel.DataAnnotations;
using System.Data;

namespace DoAn.Models
{
    public class TaiKhoan
    {
        [Key]
        public Guid ID_TaiKhoan { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public Guid ID_Roles { get; set; }
        public Roles Roles { get; set; }

        public ICollection<NhanVien> NhanViens { get; set; }
        public ICollection<KhachHang> KhachHangs { get; set; }
        public ICollection<Voucher> Vouchers { get; set; }
    }
}
