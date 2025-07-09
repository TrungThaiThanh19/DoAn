using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class GioiTinh
    {
        [Key]
        public Guid IdGioiTinh { get; set; } // PK
        public string TenGioTinh { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now; // Ngày tạo, mặc định là ngày hiện tại
        public DateTime? NgayCapNhat { get; set; }


        public virtual ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
