using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models
{
    public class KhachHang
    {
        [Key]
        public Guid ID_KhachHang { get; set; }

        [Required]
        public string Ma_KhachHang { get; set; }

        [Required]
        public string Ten_KhachHang { get; set; }

        [Required]
        public string GioiTinh { get; set; } // Gợi ý sau này dùng enum

        [Required]
        [Phone]
        public string SoDienThoai { get; set; }

        public DateTime? NgaySinh { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public int TrangThai { get; set; }

        // FK đến tài khoản
        [ForeignKey("TaiKhoan")]
        public Guid ID_TaiKhoan { get; set; }

        public TaiKhoan TaiKhoan { get; set; }

        // Navigation: 1 KH - N Hóa đơn
        public ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();
    }
}
