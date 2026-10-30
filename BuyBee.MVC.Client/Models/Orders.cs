namespace EMart.Models
{
    public class Orders
    {
        public int OrderId { get; set; }

        public string OrderNumber { get; set; } = string.Empty;

        public int CustomerId { get; set; }

        public int AddressId { get; set; }

        public DateTime OrderDate { get; set; }

        public decimal SubTotal { get; set; }

        public decimal ShippingAmount { get; set; }

        public decimal DiscountAmount { get; set; }

        public decimal GrandTotal { get; set; }

        public string OrderStatus { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = string.Empty;

        public string? CouponCode { get; set; }

        public int CoinsRedeemed { get; set; }

        public decimal CoinsDiscount { get; set; }

        public int CoinsEarned { get; set; }

        public string? CarrierName { get; set; }

        public string? TrackingNumber { get; set; }

        public string? DeliveryOtp { get; set; }
        public int? DeliveryPartnerId { get; set; }
        public string? DeliveryPartnerName { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public int DeliveryAttempts { get; set; }
        public string? DeliveryNotes { get; set; }
        public string? FailedReason { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}