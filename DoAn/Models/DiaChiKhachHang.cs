using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class DiaChiKhachHang
    {
        [Key]
        public Guid ID_DiaChiKhachHang { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số nhà/đường")]
        public string SoNha { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Xã/Phường")]
        public string Xa_Phuong { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện")]
        public string Quan_Huyen { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố")]
        public string Tinh_ThanhPho { get; set; }

        public bool DiaChiMacDinh { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [MaxLength(100)]
        public string HoTen { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [MaxLength(20)]
        public string SoDienThoai { get; set; }

        public Guid ID_KhachHang { get; set; }
        public KhachHang KhachHang { get; set; }
    }
}