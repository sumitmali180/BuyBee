using EMart.Models;
using System.ComponentModel.DataAnnotations;

namespace EMart.ViewModels
{
    public class CheckoutVM
    {
        public List<CartItem> CartItems { get; set; } = new();

        public List<Addresses> Addresses { get; set; } = new();

        [Required(ErrorMessage = "Please select a delivery address.")]
        public int? SelectedAddressId { get; set; }

        [Required(ErrorMessage = "Please select a payment method.")]
        public string PaymentMethod { get; set; } = "COD";

        public decimal Subtotal { get; set; }

        public decimal Shipping { get; set; }

        public decimal Discount { get; set; }

        public string? CouponCode { get; set; }

        public int AvailableBeeCoins { get; set; }

        public int CoinsToRedeem { get; set; }

        public decimal BeeCoinsDiscount { get; set; }

        public int EstimatedCoinsToEarn { get; set; }

        public decimal GrandTotal { get; set; }
    }
}