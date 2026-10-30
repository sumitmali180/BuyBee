using EMart.MCP.Server.Models;
using EMart.MCP.Server.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.MCP.Server.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly string _connectionString;

        public ProductRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DBCS")
                ?? throw new InvalidOperationException(
                    "DefaultConnection connection string not found.");
        }


        // ============================================================
        // MCP SEARCH
        // ============================================================

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
            var products = new List<Product>();

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            using var connection =
                new SqlConnection(_connectionString);

            using var command =
                new SqlCommand("SP_Products", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            AddParameter(command, "@Flag", "MCP_SEARCH");

            AddParameter(
                command,
                "@SearchText",
                searchText ?? string.Empty);

            AddParameter(
                command,
                "@CategoryId",
                categoryId);

            AddParameter(
                command,
                "@SubCategoryId",
                subCategoryId);

            AddParameter(
                command,
                "@BrandId",
                brandId);

            AddParameter(
                command,
                "@MinPrice",
                minPrice);

            AddParameter(
                command,
                "@MaxPrice",
                maxPrice);

            AddParameter(
                command,
                "@IsActive",
                isActive);

            AddParameter(
                command,
                "@InStock",
                inStock);

            AddParameter(
                command,
                "@PageNumber",
                pageNumber);

            AddParameter(
                command,
                "@PageSize",
                pageSize);

            await connection.OpenAsync();

            using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }


        // ============================================================
        // GET PRODUCT
        // ============================================================

        public async Task<Product?> GetProductAsync(
            int? productId = null,
            string? sku = null)
        {
            if (productId == null &&
                string.IsNullOrWhiteSpace(sku))
            {
                return null;
            }

            using var connection =
                new SqlConnection(_connectionString);

            using var command =
                new SqlCommand("SP_Products", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            AddParameter(
                command,
                "@Flag",
                "SELECTBYID");

            AddParameter(
                command,
                "@ProductId",
                productId);

            AddParameter(
                command,
                "@SKU",
                sku);

            await connection.OpenAsync();

            using var reader =
                await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return MapProduct(reader);
            }

            return null;
        }


        // ============================================================
        // PRODUCTS BY CATEGORY
        // ============================================================

        public async Task<List<Product>> GetProductsByCategoryAsync(
            int categoryId,
            int? subCategoryId = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var products = new List<Product>();

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            using var connection =
                new SqlConnection(_connectionString);

            using var command =
                new SqlCommand("SP_Products", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            AddParameter(
                command,
                "@Flag",
                "MCP_BY_CATEGORY");

            AddParameter(
                command,
                "@CategoryId",
                categoryId);

            AddParameter(
                command,
                "@SubCategoryId",
                subCategoryId);

            AddParameter(
                command,
                "@IsActive",
                isActive);

            AddParameter(
                command,
                "@PageNumber",
                pageNumber);

            AddParameter(
                command,
                "@PageSize",
                pageSize);

            await connection.OpenAsync();

            using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }


        // ============================================================
        // PRODUCTS BY BRAND
        // ============================================================

        public async Task<List<Product>> GetProductsByBrandAsync(
            int brandId,
            string? searchText = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var products = new List<Product>();

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            using var connection =
                new SqlConnection(_connectionString);

            using var command =
                new SqlCommand("SP_Products", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            AddParameter(
                command,
                "@Flag",
                "MCP_BY_BRAND");

            AddParameter(
                command,
                "@BrandId",
                brandId);

            AddParameter(
                command,
                "@SearchText",
                searchText ?? string.Empty);

            AddParameter(
                command,
                "@IsActive",
                isActive);

            AddParameter(
                command,
                "@PageNumber",
                pageNumber);

            AddParameter(
                command,
                "@PageSize",
                pageSize);

            await connection.OpenAsync();

            using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }


        // ============================================================
        // PRODUCTS BY PRICE RANGE
        // ============================================================

        public async Task<List<Product>> GetProductsByPriceRangeAsync(
            decimal minPrice,
            decimal maxPrice,
            int? categoryId = null,
            int? brandId = null,
            bool? isActive = null,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var products = new List<Product>();

            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : Math.Min(pageSize, 100);

            using var connection =
                new SqlConnection(_connectionString);

            using var command =
                new SqlCommand("SP_Products", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            AddParameter(
                command,
                "@Flag",
                "MCP_BY_PRICE");

            AddParameter(
                command,
                "@MinPrice",
                minPrice);

            AddParameter(
                command,
                "@MaxPrice",
                maxPrice);

            AddParameter(
                command,
                "@CategoryId",
                categoryId);

            AddParameter(
                command,
                "@BrandId",
                brandId);

            AddParameter(
                command,
                "@IsActive",
                isActive);

            AddParameter(
                command,
                "@PageNumber",
                pageNumber);

            AddParameter(
                command,
                "@PageSize",
                pageSize);

            await connection.OpenAsync();

            using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                products.Add(MapProduct(reader));
            }

            return products;
        }


        // ============================================================
        // CHECK PRODUCT STOCK
        // ============================================================

        public async Task<ProductStock?> CheckProductStockAsync(
            int? productId = null,
            string? sku = null)
        {
            if (productId == null &&
                string.IsNullOrWhiteSpace(sku))
            {
                return null;
            }

            using var connection =
                new SqlConnection(_connectionString);

            using var command =
                new SqlCommand("SP_Products", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            AddParameter(
                command,
                "@Flag",
                "MCP_STOCK");

            AddParameter(
                command,
                "@ProductId",
                productId);

            AddParameter(
                command,
                "@SKU",
                sku);

            await connection.OpenAsync();

            using var reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            return new ProductStock
            {
                ProductId =
                    Convert.ToInt32(
                        reader["ProductId"]),

                SKU =
                    reader["SKU"]?.ToString(),

                ProductName =
                    reader["ProductName"]?.ToString(),

                Quantity =
                    reader["Quantity"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(
                            reader["Quantity"]),

                IsActive =
                    reader["IsActive"] != DBNull.Value &&
                    Convert.ToBoolean(
                        reader["IsActive"]),

                InStock =
                    reader["InStock"] != DBNull.Value &&
                    Convert.ToBoolean(
                        reader["InStock"]),

                StockStatus =
                    reader["StockStatus"]?.ToString()
            };
        }


        // ============================================================
        // PRODUCT DETAILS
        // ============================================================

        public async Task<ProductDetails?> GetProductDetailsAsync(
            int productId)
        {
            using var connection =
                new SqlConnection(_connectionString);

            using var command =
                new SqlCommand("SP_Products", connection);

            command.CommandType =
                CommandType.StoredProcedure;

            AddParameter(
                command,
                "@Flag",
                "MCP_DETAILS");

            AddParameter(
                command,
                "@ProductId",
                productId);

            await connection.OpenAsync();

            using var reader =
                await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var details = new ProductDetails
            {
                Product = MapProduct(reader)
            };

            // Move to second result set
            if (await reader.NextResultAsync())
            {
                while (await reader.ReadAsync())
                {
                    details.Images.Add(
                        new ProductImage
                        {
                            ImageId =
                                Convert.ToInt32(
                                    reader["ImageId"]),

                            ProductId =
                                Convert.ToInt32(
                                    reader["ProductId"]),

                            ImagePath =
                                reader["ImagePath"]?.ToString()
                        });
                }
            }

            return details;
        }


        // ============================================================
        // PRODUCT MAPPER
        // ============================================================

        private static Product MapProduct(
            SqlDataReader reader)
        {
            return new Product
            {
                ProductId =
                    GetInt(reader, "ProductId"),

                SKU =
                    GetString(reader, "SKU"),

                ProductName =
                    GetString(reader, "ProductName"),

                UnitId =
                    GetNullableInt(reader, "UnitId"),

                UnitName =
                    GetString(reader, "UnitName"),

                CategoryId =
                    GetNullableInt(reader, "CategoryId"),

                CategoryName =
                    GetString(reader, "CategoryName"),

                SubCategoryId =
                    GetNullableInt(reader, "SubCategoryId"),

                SubCategoryName =
                    GetString(reader, "SubCategoryName"),

                BrandId =
                    GetNullableInt(reader, "BrandId"),

                BrandName =
                    GetString(reader, "BrandName"),

                Price =
                    GetNullableDecimal(reader, "Price"),

                Quantity =
                    GetNullableInt(reader, "Quantity"),

                Description =
                    GetString(reader, "Description"),

                IsActive =
                    GetNullableBool(reader, "IsActive"),

                CreatedDate =
                    GetNullableDateTime(
                        reader,
                        "CreatedDate"),

                CreatedOn =
                    GetNullableDateTime(
                        reader,
                        "CreatedOn"),

                UpdatedOn =
                    GetNullableDateTime(
                        reader,
                        "UpdatedOn"),

                CreatedBy =
                    GetNullableInt(
                        reader,
                        "CreatedBy"),

                UpdatedBy =
                    GetNullableInt(
                        reader,
                        "UpdatedBy"),

                ThumbnailImage =
                    GetString(
                        reader,
                        "ThumbnailImage")
            };
        }


        // ============================================================
        // PARAMETER HELPER
        // ============================================================

        private static void AddParameter(
            SqlCommand command,
            string name,
            object? value)
        {
            command.Parameters.AddWithValue(
                name,
                value ?? DBNull.Value);
        }


        // ============================================================
        // SAFE READER HELPERS
        // ============================================================

        private static bool HasColumn(
            SqlDataReader reader,
            string columnName)
        {
            for (int i = 0;
                 i < reader.FieldCount;
                 i++)
            {
                if (reader.GetName(i)
                    .Equals(
                        columnName,
                        StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }


        private static string? GetString(
            SqlDataReader reader,
            string columnName)
        {
            if (!HasColumn(reader, columnName))
                return null;

            return reader[columnName] == DBNull.Value
                ? null
                : reader[columnName]?.ToString();
        }


        private static int GetInt(
            SqlDataReader reader,
            string columnName)
        {
            if (!HasColumn(reader, columnName) ||
                reader[columnName] == DBNull.Value)
            {
                return 0;
            }

            return Convert.ToInt32(
                reader[columnName]);
        }


        private static int? GetNullableInt(
            SqlDataReader reader,
            string columnName)
        {
            if (!HasColumn(reader, columnName) ||
                reader[columnName] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToInt32(
                reader[columnName]);
        }


        private static decimal? GetNullableDecimal(
            SqlDataReader reader,
            string columnName)
        {
            if (!HasColumn(reader, columnName) ||
                reader[columnName] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDecimal(
                reader[columnName]);
        }


        private static bool? GetNullableBool(
            SqlDataReader reader,
            string columnName)
        {
            if (!HasColumn(reader, columnName) ||
                reader[columnName] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToBoolean(
                reader[columnName]);
        }


        private static DateTime? GetNullableDateTime(
            SqlDataReader reader,
            string columnName)
        {
            if (!HasColumn(reader, columnName) ||
                reader[columnName] == DBNull.Value)
            {
                return null;
            }

            return Convert.ToDateTime(
                reader[columnName]);
        }
    }
}