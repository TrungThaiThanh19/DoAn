using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class GioiTinh
    {
        [Key]
        public Guid ID_GioiTinh { get; set; }
        public string TenGioiTinh { get; set; }
        public ICollection<SanPham> SanPhams { get; set; }
    }
}
