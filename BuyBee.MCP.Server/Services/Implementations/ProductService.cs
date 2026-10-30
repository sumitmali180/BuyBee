using EMart.MCP.Server.Models;
using EMart.MCP.Server.Repositories.Interfaces;
using EMart.MCP.Server.Services.Interfaces;

namespace EMart.MCP.Server.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<List<Product>> SearchProductsAsync(
            string searchText = "",
            int? categoryId = null,
            int? subCategoryId = null,
            int? brandId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            bool? isActive = null,
            bool? inStock = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            pageNumber = NormalizePageNumber(pageNumber);
            pageSize = NormalizePageSize(pageSize);

            searchText = searchText?.Trim() ?? string.Empty;

            if (minPrice.HasValue && minPrice < 0)
                throw new ArgumentException(
                    "Minimum price cannot be negative.");

            if (maxPrice.HasValue && maxPrice < 0)
                throw new ArgumentException(
                    "Maximum price cannot be negative.");

            if (minPrice.HasValue &&
                maxPrice.HasValue &&
                minPrice > maxPrice)
            {
                throw new ArgumentException(
                    "Minimum price cannot be greater than maximum price.");
            }

            if (categoryId.HasValue && categoryId <= 0)
                throw new ArgumentException(
                    "Invalid category ID.");

            if (subCategoryId.HasValue && subCategoryId <= 0)
                throw new ArgumentException(
                    "Invalid subcategory ID.");

            if (brandId.HasValue && brandId <= 0)
                throw new ArgumentException(
                    "Invalid brand ID.");

            return await _productRepository.SearchProductsAsync(
                searchText,
                categoryId,
                subCategoryId,
                brandId,
                minPrice,
                maxPrice,
                isActive,
                inStock,
                pageNumber,
                pageSize);
        }


        public async Task<Product?> GetProductAsync(
            int? productId = null,
            string? sku = null)
        {
            if (!productId.HasValue &&
                string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException(
                    "ProductId or SKU is required.");
            }

            if (productId.HasValue && productId <= 0)
            {
                throw new ArgumentException(
                    "Invalid product ID.");
            }

            sku = sku?.Trim();

            return await _productRepository.GetProductAsync(
                productId,
                sku);
        }


        public async Task<List<Product>> GetProductsByCategoryAsync(
            int categoryId,
            int? subCategoryId = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (categoryId <= 0)
                throw new ArgumentException(
                    "Invalid category ID.");

            if (subCategoryId.HasValue &&
                subCategoryId <= 0)
            {
                throw new ArgumentException(
                    "Invalid subcategory ID.");
            }

            pageNumber = NormalizePageNumber(pageNumber);
            pageSize = NormalizePageSize(pageSize);

            return await _productRepository
                .GetProductsByCategoryAsync(
                    categoryId,
                    subCategoryId,
                    isActive,
                    pageNumber,
                    pageSize);
        }


        public async Task<List<Product>> GetProductsByBrandAsync(
            int brandId,
            string? searchText = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (brandId <= 0)
                throw new ArgumentException(
                    "Invalid brand ID.");

            pageNumber = NormalizePageNumber(pageNumber);
            pageSize = NormalizePageSize(pageSize);

            searchText = searchText?.Trim();

            return await _productRepository
                .GetProductsByBrandAsync(
                    brandId,
                    searchText,
                    isActive,
                    pageNumber,
                    pageSize);
        }


        public async Task<List<Product>> GetProductsByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice,
            int? categoryId = null,
            int? brandId = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            if (minPrice < 0)
                throw new ArgumentException(
                    "Minimum price cannot be negative.");

            if (maxPrice < 0)
                throw new ArgumentException(
                    "Maximum price cannot be negative.");

            if (minPrice > maxPrice)
                throw new ArgumentException(
                    "Minimum price cannot be greater than maximum price.");

            if (categoryId.HasValue && categoryId <= 0)
                throw new ArgumentException(
                    "Invalid category ID.");

            if (brandId.HasValue && brandId <= 0)
                throw new ArgumentException(
                    "Invalid brand ID.");

            pageNumber = NormalizePageNumber(pageNumber);
            pageSize = NormalizePageSize(pageSize);

            return await _productRepository
                .GetProductsByPriceRangeAsync(
                    minPrice,
                    maxPrice,
                    categoryId,
                    brandId,
                    isActive,
                    pageNumber,
                    pageSize);
        }


        public async Task<ProductStock?> CheckProductStockAsync(
            int? productId = null,
            string? sku = null)
        {
            if (!productId.HasValue &&
                string.IsNullOrWhiteSpace(sku))
            {
                throw new ArgumentException(
                    "ProductId or SKU is required.");
            }

            if (productId.HasValue && productId <= 0)
            {
                throw new ArgumentException(
                    "Invalid product ID.");
            }

            sku = sku?.Trim();

            return await _productRepository
                .CheckProductStockAsync(
                    productId,
                    sku);
        }


        public async Task<ProductDetails?> GetProductDetailsAsync(
            int productId)
        {
            if (productId <= 0)
                throw new ArgumentException(
                    "Invalid product ID.");

            return await _productRepository
                .GetProductDetailsAsync(productId);
        }


        private static int NormalizePageNumber(
            int pageNumber)
        {
            return pageNumber < 1
                ? 1
                : pageNumber;
        }


        private static int NormalizePageSize(
            int pageSize)
        {
            if (pageSize < 1)
                return 10;

            return Math.Min(pageSize, 100);
        }
    }
}