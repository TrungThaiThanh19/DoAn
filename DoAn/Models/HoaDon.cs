using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models
{
    public class HoaDon
    {
        [Key]
        public Guid ID_HoaDon { get; set; }
        public string HoTen { get; set; }
        public string? Email { get; set; }
        public string Sdt_NguoiNhan { get; set; }
        public string DiaChi { get; set; }
        public string PhuongThucThanhToan { get; set; }
        public string PhuongThucNhanHang { get; set; }
        public decimal TongTienTruocGiam { get; set; }
        public decimal TongTienSauGiam { get; set; }
        public decimal PhuThu { get; set; }
        public string LoaiHoaDon { get; set; }
        public string GhiChu { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime? NgayCapNhat { get; set; }
        public string TrangThai { get; set; }
        public Guid? ID_Voucher { get; set; }
		[ForeignKey("ID_Voucher")]
		public Voucher Voucher { get; set; }
        public ICollection<QuanLyTraHang> TraHangs { get; set; }
        public ICollection<HoaDonChiTiet> HoaDonChiTiets { get; set; }
    }
}
