using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class Voucher
    {
        [Key]
        public Guid IdVch { get; set; } // PK theo ERD
        public string TenVoucher { get; set; }
        public decimal PhanTramGiam { get; set; } // Giá trị giảm (0-100 cho phần trăm)
        public int SoLuong { get; set; } // Số lượng voucher còn lại hoặc tổng số
        public DateTime NgayBatDau { get; set; }
        public DateTime NgayKetThuc { get; set; }
        public DateTime NgayTao { get; set; } // Thêm trường Ngày Tạo
        public bool TrangThai { get; set; } // Thêm trường Trạng Thái

        public virtual ICollection<DonDatHang> DonDatHangs { get; set; } // Voucher có thể được áp dụng cho nhiều đơn hàng
        public virtual ICollection<HoaDon> HoaDons { get; set; } // Voucher có thể được áp dụng cho nhiều giỏ hàng
    }

}

