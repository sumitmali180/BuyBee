namespace EMart.MCP.Server.Models
{
    public class ProductStock
    {
        public int ProductId { get; set; }

        public string? SKU { get; set; }

        public string? ProductName { get; set; }

        public int Quantity { get; set; }

        public bool IsActive { get; set; }

        public bool InStock { get; set; }

        public string? StockStatus { get; set; }
    }
}
