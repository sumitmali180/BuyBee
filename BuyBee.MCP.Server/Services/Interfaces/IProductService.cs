using EMart.MCP.Server.Models;

namespace EMart.MCP.Server.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<Product>> SearchProductsAsync(
            string searchText = "",
            int? categoryId = null,
            int? subCategoryId = null,
            int? brandId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isActive = null,
            bool? inStock = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<Product?> GetProductAsync(
            int? productId = null,
            string? sku = null);

        Task<List<Product>> GetProductsByCategoryAsync(
            int categoryId,
            int? subCategoryId = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<List<Product>> GetProductsByBrandAsync(
            int brandId,
            string? searchText = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<List<Product>> GetProductsByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice,
            int? categoryId = null,
            int? brandId = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10);

        Task<ProductStock?> CheckProductStockAsync(
            int? productId = null,
            string? sku = null);

        Task<ProductDetails?> GetProductDetailsAsync(
            int productId);
    }
}