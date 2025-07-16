using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModels
{
    public class RegisterViewModel
    {
        // --- Thông tin Tài khoản ---
        [Required(ErrorMessage = "Tên đăng nhập không được để trống.")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Tên đăng nhập phải có ít nhất 3 và tối đa 100 ký tự.")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải có ít nhất 6 và tối đa 100 ký tự.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; }

        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // --- Thông tin Khách hàng ---
        [Required(ErrorMessage = "Tên của bạn không được để trống.")]
        [Display(Name = "Họ và tên")]
        public string TenKhachHang { get; set; }

        [Display(Name = "Giới tính")]
        // Có thể dùng [Required] nếu bạn muốn bắt buộc chọn giới tính
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string SoDienThoai { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống.")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)] // Định dạng hiển thị ngày
        [Display(Name = "Ngày sinh")]
        public DateTime NgaySinh { get; set; }
    }
}
