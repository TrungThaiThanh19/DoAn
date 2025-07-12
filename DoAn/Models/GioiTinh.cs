using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class GioiTinh
    {
        [Key]
        public Guid ID_GioiTinh { get; set; }
        public string Ten_GioiTinh { get; set; }

        public ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
    }
}
