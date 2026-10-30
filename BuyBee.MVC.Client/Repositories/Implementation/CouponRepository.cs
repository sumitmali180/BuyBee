using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class CouponRepository : ICouponRepository
    {
        private readonly IConfiguration _configuration;

        public CouponRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DBCS"));
        }

        public async Task<Coupon?> GetByCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            const string sql = @"
                SELECT CouponId, Code, DiscountType, DiscountValue, MinOrderAmount, 
                       MaxDiscount, ExpiryDate, UsageLimit, TimesUsed, IsActive, CreatedAt
                FROM dbo.Coupons
                WHERE UPPER(LTRIM(RTRIM(Code))) = UPPER(LTRIM(RTRIM(@Code)));";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Code", code.Trim());
            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapCoupon(reader);
            }

            return null;
        }

        public async Task<List<Coupon>> GetAll()
        {
            var list = new List<Coupon>();
            const string sql = @"
                SELECT CouponId, Code, DiscountType, DiscountValue, MinOrderAmount, 
                       MaxDiscount, ExpiryDate, UsageLimit, TimesUsed, IsActive, CreatedAt
                FROM dbo.Coupons
                ORDER BY CreatedAt DESC;";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            await con.OpenAsync();

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapCoupon(reader));
            }

            return list;
        }

        public async Task<bool> Create(Coupon coupon)
        {
            const string sql = @"
                INSERT INTO dbo.Coupons 
                (Code, DiscountType, DiscountValue, MinOrderAmount, MaxDiscount, ExpiryDate, UsageLimit, TimesUsed, IsActive, CreatedAt)
                VALUES 
                (@Code, @DiscountType, @DiscountValue, @MinOrderAmount, @MaxDiscount, @ExpiryDate, @UsageLimit, 0, @IsActive, GETDATE());";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Code", coupon.Code.Trim().ToUpper());
            cmd.Parameters.AddWithValue("@DiscountType", coupon.DiscountType ?? "Percentage");
            cmd.Parameters.AddWithValue("@DiscountValue", coupon.DiscountValue);
            cmd.Parameters.AddWithValue("@MinOrderAmount", coupon.MinOrderAmount);
            cmd.Parameters.AddWithValue("@MaxDiscount", (object?)coupon.MaxDiscount ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@ExpiryDate", (object?)coupon.ExpiryDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@UsageLimit", (object?)coupon.UsageLimit ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@IsActive", coupon.IsActive);

            await con.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<bool> ToggleActive(int couponId)
        {
            const string sql = @"
                UPDATE dbo.Coupons
                SET IsActive = CASE WHEN IsActive = 1 THEN 0 ELSE 1 END
                WHERE CouponId = @CouponId;";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CouponId", couponId);
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> Delete(int couponId)
        {
            const string sql = "DELETE FROM dbo.Coupons WHERE CouponId = @CouponId;";
            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CouponId", couponId);
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> IncrementUsage(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return false;

            const string sql = @"
                UPDATE dbo.Coupons
                SET TimesUsed = TimesUsed + 1
                WHERE UPPER(LTRIM(RTRIM(Code))) = UPPER(LTRIM(RTRIM(@Code)));";

            using var con = GetConnection();
            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@Code", code.Trim());
            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public (bool IsValid, decimal DiscountAmount, string Message) ValidateCoupon(Coupon coupon, decimal cartSubtotal)
        {
            if (coupon == null)
            {
                return (false, 0, "Invalid promo code.");
            }

            if (!coupon.IsActive)
            {
                return (false, 0, "This coupon is currently inactive.");
            }

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate.Value < DateTime.Now)
            {
                return (false, 0, $"This coupon expired on {coupon.ExpiryDate.Value:dd MMM yyyy}.");
            }

            if (coupon.UsageLimit.HasValue && coupon.TimesUsed >= coupon.UsageLimit.Value)
            {
                return (false, 0, "This coupon has reached its maximum redemption limit.");
            }

            if (cartSubtotal < coupon.MinOrderAmount)
            {
                return (false, 0, $"Minimum cart subtotal of ₹{coupon.MinOrderAmount:N0} required for this coupon.");
            }

            decimal discount = 0;
            if (coupon.DiscountType.Equals("Percentage", StringComparison.OrdinalIgnoreCase))
            {
                discount = Math.Round(cartSubtotal * (coupon.DiscountValue / 100m), 2);
                if (coupon.MaxDiscount.HasValue && coupon.MaxDiscount.Value > 0 && discount > coupon.MaxDiscount.Value)
                {
                    discount = coupon.MaxDiscount.Value;
                }
            }
            else // Fixed
            {
                discount = coupon.DiscountValue;
                if (discount > cartSubtotal)
                {
                    discount = cartSubtotal; // Cannot exceed cart value
                }
            }

            return (true, discount, $"Coupon '{coupon.Code}' applied successfully! You saved ₹{discount:N2}.");
        }

        private static Coupon MapCoupon(SqlDataReader reader)
        {
            return new Coupon
            {
                CouponId = reader.GetInt32(reader.GetOrdinal("CouponId")),
                Code = reader.GetString(reader.GetOrdinal("Code")),
                DiscountType = reader.GetString(reader.GetOrdinal("DiscountType")),
                DiscountValue = reader.GetDecimal(reader.GetOrdinal("DiscountValue")),
                MinOrderAmount = reader.GetDecimal(reader.GetOrdinal("MinOrderAmount")),
                MaxDiscount = reader.IsDBNull(reader.GetOrdinal("MaxDiscount")) ? null : reader.GetDecimal(reader.GetOrdinal("MaxDiscount")),
                ExpiryDate = reader.IsDBNull(reader.GetOrdinal("ExpiryDate")) ? null : reader.GetDateTime(reader.GetOrdinal("ExpiryDate")),
                UsageLimit = reader.IsDBNull(reader.GetOrdinal("UsageLimit")) ? null : reader.GetInt32(reader.GetOrdinal("UsageLimit")),
                TimesUsed = reader.GetInt32(reader.GetOrdinal("TimesUsed")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt"))
            };
        }
    }
}
