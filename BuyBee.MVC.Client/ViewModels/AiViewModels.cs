namespace EMart.ViewModels
{
    public class AiChatMessage
    {
        public string Role { get; set; } = "user";
        public string Content { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    public class AiChatRequest
    {
        public List<AiChatMessage> Messages { get; set; } = new();
        public int? ContextProductId { get; set; }
        public string? ContextUrl { get; set; }
    }

    public class AiProductCard
    {
        public int ProductId { get; set; }
        public string? Sku { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public string? CategoryName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; } = true;
        public bool InStock => Quantity > 0;
        public string? ThumbnailImage { get; set; }
        public string? Description { get; set; }
    }

    public class AiChatResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = string.Empty;
        public List<AiProductCard> Products { get; set; } = new();
        public List<string> ToolsUsed { get; set; } = new();
        public string? Error { get; set; }
    }

    public class AiStatusResponse
    {
        public bool GroqReady { get; set; }
        public string GroqModel { get; set; } = string.Empty;
        public bool McpServerReady { get; set; }
        public string McpEndpoint { get; set; } = string.Empty;
        public int ToolCount { get; set; }
        public List<string> AvailableTools { get; set; } = new();
    }
}
