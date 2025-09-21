// ViewModel/CheckoutReviewVM.cs
using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModel
{
    public class CheckoutReviewVM
    {
        public Guid AddressId { get; set; }
        public string FullAddress { get; set; } = "";

        public string ReceiverName { get; set; } = "";
        public string Phone { get; set; } = "";

        public List<GioHangItemVMD> Items { get; set; } = new();
        public decimal ShippingFee { get; set; }
        public string PaymentMethod { get; set; } = "COD";
    }

    public class PlaceOrderPost
    {
        public Guid AddressId { get; set; }
        public string ReceiverName { get; set; } = "";
        public string Phone { get; set; } = "";
        public string PaymentMethod { get; set; } = "COD";
        public decimal ShippingFee { get; set; }
        public string? Note { get; set; }
    }
}