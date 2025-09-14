using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class NhanVien
    {
        [Key]
        public Guid ID_NhanVien { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [StringLength(20, ErrorMessage = "Mã nhân viên không được vượt quá 20 ký tự")]
        public string Ma_NhanVien { get; set; }

        [Required(ErrorMessage = "Tên nhân viên không được để trống")]
        [StringLength(100, ErrorMessage = "Tên nhân viên không được vượt quá 100 ký tự")]
        public string Ten_NhanVien { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        [MinimumAge(18, ErrorMessage = "Nhân viên phải đủ 18 tuổi trở lên")]
        public DateTime NgaySinh { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Địa chỉ liên hệ không được để trống")]
        [StringLength(200, ErrorMessage = "Địa chỉ không được vượt quá 200 ký tự")]
        public string DiaChiLienHe { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        [RegularExpression("Nam|Nữ|Khác", ErrorMessage = "Giới tính phải là Nam, Nữ hoặc Khác")]
        public string GioiTinh { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(03|09)\d{8}$", ErrorMessage = "Số điện thoại phải có 10 số và bắt đầu bằng 03 hoặc 09")]
        public string SoDienThoai { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "Ngày tham gia")]
        public DateTime? NgayThamGia { get; set; } = DateTime.UtcNow;

        [Required(ErrorMessage = "Trạng thái là bắt buộc")]
        [Range(0, 1, ErrorMessage = "Trạng thái chỉ có thể là 0 (ẩn) hoặc 1 (hiển thị)")]
        public int TrangThai { get; set; }

        [Required]
        public Guid ID_TaiKhoan { get; set; }

        public TaiKhoan? TaiKhoan { get; set; }

        public ICollection<HoaDon> HoaDons { get; set; }
    }

    // 🔥 Custom attribute kiểm tra tuổi
    public class MinimumAgeAttribute : ValidationAttribute
    {
        private readonly int _minAge;
        public MinimumAgeAttribute(int minAge)
        {
            _minAge = minAge;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime date)
            {
                var today = DateTime.Today;
                var age = today.Year - date.Year;
                if (date > today.AddYears(-age)) age--; // chưa tới sinh nhật thì trừ 1

                if (age < _minAge)
                {
                    return new ValidationResult(ErrorMessage ?? $"Tuổi phải từ {_minAge} trở lên");
                }
            }
            return ValidationResult.Success!;
        }
    }
}
