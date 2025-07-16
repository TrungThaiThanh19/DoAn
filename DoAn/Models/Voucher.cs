using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DoAn.Models
{
    public class Voucher
    {
        [Key]
        public Guid ID_Voucher { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng nhập mã voucher.")]
        [StringLength(20, ErrorMessage = "⚠️ Mã voucher không được vượt quá 20 ký tự.")]
        public string Ma_Voucher { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng nhập tên voucher.")]
        [StringLength(100, ErrorMessage = "⚠️ Tên voucher không được vượt quá 100 ký tự.")]
        public string Ten_Voucher { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng nhập ngày tạo.")]
        [DataType(DataType.Date)]
        public DateTime NgayTao { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng nhập ngày hết hạn.")]
        [DataType(DataType.Date)]
        public DateTime NgayHetHan { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng chọn kiểu giảm giá.")]
        public string KieuGiamGia { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng nhập giá trị giảm.")]
        [Range(0, double.MaxValue, ErrorMessage = "⚠️ Giá trị giảm phải lớn hơn hoặc bằng 0.")]
        public decimal GiaTriGiam { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng nhập giá trị tối thiểu.")]
        [Range(0, double.MaxValue, ErrorMessage = "⚠️ Giá trị tối thiểu phải lớn hơn hoặc bằng 0.")]
        public decimal GiaTriToiThieu { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng nhập giá trị tối đa.")]
        [Range(0, double.MaxValue, ErrorMessage = "⚠️ Giá trị tối đa phải lớn hơn hoặc bằng 0.")]
        public decimal GiaTriToiDa { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng nhập số lượng.")]
        [Range(0, int.MaxValue, ErrorMessage = "⚠️ Số lượng phải là số không âm.")]
        public int SoLuong { get; set; }

        [Required(ErrorMessage = "⚠️ Vui lòng chọn trạng thái.")]
        [EnumDataType(typeof(TrangThaiVoucher), ErrorMessage = "⚠️ Trạng thái không hợp lệ.")]
        public TrangThaiVoucher TrangThai { get; set; }

        
        public string MoTa { get; set; }

        
        public Guid ID_TaiKhoan { get; set; }

        public TaiKhoan? TaiKhoan { get; set; }

        public ICollection<HoaDon>? HoaDons { get; set; }

        public enum TrangThaiVoucher
        {
            ChuaSuDung = 0,
            DaSuDung = 1,
            HetHan = 2
        }
    }
}
