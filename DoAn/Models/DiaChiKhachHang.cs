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

        public Guid ID_KhachHang { get; set; }
        public KhachHang KhachHang { get; set; }
    }
}
