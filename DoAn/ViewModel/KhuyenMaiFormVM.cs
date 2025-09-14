// ViewModels/KhuyenMai/KhuyenMaiFormVM.cs
using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModels.KhuyenMaiVM
{
    public class KhuyenMaiFormVM
    {
        public Guid? ID_KhuyenMai { get; set; }

        [Required, StringLength(50)]
        public string Ma_KhuyenMai { get; set; }

        [Required, StringLength(200)]
        public string Ten_KhuyenMai { get; set; }

        [Required] // "percent" | "fixed"
        public string KieuGiamGia { get; set; } = "percent";

        [Range(0.01, double.MaxValue)]
        public decimal GiaTriGiam { get; set; }

        // Áp dụng như "cap" khi giảm % (0 = không cap)
        [Range(0, double.MaxValue)]
        public decimal GiaTriToiDa { get; set; } = 0;

        public string? MoTa { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime NgayBatDau { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime NgayHetHan { get; set; }

        public int TrangThai { get; set; } = 1;

        // Gắn SPCT
        public List<Guid> SanPhamChiTietIds { get; set; } = new();
    }
}