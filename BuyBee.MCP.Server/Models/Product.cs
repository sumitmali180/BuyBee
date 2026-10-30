namespace EMart.MCP.Server.Models
{
    public class Product
    {
        public int ProductId { get; set; }

        public string? SKU { get; set; }

        public string? ProductName { get; set; }

        public int? UnitId { get; set; }
        public string? UnitName { get; set; }

        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }

        public int? SubCategoryId { get; set; }
        public string? SubCategoryName { get; set; }

        public int? BrandId { get; set; }
        public string? BrandName { get; set; }

        public decimal? Price { get; set; }

        public int? Quantity { get; set; }

        public string? Description { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        public string? ThumbnailImage { get; set; }
    }
}
