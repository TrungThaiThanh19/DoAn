using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class KhachHang
    {
        [Key]
        public Guid ID_KhachHang { get; set; }
        public string Ma_KhachHang { get; set; }
        public string Ten_KhachHang { get; set; }
        public string GioiTinh { get; set; }
        public string SoDienThoai { get; set; }
        public DateTime NgaySinh { get; set; }
        public string Email { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public int TrangThai { get; set; }

        public Guid ID_TaiKhoan { get; set; }
        public TaiKhoan TaiKhoan { get; set; }




		public ICollection<HoaDon> HoaDons { get; set; }
	}
}
