using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoAn.Models
{
    public class HoaDon
    {
        [Key]
        public Guid ID_HoaDon { get; set; }

        [Required]
        public string Ma_HoaDon { get; set; }

        public string? HoTen { get; set; }
        public string? Email { get; set; }

        public string? Sdt_NguoiNhan { get; set; }

        public string? DiaChi { get; set; }

        [Required]
        public string HinhThucThanhToan { get; set; }

        [Required]
        public string PhuongThucNhanHang { get; set; }

        [Required]
        public decimal TongTienTruocGiam { get; set; }

        [Required]
        public decimal TongTienSauGiam { get; set; }

        public decimal? PhuThu { get; set; }

        [Required]
        public string LoaiHoaDon { get; set; }

        public string? GhiChu { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        public DateTime? NgayCapNhat { get; set; }

        public int TrangThai { get; set; }

        // Foreign key - Voucher
        [ForeignKey("Voucher")]
        public Guid? ID_Voucher { get; set; }
        public Voucher? Voucher { get; set; }

        // Foreign key - NhanVien
        [ForeignKey("NhanVien")]
        public Guid? ID_NhanVien { get; set; }
        public NhanVien? NhanVien { get; set; }

        // Foreign key - KhachHang
        [ForeignKey("KhachHang")]
        public Guid? ID_KhachHang { get; set; }
        public KhachHang? KhachHang { get; set; }

        // Navigation
        public ICollection<QuanLyTraHang> TraHangs { get; set; } = new List<QuanLyTraHang>();
        public ICollection<HoaDonChiTiet> HoaDonChiTiets { get; set; } = new List<HoaDonChiTiet>();
        public ICollection<TrangThaiDonHang> TrangThaiDonHangs { get; set; } = new List<TrangThaiDonHang>();
    }
}
