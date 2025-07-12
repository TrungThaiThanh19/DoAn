using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class ThuongHieu
    {
        [Key]
        public Guid ID_ThuongHieu { get; set; }
        public string Ma_ThuongHieu { get; set; }
        public string Ten_ThuongHieu { get; set; }
        public int TrangThai { get; set; }

        public ICollection<SanPham> SanPhams { get; set; }
    }
}
