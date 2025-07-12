using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class MuiHuong
    {
        [Key]
        public Guid ID_MuiHuong { get; set; }
        public string Ma_MuiHuong { get; set; }
        public string Ten_MuiHuong { get; set; }
        public int TrangThai { get; set; }

        public ICollection<SanPham> SanPhams { get; set; }
    }
}
