namespace EMart.MCP.Server.Models
{
    public class Category
    {
        public int CategoryId { get; set; }

        public string? CategoryName { get; set; }

        public bool? IsActive { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }
    }
}
