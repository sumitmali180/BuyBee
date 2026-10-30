namespace EMart.Models
{
    public class OrderDetails
    {
        public int OrderDetailId { get; set; }

        public int OrderId { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; } = string.Empty;

        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; }

        public decimal Total { get; set; }
        
        public int? VendorId { get; set; }

        public string ItemStatus { get; set; } = "Pending";

        public string? ImagePath { get; set; }

        public string? VendorStoreName { get; set; }
    }
}