using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class DiaChiKhachHang
    {
        [Key]
        public Guid ID_DiaChiKhachHang { get; set; }

        public string SoNha { get; set; }
        public string Xa_Phuong { get; set; }
        public string Quan_Huyen { get; set; }
        public string Tinh_ThanhPho { get; set; }
        public bool DiaChiMacDinh { get; set; }

        // NEW: Họ tên người nhận gắn với địa chỉ
        [MaxLength(100)]
        public string? HoTen { get; set; }

        // ĐT kèm địa chỉ (giữ như bạn đã thêm)
        [MaxLength(20)]
        public string? SoDienThoai { get; set; }

        public Guid ID_KhachHang { get; set; }
        public KhachHang KhachHang { get; set; }
    }
}