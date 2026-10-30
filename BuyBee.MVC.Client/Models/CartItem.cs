
namespace EMart.Models
{
    public class CartItem
    {
        public int CartId { get; set; }

        public int CustomerId { get; set; }

        public int ProductId { get; set; }

        public int Quantity { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        // Display Properties

        public string? ProductName { get; set; }

        public decimal Price { get; set; }

        public string? ThumbnailImage { get; set; }

        public decimal Total => Price * Quantity;
    }
}