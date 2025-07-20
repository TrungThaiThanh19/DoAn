using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class ThuongHieu
    {
        [Key]
        public Guid ID_ThuongHieu { get; set; }
        public string TenThuongHieu { get; set; }
        public ICollection<SanPham> SanPhams { get; set; }
    }
}
