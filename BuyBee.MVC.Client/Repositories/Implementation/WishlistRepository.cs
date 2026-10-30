using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly IConfiguration _configuration;

        public WishlistRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DBCS"));
        }

        public async Task<bool> AddToWishlist(int customerId, int productId)
        {
            if (customerId <= 0 || productId <= 0) return false;

            const string sql = @"
                IF NOT EXISTS (SELECT 1 FROM Wishlist WHERE CustomerId = @CustomerId AND ProductId = @ProductId)
                BEGIN
                    INSERT INTO Wishlist (CustomerId, ProductId, CreatedOn)
                    VALUES (@CustomerId, @ProductId, GETDATE());
                END";

            using (var con = GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@ProductId", productId);
                await con.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                return true;
            }
        }

        public async Task<bool> RemoveFromWishlist(int customerId, int productId)
        {
            if (customerId <= 0 || productId <= 0) return false;

            const string sql = "DELETE FROM Wishlist WHERE CustomerId = @CustomerId AND ProductId = @ProductId;";

            using (var con = GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@ProductId", productId);
                await con.OpenAsync();
                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
        }

        public async Task<bool> IsInWishlist(int customerId, int productId)
        {
            if (customerId <= 0 || productId <= 0) return false;

            const string sql = "SELECT COUNT(1) FROM Wishlist WHERE CustomerId = @CustomerId AND ProductId = @ProductId;";

            using (var con = GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                cmd.Parameters.AddWithValue("@ProductId", productId);
                await con.OpenAsync();
                var count = (int?)await cmd.ExecuteScalarAsync();
                return count.HasValue && count.Value > 0;
            }
        }

        public async Task<List<Products>> GetWishlist(int customerId)
        {
            var list = new List<Products>();
            if (customerId <= 0) return list;

            const string sql = @"
                SELECT p.ProductId, p.ProductName, p.SKU, p.Price, p.Quantity, p.Description, p.IsActive,
                       c.CategoryId, c.CategoryName,
                       b.BrandId, b.BrandName,
                       (SELECT TOP 1 pi.ImagePath 
                        FROM ProductImages pi 
                        WHERE pi.ProductId = p.ProductId 
                        ORDER BY pi.ImageId ASC) AS ThumbnailImage,
                       w.CreatedOn AS WishlistCreatedOn
                FROM Wishlist w
                INNER JOIN Products p ON w.ProductId = p.ProductId
                LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                LEFT JOIN Brands b ON p.BrandId = b.BrandId
                WHERE w.CustomerId = @CustomerId AND p.IsActive = 1
                ORDER BY w.CreatedOn DESC;";

            using (var con = GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                await con.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Products
                        {
                            ProductId = reader["ProductId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["ProductId"]),
                            ProductName = reader["ProductName"]?.ToString() ?? "",
                            SKU = reader["SKU"]?.ToString() ?? "",
                            Price = reader["Price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Price"]),
                            Quantity = reader["Quantity"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Quantity"]),
                            Description = reader["Description"]?.ToString() ?? "",
                            IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"]),
                            CategoryId = reader["CategoryId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["CategoryId"]),
                            CategoryName = reader["CategoryName"]?.ToString() ?? "",
                            BrandId = reader["BrandId"] == DBNull.Value ? 0 : Convert.ToInt32(reader["BrandId"]),
                            BrandName = reader["BrandName"]?.ToString() ?? "",
                            ThumbnailImage = reader["ThumbnailImage"]?.ToString() ?? "/images/no-image.png",
                            CreatedOn = reader["WishlistCreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(reader["WishlistCreatedOn"])
                        });
                    }
                }
            }

            return list;
        }

        public async Task<int> GetWishlistCount(int customerId)
        {
            if (customerId <= 0) return 0;

            const string sql = "SELECT COUNT(1) FROM Wishlist WHERE CustomerId = @CustomerId;";

            using (var con = GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                await con.OpenAsync();
                var count = (int?)await cmd.ExecuteScalarAsync();
                return count ?? 0;
            }
        }

        public async Task<List<int>> GetWishlistProductIds(int customerId)
        {
            var ids = new List<int>();
            if (customerId <= 0) return ids;

            const string sql = "SELECT ProductId FROM Wishlist WHERE CustomerId = @CustomerId;";

            using (var con = GetConnection())
            using (var cmd = new SqlCommand(sql, con))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                await con.OpenAsync();

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        if (reader["ProductId"] != DBNull.Value)
                        {
                            ids.Add(Convert.ToInt32(reader["ProductId"]));
                        }
                    }
                }
            }

            return ids;
        }
    }
}
