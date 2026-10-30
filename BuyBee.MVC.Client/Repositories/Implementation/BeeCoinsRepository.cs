using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class BeeCoinsRepository : IBeeCoinsRepository
    {
        private readonly IConfiguration _configuration;

        public BeeCoinsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            string? connectionString = _configuration.GetConnectionString("DBCS");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DBCS' was not found.");
            }
            return new SqlConnection(connectionString);
        }

        public async Task<CustomerWallet> GetWallet(int customerId)
        {
            using var con = GetConnection();
            await con.OpenAsync();

            const string selectSql = @"
                SELECT WalletId, CustomerId, Balance, TotalEarned, TotalRedeemed, CreatedAt, UpdatedAt
                FROM CustomerWallets
                WHERE CustomerId = @CustomerId;";

            using (var cmd = new SqlCommand(selectSql, con))
            {
                cmd.Parameters.AddWithValue("@CustomerId", customerId);
                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return new CustomerWallet
                    {
                        WalletId = Convert.ToInt32(reader["WalletId"]),
                        CustomerId = Convert.ToInt32(reader["CustomerId"]),
                        Balance = Convert.ToInt32(reader["Balance"]),
                        TotalEarned = Convert.ToInt32(reader["TotalEarned"]),
                        TotalRedeemed = Convert.ToInt32(reader["TotalRedeemed"]),
                        CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                        UpdatedAt = Convert.ToDateTime(reader["UpdatedAt"])
                    };
                }
            }

            // If wallet doesn't exist, create it with 150 Welcome Bonus coins
            const string insertSql = @"
                INSERT INTO CustomerWallets (CustomerId, Balance, TotalEarned, TotalRedeemed, CreatedAt, UpdatedAt)
                VALUES (@CustomerId, 150, 150, 0, GETDATE(), GETDATE());
                SELECT SCOPE_IDENTITY();";

            int newWalletId = 0;
            using (var insertCmd = new SqlCommand(insertSql, con))
            {
                insertCmd.Parameters.AddWithValue("@CustomerId", customerId);
                var res = await insertCmd.ExecuteScalarAsync();
                newWalletId = Convert.ToInt32(res);
            }

            // Insert welcome bonus transaction
            const string txSql = @"
                INSERT INTO BeeCoinTransactions (CustomerId, OrderId, Coins, TransactionType, Description, BalanceAfter, CreatedOn)
                VALUES (@CustomerId, NULL, 150, 'WelcomeBonus', 'Welcome to BeeCoins! 150 bonus coins credited.', 150, GETDATE());";

            using (var txCmd = new SqlCommand(txSql, con))
            {
                txCmd.Parameters.AddWithValue("@CustomerId", customerId);
                await txCmd.ExecuteNonQueryAsync();
            }

            return new CustomerWallet
            {
                WalletId = newWalletId,
                CustomerId = customerId,
                Balance = 150,
                TotalEarned = 150,
                TotalRedeemed = 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
        }

        public async Task<List<BeeCoinTransaction>> GetTransactions(int customerId)
        {
            var list = new List<BeeCoinTransaction>();
            using var con = GetConnection();
            await con.OpenAsync();

            const string sql = @"
                SELECT T.TransactionId, T.CustomerId, T.OrderId, O.OrderNumber, T.Coins, T.TransactionType, T.Description, T.BalanceAfter, T.CreatedOn
                FROM BeeCoinTransactions T
                LEFT JOIN Orders O ON O.OrderId = T.OrderId
                WHERE T.CustomerId = @CustomerId
                ORDER BY T.CreatedOn DESC;";

            using var cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CustomerId", customerId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new BeeCoinTransaction
                {
                    TransactionId = Convert.ToInt32(reader["TransactionId"]),
                    CustomerId = Convert.ToInt32(reader["CustomerId"]),
                    OrderId = reader["OrderId"] == DBNull.Value ? null : Convert.ToInt32(reader["OrderId"]),
                    OrderNumber = reader["OrderNumber"] == DBNull.Value ? null : reader["OrderNumber"].ToString(),
                    Coins = Convert.ToInt32(reader["Coins"]),
                    TransactionType = reader["TransactionType"].ToString() ?? "",
                    Description = reader["Description"].ToString() ?? "",
                    BalanceAfter = Convert.ToInt32(reader["BalanceAfter"]),
                    CreatedOn = Convert.ToDateTime(reader["CreatedOn"])
                });
            }

            return list;
        }

        public async Task<bool> AwardCoins(int customerId, int orderId, decimal orderAmount, string? description = null)
        {
            int coinsToAward = (int)(orderAmount / 100) * 5;
            if (coinsToAward <= 0) return true;

            using var con = GetConnection();
            await con.OpenAsync();

            using var tx = con.BeginTransaction();
            try
            {
                // Ensure wallet exists
                var wallet = await GetWallet(customerId);

                int newBalance = wallet.Balance + coinsToAward;
                int newTotalEarned = wallet.TotalEarned + coinsToAward;

                const string updateSql = @"
                    UPDATE CustomerWallets
                    SET Balance = @Balance, TotalEarned = @TotalEarned, UpdatedAt = GETDATE()
                    WHERE CustomerId = @CustomerId;";

                using (var cmd = new SqlCommand(updateSql, con, tx))
                {
                    cmd.Parameters.AddWithValue("@Balance", newBalance);
                    cmd.Parameters.AddWithValue("@TotalEarned", newTotalEarned);
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    await cmd.ExecuteNonQueryAsync();
                }

                string desc = description ?? $"Reward for Order #{orderId} (Earned 5 coins per ₹100 spent)";
                const string insertTxSql = @"
                    INSERT INTO BeeCoinTransactions (CustomerId, OrderId, Coins, TransactionType, Description, BalanceAfter, CreatedOn)
                    VALUES (@CustomerId, @OrderId, @Coins, 'Earned', @Description, @BalanceAfter, GETDATE());";

                using (var txCmd = new SqlCommand(insertTxSql, con, tx))
                {
                    txCmd.Parameters.AddWithValue("@CustomerId", customerId);
                    txCmd.Parameters.AddWithValue("@OrderId", orderId);
                    txCmd.Parameters.AddWithValue("@Coins", coinsToAward);
                    txCmd.Parameters.AddWithValue("@Description", desc);
                    txCmd.Parameters.AddWithValue("@BalanceAfter", newBalance);
                    await txCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> RedeemCoins(int customerId, int orderId, int coins)
        {
            if (coins <= 0) return true;

            using var con = GetConnection();
            await con.OpenAsync();

            using var tx = con.BeginTransaction();
            try
            {
                var wallet = await GetWallet(customerId);
                if (wallet.Balance < coins)
                {
                    return false;
                }

                int newBalance = wallet.Balance - coins;
                int newTotalRedeemed = wallet.TotalRedeemed + coins;

                const string updateSql = @"
                    UPDATE CustomerWallets
                    SET Balance = @Balance, TotalRedeemed = @TotalRedeemed, UpdatedAt = GETDATE()
                    WHERE CustomerId = @CustomerId;";

                using (var cmd = new SqlCommand(updateSql, con, tx))
                {
                    cmd.Parameters.AddWithValue("@Balance", newBalance);
                    cmd.Parameters.AddWithValue("@TotalRedeemed", newTotalRedeemed);
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    await cmd.ExecuteNonQueryAsync();
                }

                string desc = $"Redeemed at Checkout for Order #{orderId} (₹{coins} instant discount)";
                const string insertTxSql = @"
                    INSERT INTO BeeCoinTransactions (CustomerId, OrderId, Coins, TransactionType, Description, BalanceAfter, CreatedOn)
                    VALUES (@CustomerId, @OrderId, @Coins, 'Redeemed', @Description, @BalanceAfter, GETDATE());";

                using (var txCmd = new SqlCommand(insertTxSql, con, tx))
                {
                    txCmd.Parameters.AddWithValue("@CustomerId", customerId);
                    txCmd.Parameters.AddWithValue("@OrderId", orderId);
                    txCmd.Parameters.AddWithValue("@Coins", -coins);
                    txCmd.Parameters.AddWithValue("@Description", desc);
                    txCmd.Parameters.AddWithValue("@BalanceAfter", newBalance);
                    await txCmd.ExecuteNonQueryAsync();
                }

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> CanRedeem(int customerId, int coins)
        {
            if (coins <= 0) return false;
            var wallet = await GetWallet(customerId);
            return wallet.Balance >= coins;
        }
    }
}
