using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class KhuyenMai
    {
        [Key]
        public Guid ID_KhuyenMai { get; set; }

        [Required(ErrorMessage = "⚠️ Mã khuyến mãi không được để trống.")]
        [StringLength(100, ErrorMessage = "⚠️ Mã khuyến mãi không được vượt quá 100 ký tự.")]
        public string Ma_KhuyenMai { get; set; }

        [Required(ErrorMessage = "⚠️ Tên khuyến mãi không được để trống.")]
        [StringLength(100, ErrorMessage = "⚠️ Tên khuyến mãi không được vượt quá 100 ký tự.")]
        public string Ten_KhuyenMai { get; set; }

        [Required(ErrorMessage = "⚠️ Ngày bắt đầu không được để trống.")]
        [DataType(DataType.Date)]
        public DateTime NgayBatDau { get; set; }

        [Required(ErrorMessage = "⚠️ Ngày hết hạn không được để trống.")]
        [DataType(DataType.Date)]
        public DateTime NgayHetHan { get; set; }

        [Required(ErrorMessage = "⚠️ Kiểu giảm giá không được để trống.")]
        public string KieuGiamGia { get; set; } // "Giảm theo %" hoặc "Giảm theo số tiền"

        [Required(ErrorMessage = "⚠️ Giá trị giảm không được để trống.")]
        [Range(1, 1000000, ErrorMessage = "⚠️ Giá trị giảm phải lớn hơn 0.")]
        public decimal GiaTriGiam { get; set; }

        [Range(0, 1000000, ErrorMessage = "⚠️ Giá trị tối đa không hợp lệ.")]
        public decimal GiaTriToiDa { get; set; } // Optional (chỉ áp dụng nếu giảm %)

       
        public string MoTa { get; set; }

        
        public int TrangThai { get; set; } // 0: Ngưng, 1: Hoạt động

        // Navigation Property
        public ICollection<ChiTietKhuyenMai> ChiTietKhuyenMais { get; set; }
    }
}
