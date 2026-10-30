namespace EMart.Models
{
    public class Coupon
    {
        public int CouponId { get; set; }

        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// "Percentage" or "Fixed"
        /// </summary>
        public string DiscountType { get; set; } = "Percentage";

        public decimal DiscountValue { get; set; }

        public decimal MinOrderAmount { get; set; }

        public decimal? MaxDiscount { get; set; }

        public DateTime? ExpiryDate { get; set; }

        public int? UsageLimit { get; set; }

        public int TimesUsed { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
