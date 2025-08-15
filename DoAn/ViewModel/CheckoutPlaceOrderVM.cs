using System;
using System.ComponentModel.DataAnnotations;

namespace DoAn.ViewModel
{
    public class CheckoutPlaceOrderVM
    {
        [Required]
        public Guid DiaChiId { get; set; }

        [Required]
        public string PaymentMethod { get; set; } = "COD";
    }
}
