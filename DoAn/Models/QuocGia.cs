using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class QuocGia
    {
        [Key]
        public Guid ID_QuocGia { get; set; }
        public string Ten_QuocGia { get; set; }

        public ICollection<SanPham> SanPhams { get; set; }
    }
}
