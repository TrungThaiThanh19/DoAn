using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModel
{
    public class CheckoutAddressVM
    {
        public List<AddressVM> Addresses { get; set; } = new();
        public Guid? SelectedAddressId { get; set; }

        // Thêm property này để bind dữ liệu từ form AddAddress
        public AddressVM NewAddress { get; set; } = new();

        public class AddressVM
        {
            public Guid ID_DiaChiKhachHang { get; set; }

            [Required(ErrorMessage = "Vui lòng nhập số nhà/đường")]
            public string SoNha { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng chọn Xã/Phường")]
            public string Xa_Phuong { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng chọn Quận/Huyện")]
            public string Quan_Huyen { get; set; } = "";

            [Required(ErrorMessage = "Vui lòng chọn Tỉnh/Thành phố")]
            public string Tinh_ThanhPho { get; set; } = "";

            public bool DiaChiMacDinh { get; set; }

            [Required(ErrorMessage = "Họ tên không được để trống")]
            public string? HoTen { get; set; }

            [Required(ErrorMessage = "Số điện thoại không được để trống")]
            [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
            public string? SoDienThoai { get; set; }
        }
    }
}