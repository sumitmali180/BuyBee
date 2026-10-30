using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class AccountRepository : IAccountRepository
    {
        private readonly IConfiguration _configuration;

        public AccountRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("DBCS"));
        }

        #region Login

        public async Task<CustomerAccountVM?> Login(string email)
        {
            CustomerAccountVM? model = null;

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(
                    "sp_CustomerAccount", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Flag", "LOGIN");

                    cmd.Parameters.AddWithValue(
                        "@Email", email);

                    await con.OpenAsync();

                    using (SqlCommand ensureCmd = new SqlCommand(@"
                        IF EXISTS (SELECT 1 FROM Users WHERE LOWER(Email) = LOWER(@Email))
                        BEGIN
                            DECLARE @Uid INT;
                            SELECT @Uid = UserId FROM Users WHERE LOWER(Email) = LOWER(@Email);
                            IF NOT EXISTS (SELECT 1 FROM Customers WHERE UserId = @Uid)
                                INSERT INTO Customers (UserId) VALUES (@Uid);
                        END", con))
                    {
                        ensureCmd.Parameters.AddWithValue("@Email", email);
                        await ensureCmd.ExecuteNonQueryAsync();
                    }

                    using (SqlDataReader reader =
                           await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            model = new CustomerAccountVM
                            {
                                UserId = Convert.ToInt32(
                                    reader["UserId"]),

                                CustomerId = Convert.ToInt32(
                                    reader["CustomerId"]),

                                FullName = reader["FullName"]?.ToString(),

                                Email = reader["Email"]?.ToString(),

                                PasswordHash =
                                    reader["PasswordHash"]?.ToString(),

                                RoleId =
                                    reader["RoleId"] == DBNull.Value
                                    ? null
                                    : Convert.ToInt32(reader["RoleId"]),

                                RoleName =
                                    reader["RoleName"]?.ToString(),

                                IsActive =
                                    reader["IsActive"] != DBNull.Value &&
                                    Convert.ToBoolean(reader["IsActive"])
                            };
                        }
                    }
                }
            }

            return model;
        }

        #endregion

        #region Email Exists

        public async Task<bool> EmailExists(string email)
        {
            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(
                    "sp_CustomerAccount", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Flag", "EMAILEXISTS");

                    cmd.Parameters.AddWithValue(
                        "@Email", email);

                    await con.OpenAsync();

                    object? result =
                        await cmd.ExecuteScalarAsync();

                    return result != null &&
                           Convert.ToInt32(result) > 0;
                }
            }
        }

        #endregion

        #region Register Customer

        public async Task<int> RegisterCustomer(
            RegisterVM model,
            string passwordHash)
        {
            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(
                    "sp_CustomerAccount", con))
                {
                    cmd.CommandType =
                        CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Flag", "REGISTER");

                    cmd.Parameters.AddWithValue(
                        "@FullName",
                        model.FullName ?? "");

                    cmd.Parameters.AddWithValue(
                        "@Email",
                        model.Email ?? "");

                    cmd.Parameters.AddWithValue(
                        "@PasswordHash",
                        passwordHash);

                    await con.OpenAsync();

                    object? result =
                        await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        return Convert.ToInt32(result);
                    }
                }
            }

            return 0;
        }

        #endregion

        #region Get Customer By UserId

        public async Task<CustomerAccountVM?> GetCustomerByUserId(
            int userId)
        {
            CustomerAccountVM? model = null;

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand(
                    "sp_CustomerAccount", con))
                {
                    cmd.CommandType =
                        CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue(
                        "@Flag", "GETBYUSERID");

                    cmd.Parameters.AddWithValue(
                        "@UserId", userId);

                    await con.OpenAsync();

                    using (SqlCommand ensureCmd = new SqlCommand(@"
                        IF EXISTS (SELECT 1 FROM Users WHERE UserId = @UserId)
                        BEGIN
                            IF NOT EXISTS (SELECT 1 FROM Customers WHERE UserId = @UserId)
                                INSERT INTO Customers (UserId) VALUES (@UserId);
                        END", con))
                    {
                        ensureCmd.Parameters.AddWithValue("@UserId", userId);
                        await ensureCmd.ExecuteNonQueryAsync();
                    }

                    using (SqlDataReader reader =
                           await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            model = new CustomerAccountVM
                            {
                                UserId =
                                    Convert.ToInt32(
                                        reader["UserId"]),

                                CustomerId =
                                    Convert.ToInt32(
                                        reader["CustomerId"]),

                                FullName =
                                    reader["FullName"]?.ToString(),

                                Email =
                                    reader["Email"]?.ToString(),

                                RoleId =
                                    reader["RoleId"] == DBNull.Value
                                    ? null
                                    : Convert.ToInt32(
                                        reader["RoleId"]),

                                RoleName =
                                    reader["RoleName"]?.ToString(),

                                IsActive =
                                    reader["IsActive"] != DBNull.Value &&
                                    Convert.ToBoolean(
                                        reader["IsActive"])
                            };
                        }
                    }
                }
            }

            return model;
        }

        #endregion
    }
}