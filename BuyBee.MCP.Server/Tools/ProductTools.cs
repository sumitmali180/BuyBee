using EMart.MCP.Server.Models;
using EMart.MCP.Server.Services.Interfaces;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace EMart.MCP.Server.Tools
{
    [McpServerToolType]
    public class ProductTools
    {
        private readonly IProductService _productService;

        public ProductTools(IProductService productService)
        {
            _productService = productService;
        }


        // ============================================================
        // SEARCH PRODUCTS
        // ============================================================

        [McpServerTool]
        [Description("""
            Search EMart products using natural search criteria and
            optional filters.

            Use this tool when the user wants to find products by:
            - product name
            - SKU
            - category
            - subcategory
            - brand
            - minimum price
            - maximum price
            - active status
            - stock availability

            Supports pagination and combined filters.
            """)]
        public async Task<List<Product>> SearchProducts(
            [Description("""
                Product name, SKU, category, subcategory, brand,
                or other product-related search text.
                Optional.
                """)]
            string searchText = "",

            [Description("Category ID. Optional.")]
            int? categoryId = null,

            [Description("SubCategory ID. Optional.")]
            int? subCategoryId = null,

            [Description("Brand ID. Optional.")]
            int? brandId = null,

            [Description("Minimum product price. Optional.")]
            decimal? minPrice = null,

            [Description("Maximum product price. Optional.")]
            decimal? maxPrice = null,

            [Description("Filter by active status. Optional.")]
            bool? isActive = null,

            [Description("""
                If true, return only products that have stock.
                If false, return only products that are out of stock.
                Optional.
                """)]
            bool? inStock = null,

            [Description("Page number. Starts from 1.")]
            int pageNumber = 1,

            [Description("""
                Number of products to return.
                Maximum 100.
                """)]
            int pageSize = 10)
        {
            return await _productService.SearchProductsAsync(
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


        // ============================================================
        // GET PRODUCT
        // ============================================================

        [McpServerTool]
        [Description("""
            Get a specific EMart product using ProductId or SKU.

            Use this tool when the user identifies a particular
            product and wants its basic information.
            """)]
        public async Task<Product?> GetProduct(
            [Description("""
                Product ID.
                Optional if SKU is provided.
                """)]
            int? productId = null,

            [Description("""
                Product SKU.
                Optional if ProductId is provided.
                """)]
            string? sku = null)
        {
            return await _productService.GetProductAsync(
                productId,
                sku);
        }


        // ============================================================
        // PRODUCTS BY CATEGORY
        // ============================================================

        [McpServerTool]
        [Description("""
            Find EMart products belonging to a specific category.

            Use this when the user wants products from a particular
            category. A subcategory and active-status filter can
            optionally be applied.
            """)]
        public async Task<List<Product>> FindProductsByCategory(
            [Description("Category ID.")]
            int categoryId,

            [Description("SubCategory ID. Optional.")]
            int? subCategoryId = null,

            [Description("""
                Filter by active status.
                Optional.
                """)]
            bool? isActive = null,

            [Description("Page number. Starts from 1.")]
            int pageNumber = 1,

            [Description("""
                Number of products to return.
                Maximum 100.
                """)]
            int pageSize = 10)
        {
            return await _productService
                .GetProductsByCategoryAsync(
                    categoryId,
                    subCategoryId,
                    isActive,
                    pageNumber,
                    pageSize);
        }


        // ============================================================
        // PRODUCTS BY BRAND
        // ============================================================

        [McpServerTool]
        [Description("""
            Find EMart products belonging to a specific brand.

            Optionally search within that brand and filter by
            active status.
            """)]
        public async Task<List<Product>> FindProductsByBrand(
            [Description("Brand ID.")]
            int brandId,

            [Description("""
                Product name or SKU to search within the brand.
                Optional.
                """)]
            string? searchText = null,

            [Description("""
                Filter by active status.
                Optional.
                """)]
            bool? isActive = null,

            [Description("Page number. Starts from 1.")]
            int pageNumber = 1,

            [Description("""
                Number of products to return.
                Maximum 100.
                """)]
            int pageSize = 10)
        {
            return await _productService
                .GetProductsByBrandAsync(
                    brandId,
                    searchText,
                    isActive,
                    pageNumber,
                    pageSize);
        }


        // ============================================================
        // PRODUCTS BY PRICE RANGE
        // ============================================================

        [McpServerTool]
        [Description("""
            Find EMart products within a specified price range.

            Category, brand, active-status and pagination filters
            can also be provided.
            """)]
        public async Task<List<Product>> FindProductsByPriceRange(
            [Description("Minimum product price.")]
            decimal minPrice,

            [Description("Maximum product price.")]
            decimal maxPrice,

            [Description("Category ID. Optional.")]
            int? categoryId = null,

            [Description("Brand ID. Optional.")]
            int? brandId = null,

            [Description("""
                Filter by active status.
                Optional.
                """)]
            bool? isActive = null,

            [Description("Page number. Starts from 1.")]
            int pageNumber = 1,

            [Description("""
                Number of products to return.
                Maximum 100.
                """)]
            int pageSize = 10)
        {
            return await _productService
                .GetProductsByPriceRangeAsync(
                    minPrice,
                    maxPrice,
                    categoryId,
                    brandId,
                    isActive,
                    pageNumber,
                    pageSize);
        }


        // ============================================================
        // CHECK PRODUCT STOCK
        // ============================================================

        [McpServerTool]
        [Description("""
            Check the current stock status of an EMart product.

            Identify the product using either ProductId or SKU.

            Returns:
            - product information
            - available quantity
            - whether the product is in stock
            - human-readable stock status
            """)]
        public async Task<ProductStock?> CheckProductStock(
            [Description("""
                Product ID.
                Optional if SKU is provided.
                """)]
            int? productId = null,

            [Description("""
                Product SKU.
                Optional if ProductId is provided.
                """)]
            string? sku = null)
        {
            return await _productService
                .CheckProductStockAsync(
                    productId,
                    sku);
        }


        // ============================================================
        // PRODUCT DETAILS
        // ============================================================

        [McpServerTool]
        [Description("""
            Get complete details for an EMart product.

            Returns product information together with its
            associated product images.

            Use this when detailed product information is required.
            """)]
        public async Task<ProductDetails?> GetProductDetails(
            [Description("Product ID.")]
            int productId)
        {
            return await _productService
                .GetProductDetailsAsync(productId);
        }
    }
}