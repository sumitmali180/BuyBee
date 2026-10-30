using EMart.Models;

namespace EMart.ViewModels
{
    public class ProductCatalogVM
    {
        public PagedResult<Products> Products { get; set; } = new();

        public List<Categories> Categories { get; set; } = new();

        public List<Brands> Brands { get; set; } = new();

        // Filters
        public string? SearchText { get; set; }

        public int? SelectedCategoryId { get; set; }

        public string? SelectedCategoryName { get; set; }

        public int? SelectedSubCategoryId { get; set; }

        public int? SelectedBrandId { get; set; }

        public string? SelectedBrandName { get; set; }

        public decimal? MinPrice { get; set; }

        public decimal? MaxPrice { get; set; }

        public string? SortBy { get; set; } = "newest";
    }
}
