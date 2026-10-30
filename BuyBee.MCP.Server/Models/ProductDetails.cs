namespace EMart.MCP.Server.Models
{
    public class ProductDetails
    {
        public Product? Product { get; set; }

        public List<ProductImage> Images { get; set; } = new();
    }
}
