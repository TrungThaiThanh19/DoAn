using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class SanPhamChiTiet
    {
        [Key]
        public Guid IdCtsp { get; set; } // Khóa chính
        public Guid? IdSp { get; set; } // Khóa ngoại tới SanPham
        public Guid? IdThuongHieu { get; set; } // Khóa ngoại tới ThuongHieu
        public Guid? IdMuiHuong { get; set; } // Khóa ngoại tới MuiHuong
        public Guid? IdTheTich { get; set; } // Khóa ngoại tới TheTich
        public Guid? IdGioTinh { get; set; } // FK tới GioTinh
        public int SoLuongTon { get; set; }
        public bool TrangThai { get; set; } // Trạng thái sản phẩm chi tiết (còn bán hay không)
        public DateTime NgayTao { get; set; } = DateTime.Now; // Ngày tạo, mặc định là ngày hiện tại

        // Navigation properties
        public virtual SanPham? SanPham { get; set; }
        public virtual ThuongHieu? ThuongHieu { get; set; }
        public virtual MuiHuong? MuiHuong { get; set; }
        public virtual TheTich? TheTich { get; set; }
        public virtual GioiTinh? GioTinh { get; set; }

        public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; }
        public virtual ICollection<ChiTietDonDatHang> ChiTietDonDatHangs { get; set; } // SPCT có trong ChiTietDonDatHang online
        public virtual ICollection<GioHangChiTiet> GioHangChiTiets { get; set; }
    }
}
