using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class KhuyenMai
    {
        [Key]
        public Guid ID_KhuyenMai { get; set; }
        public string Ma_KhuyenMai { get; set; }
        public string Ten_KhuyenMai { get; set; }
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayHetHan { get; set; }
        public string KieuGiamGia { get; set; }
        public decimal GiaTriGiam { get; set; }
        public decimal GiaTriToiDa { get; set; }
        public string MoTa { get; set; }
        public int TrangThai { get; set; }

        public ICollection<ChiTietKhuyenMai> ChiTietKhuyenMais { get; set; }
    }
}
