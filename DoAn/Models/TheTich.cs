using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class TheTich
    {
        [Key]
        public Guid ID_TheTich { get; set; }
        public string TenTheTich { get; set; }
        public int TrangThai { get; set; }
        public ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
