namespace EMart.Models
{
    public class ProductReview
    {
        public int ReviewId { get; set; }

        public int ProductId { get; set; }

        public int CustomerId { get; set; }

        public string CustomerName { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public string? ImagesJson { get; set; }

        public List<string> ImageList { get; set; } = new();

        public bool IsVerifiedPurchase { get; set; }

        public bool IsApproved { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
