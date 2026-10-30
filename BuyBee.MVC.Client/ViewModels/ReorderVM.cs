namespace EMart.ViewModels
{
    public class ReorderItemVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ThumbnailImage { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public int TimesOrdered { get; set; }
        public DateTime LastOrderedDate { get; set; }
        public int DaysSinceLastOrder { get; set; }
        public int CadenceDays { get; set; }
        public bool IsDueForReorder { get; set; }
        public string UrgencyLevel { get; set; } = "Regular"; // 'Urgent', 'DueSoon', 'Regular'
        public string AiReason { get; set; } = string.Empty;
        public int StockQuantity { get; set; }
        public bool IsInStock => StockQuantity > 0;
        public int DefaultReorderQuantity { get; set; } = 1;
    }

    public class ReorderHubVM
    {
        public List<ReorderItemVM> DueItems { get; set; } = new();
        public List<ReorderItemVM> FrequentItems { get; set; } = new();
        public List<ReorderItemVM> AllPastItems { get; set; } = new();
        public int TotalItemsCount => AllPastItems.Count;
        public int TotalDueCount => DueItems.Count;
        public int PotentialBeeCoinsEarned => (int)(DueItems.Sum(x => x.Price) / 100) * 5;
    }
}
