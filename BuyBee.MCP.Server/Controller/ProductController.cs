using EMart.MCP.Server.Models;
using EMart.MCP.Server.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMart.MCP.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        // ============================================================
        // SEARCH PRODUCTS
        // ============================================================

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts(
            [FromQuery] string searchText = "",
            [FromQuery] int? categoryId = null,
            [FromQuery] int? subCategoryId = null,
            [FromQuery] int? brandId = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] bool? inStock = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result = await _productService.SearchProductsAsync(
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

            return Ok(result);
        }


        // ============================================================
        // GET PRODUCT
        // ============================================================

        [HttpGet("{productId:int}")]
        public async Task<IActionResult> GetProduct(
            int productId)
        {
            var result =
                await _productService.GetProductAsync(
                    productId,
                    null);

            if (result == null)
                return NotFound(new
                {
                    success = false,
                    message = "Product not found."
                });

            return Ok(result);
        }


        // ============================================================
        // GET PRODUCT BY SKU
        // ============================================================

        [HttpGet("sku/{sku}")]
        public async Task<IActionResult> GetProductBySku(
            string sku)
        {
            var result =
                await _productService.GetProductAsync(
                    null,
                    sku);

            if (result == null)
                return NotFound(new
                {
                    success = false,
                    message = "Product not found."
                });

            return Ok(result);
        }


        // ============================================================
        // PRODUCTS BY CATEGORY
        // ============================================================

        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> GetProductsByCategory(
            int categoryId,
            [FromQuery] int? subCategoryId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result =
                await _productService
                    .GetProductsByCategoryAsync(
                        categoryId,
                        subCategoryId,
                        isActive,
                        pageNumber,
                        pageSize);

            return Ok(result);
        }


        // ============================================================
        // PRODUCTS BY BRAND
        // ============================================================

        [HttpGet("brand/{brandId:int}")]
        public async Task<IActionResult> GetProductsByBrand(
            int brandId,
            [FromQuery] string? searchText = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result =
                await _productService
                    .GetProductsByBrandAsync(
                        brandId,
                        searchText,
                        isActive,
                        pageNumber,
                        pageSize);

            return Ok(result);
        }


        // ============================================================
        // PRODUCTS BY PRICE RANGE
        // ============================================================

        [HttpGet("price-range")]
        public async Task<IActionResult> GetProductsByPriceRange(
            [FromQuery] decimal minPrice,
            [FromQuery] decimal maxPrice,
            [FromQuery] int? categoryId = null,
            [FromQuery] int? brandId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var result =
                await _productService
                    .GetProductsByPriceRangeAsync(
                        minPrice,
                        maxPrice,
                        categoryId,
                        brandId,
                        isActive,
                        pageNumber,
                        pageSize);

            return Ok(result);
        }


        // ============================================================
        // CHECK STOCK
        // ============================================================

        [HttpGet("stock")]
        public async Task<IActionResult> CheckProductStock(
            [FromQuery] int? productId = null,
            [FromQuery] string? sku = null)
        {
            var result =
                await _productService
                    .CheckProductStockAsync(
                        productId,
                        sku);

            if (result == null)
                return NotFound(new
                {
                    success = false,
                    message = "Product not found."
                });

            return Ok(result);
        }


        // ============================================================
        // PRODUCT DETAILS
        // ============================================================

        [HttpGet("details/{productId:int}")]
        public async Task<IActionResult> GetProductDetails(
            int productId)
        {
            var result =
                await _productService
                    .GetProductDetailsAsync(productId);

            if (result == null)
                return NotFound(new
                {
                    success = false,
                    message = "Product not found."
                });

            return Ok(result);
        }
    }
}