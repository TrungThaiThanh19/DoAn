using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class DonDatHang
    {
        [Key]
        public Guid ID_DDH { get; set; } // Khóa chính
        public Guid? ID_KH { get; set; } // Khóa ngoại tới KhachHang
        public Guid? ID_PTTT { get; set; } // Khóa ngoại tới Phương thức thanh toán
        public Guid? ID_TTDH { get; set; } // Khóa ngoại tới Trạng thái đơn hàng
        public Guid? ID_VCH { get; set; } // Khóa ngoại tới Voucher (nếu có)
        public decimal TongTien { get; set; }
        public string DiaChiGiaoHang { get; set; }

        public virtual KhachHang? KhachHang { get; set; }
        public virtual ICollection<ChiTietDonDatHang> ChiTietDonDatHangs { get; set; }
        public virtual TrangThaiDonHang? TrangThaiDonHang { get; set; } 
        public virtual PhuongThucThanhToan? PhuongThucThanhToan { get; set; }
        public virtual Voucher? Voucher { get; set; } // Voucher có thể áp dụng cho đơn hàng này
    }
}
