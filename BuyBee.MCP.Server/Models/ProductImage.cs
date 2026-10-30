namespace EMart.MCP.Server.Models
{
    public class ProductImage
    {
        public int ImageId { get; set; }

        public int ProductId { get; set; }

        public string? ImagePath { get; set; }
    }
}
