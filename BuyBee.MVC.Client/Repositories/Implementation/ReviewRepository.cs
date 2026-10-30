using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Text.Json;

namespace EMart.Repositories.Implementation
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly IConfiguration _configuration;

        public ReviewRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DBCS"));
        }

        public async Task<List<ProductReviewItemVM>> GetReviewsByProductId(int productId)
        {
            var list = new List<ProductReviewItemVM>();
            if (productId <= 0) return list;

            const string sql = @"
                SELECT pr.ReviewId, pr.ProductId, pr.CustomerId, 
                       ISNULL(u.FullName, 'Verified Customer') AS CustomerName,
                       pr.Rating, pr.Title, pr.Comment, pr.ImagesJson, 
                       pr.IsVerifiedPurchase, pr.CreatedAt
                FROM dbo.ProductReviews pr
                LEFT JOIN dbo.Customers c ON c.CustomerId = pr.CustomerId
                LEFT JOIN dbo.Users u ON u.UserId = c.UserId
                WHERE pr.ProductId = @ProductId AND pr.IsApproved = 1
                ORDER BY pr.CreatedAt DESC;";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var item = new ProductReviewItemVM
                {
                    ReviewId = reader.GetInt32(reader.GetOrdinal("ReviewId")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                    Rating = reader.GetInt32(reader.GetOrdinal("Rating")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Comment = reader.GetString(reader.GetOrdinal("Comment")),
                    IsVerifiedPurchase = reader.GetBoolean(reader.GetOrdinal("IsVerifiedPurchase")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                };

                string? imgJson = reader.IsDBNull(reader.GetOrdinal("ImagesJson")) ? null : reader.GetString(reader.GetOrdinal("ImagesJson"));
                if (!string.IsNullOrWhiteSpace(imgJson))
                {
                    try
                    {
                        item.Photos = JsonSerializer.Deserialize<List<string>>(imgJson) ?? new List<string>();
                    }
                    catch
                    {
                        item.Photos = new List<string>();
                    }
                }

                list.Add(item);
            }

            return list;
        }

        public async Task<ProductRatingSummaryVM> GetRatingSummary(int productId)
        {
            var summary = new ProductRatingSummaryVM();
            if (productId <= 0) return summary;

            const string sql = @"
                SELECT 
                    COUNT(*) AS TotalCount,
                    ISNULL(AVG(CAST(Rating AS FLOAT)), 0.0) AS AvgRating,
                    SUM(CASE WHEN Rating = 5 THEN 1 ELSE 0 END) AS FiveCount,
                    SUM(CASE WHEN Rating = 4 THEN 1 ELSE 0 END) AS FourCount,
                    SUM(CASE WHEN Rating = 3 THEN 1 ELSE 0 END) AS ThreeCount,
                    SUM(CASE WHEN Rating = 2 THEN 1 ELSE 0 END) AS TwoCount,
                    SUM(CASE WHEN Rating = 1 THEN 1 ELSE 0 END) AS OneCount
                FROM dbo.ProductReviews
                WHERE ProductId = @ProductId AND IsApproved = 1;";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                summary.TotalReviews = reader.GetInt32(reader.GetOrdinal("TotalCount"));
                summary.AverageRating = Math.Round(reader.GetDouble(reader.GetOrdinal("AvgRating")), 1);
                summary.FiveStarCount = reader.IsDBNull(reader.GetOrdinal("FiveCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("FiveCount"));
                summary.FourStarCount = reader.IsDBNull(reader.GetOrdinal("FourCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("FourCount"));
                summary.ThreeStarCount = reader.IsDBNull(reader.GetOrdinal("ThreeCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("ThreeCount"));
                summary.TwoStarCount = reader.IsDBNull(reader.GetOrdinal("TwoCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("TwoCount"));
                summary.OneStarCount = reader.IsDBNull(reader.GetOrdinal("OneCount")) ? 0 : reader.GetInt32(reader.GetOrdinal("OneCount"));
            }

            return summary;
        }

        public async Task<bool> AddReview(ProductReview review)
        {
            if (review == null || review.ProductId <= 0 || review.CustomerId <= 0) return false;

            const string sql = @"
                INSERT INTO dbo.ProductReviews 
                (ProductId, CustomerId, Rating, Title, Comment, ImagesJson, IsVerifiedPurchase, IsApproved, CreatedAt)
                VALUES 
                (@ProductId, @CustomerId, @Rating, @Title, @Comment, @ImagesJson, @IsVerifiedPurchase, @IsApproved, GETDATE());";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ProductId", review.ProductId);
            cmd.Parameters.AddWithValue("@CustomerId", review.CustomerId);
            cmd.Parameters.AddWithValue("@Rating", review.Rating);
            cmd.Parameters.AddWithValue("@Title", review.Title.Trim());
            cmd.Parameters.AddWithValue("@Comment", review.Comment.Trim());
            cmd.Parameters.AddWithValue("@ImagesJson", (object?)review.ImagesJson ?? "[]");
            cmd.Parameters.AddWithValue("@IsVerifiedPurchase", review.IsVerifiedPurchase);
            cmd.Parameters.AddWithValue("@IsApproved", review.IsApproved);

            await con.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> HasCustomerPurchased(int customerId, int productId)
        {
            if (customerId <= 0 || productId <= 0) return false;

            const string sql = @"
                SELECT CASE WHEN EXISTS (
                    SELECT 1 
                    FROM dbo.Orders o 
                    INNER JOIN dbo.OrderDetails od ON od.OrderId = o.OrderId
                    WHERE o.CustomerId = @CustomerId AND od.ProductId = @ProductId
                ) THEN 1 ELSE 0 END;";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CustomerId", customerId);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            await con.OpenAsync();

            object? res = await cmd.ExecuteScalarAsync();
            return res != null && Convert.ToInt32(res) == 1;
        }

        public async Task<List<ProductReview>> GetAllReviewsForAdmin()
        {
            var list = new List<ProductReview>();
            const string sql = @"
                SELECT pr.ReviewId, pr.ProductId, pr.CustomerId, 
                       ISNULL(u.FullName, 'Customer') AS CustomerName,
                       pr.Rating, pr.Title, pr.Comment, pr.ImagesJson, 
                       pr.IsVerifiedPurchase, pr.IsApproved, pr.CreatedAt
                FROM dbo.ProductReviews pr
                LEFT JOIN dbo.Customers c ON c.CustomerId = pr.CustomerId
                LEFT JOIN dbo.Users u ON u.UserId = c.UserId
                ORDER BY pr.CreatedAt DESC;";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var item = new ProductReview
                {
                    ReviewId = reader.GetInt32(reader.GetOrdinal("ReviewId")),
                    ProductId = reader.GetInt32(reader.GetOrdinal("ProductId")),
                    CustomerId = reader.GetInt32(reader.GetOrdinal("CustomerId")),
                    CustomerName = reader.GetString(reader.GetOrdinal("CustomerName")),
                    Rating = reader.GetInt32(reader.GetOrdinal("Rating")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Comment = reader.GetString(reader.GetOrdinal("Comment")),
                    ImagesJson = reader.IsDBNull(reader.GetOrdinal("ImagesJson")) ? "[]" : reader.GetString(reader.GetOrdinal("ImagesJson")),
                    IsVerifiedPurchase = reader.GetBoolean(reader.GetOrdinal("IsVerifiedPurchase")),
                    IsApproved = reader.GetBoolean(reader.GetOrdinal("IsApproved")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
                };
                list.Add(item);
            }

            return list;
        }

        public async Task<bool> ToggleApproval(int reviewId)
        {
            const string sql = @"
                UPDATE dbo.ProductReviews
                SET IsApproved = CASE WHEN IsApproved = 1 THEN 0 ELSE 1 END
                WHERE ReviewId = @ReviewId;";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ReviewId", reviewId);
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteReview(int reviewId)
        {
            const string sql = "DELETE FROM dbo.ProductReviews WHERE ReviewId = @ReviewId;";
            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@ReviewId", reviewId);
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
