using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class NhanVien
    {
        [Key]
        public Guid ID_NhanVien { get; set; }
        public string Ma_NhanVien { get; set; }
        public string Ten_NhanVien { get; set; }
        public DateTime NgaySinh { get; set; }
        public string Email { get; set; }
        public string DiaChiLienHe { get; set; }
        public string GioiTinh { get; set; }
        public string SoDienThoai { get; set; }
        public DateTime NgayThamGia { get; set; }
        public int TrangThai { get; set; }

        public Guid ID_TaiKhoan { get; set; }
        public TaiKhoan TaiKhoan { get; set; }
        public ICollection<HoaDon> HoaDons { get; set; }
    }
}
