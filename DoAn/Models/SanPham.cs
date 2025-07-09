using Microsoft.Extensions.Primitives;
using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class SanPham
    {
        [Key]
        public Guid IdSp { get; set; } // Khóa chính
        public string TenSp { get; set; }
        public decimal DonGiaBan { get; set; }
        public decimal DonGiaNhap { get; set; }
        public string HinhAnh { get; set; }
        public DateTime NgayTao { get; set; } = DateTime.Now; // Ngày tạo, mặc định là ngày hiện tại

        public bool TrangThai { get; set; } // Trạng thái sản phẩm (còn bán hay không)
        // Navigation properties
        public virtual ICollection<SanPhamChiTiet> SanPhamChiTiets { get; set; }
        public virtual ICollection<GioHangChiTiet> GioHangChiTiets { get; set; }
    }
}
