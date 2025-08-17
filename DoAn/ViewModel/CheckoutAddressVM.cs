// ViewModel/CheckoutAddressVM.cs
using DoAn.Models;

namespace DoAn.ViewModel
{
    public class CheckoutAddressVM
    {
        public List<AddressVM> Addresses { get; set; } = new();
        public Guid? SelectedAddressId { get; set; }

        public class AddressVM
        {
            public Guid ID_DiaChiKhachHang { get; set; }
            public string SoNha { get; set; } = "";
            public string Xa_Phuong { get; set; } = "";
            public string Quan_Huyen { get; set; } = "";
            public string Tinh_ThanhPho { get; set; } = "";
            public bool DiaChiMacDinh { get; set; }

            public string? HoTen { get; set; }        // NEW
            public string? SoDienThoai { get; set; }  // đã có
        }
    }
}