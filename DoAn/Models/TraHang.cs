using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class TraHang
    {
        [Key]
        public Guid ID_TraHang { get; set; }
        public string LyDo { get; set; }
        public string GhiChu { get; set; }
        public string NhanVienXuLy { get; set; }
        public DateTime NgayTao { get; set; }
        public string TrangThai { get; set; }
        public decimal TongTienHoan { get; set; }

        public Guid ID_HoaDon { get; set; }
        public HoaDon HoaDon { get; set; }

        public ICollection<ChiTietTraHang> ChiTietTraHangs { get; set; }
    }
}
