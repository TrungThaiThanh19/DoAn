using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class TheTich
    {
        [Key]
        public Guid ID_TheTich { get; set; }
        public string Ma_TheTich { get; set; }
        public decimal GiaTri { get; set; }
        public string DonVi { get; set; }
        public int TrangThai { get; set; }

        public ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
