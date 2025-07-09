using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class TrangThaiDonHang
    {
        [Key]
        public Guid IdTrangThaiHd { get; set; } // PK (Tên vẫn là IdTrangThaiHd nhưng sẽ dùng cho cả HĐ & ĐĐH)
        public string TenTrangThaiDh { get; set; }
        public string MoTa { get; set; }
        public DateTime NgayTao { get; set; }

        public virtual ICollection<DonDatHang> DonDatHangs { get; set; } // Chỉ Đơn Đặt Hàng mới dùng Trạng Thái Đơn Hàng chi tiết
    }
}
