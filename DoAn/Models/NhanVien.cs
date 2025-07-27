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
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
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
}