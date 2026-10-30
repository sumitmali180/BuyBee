using EMart.MCP.Server.Models;
using EMart.MCP.Server.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.MCP.Server.Repositories.Implementations
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly IConfiguration _configuration;

        public CategoryRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(
                _configuration.GetConnectionString("DBCS"));
        }

        public async Task<List<Category>> GetAllAsync()
        {
            var categories = new List<Category>();

            using var connection = GetConnection();
            using var command = new SqlCommand(
                "sp_Category",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue("@Flag", "SELECTALL");

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add(new Category
                {
                    CategoryId = Convert.ToInt32(
                        reader["CategoryId"]),

                    CategoryName = reader["CategoryName"]?.ToString(),

                    IsActive = reader["IsActive"] == DBNull.Value
                        ? null
                        : Convert.ToBoolean(reader["IsActive"]),

                    CreatedOn = reader["CreatedOn"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["CreatedOn"]),

                    UpdatedOn = reader["UpdatedOn"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["UpdatedOn"]),

                    CreatedBy = reader["CreatedBy"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["CreatedBy"]),

                    UpdatedBy = reader["UpdatedBy"] == DBNull.Value
                        ? null
                        : Convert.ToInt32(reader["UpdatedBy"])
                });
            }

            return categories;
        }

        public async Task<Category?> GetByIdAsync(int categoryId)
        {
            using var connection = GetConnection();

            using var command = new SqlCommand(
                "sp_Category",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@Flag",
                "SELECTBYID");

            command.Parameters.AddWithValue(
                "@CategoryId",
                categoryId);

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Category
                {
                    CategoryId = Convert.ToInt32(
                        reader["CategoryId"]),

                    CategoryName = reader["CategoryName"]?.ToString(),

                    IsActive = reader["IsActive"] == DBNull.Value
                        ? null
                        : Convert.ToBoolean(reader["IsActive"])
                };
            }

            return null;
        }

        public async Task<List<Category>> SearchAsync(
            string searchText)
        {
            var categories = new List<Category>();

            using var connection = GetConnection();

            using var command = new SqlCommand(
                "sp_Category",
                connection);

            command.CommandType = CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@Flag",
                "SEARCH");

            command.Parameters.AddWithValue(
            "@CategoryName",
            searchText ?? string.Empty);

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                categories.Add(new Category
                {
                    CategoryId = Convert.ToInt32(
                        reader["CategoryId"]),

                    CategoryName = reader["CategoryName"]?.ToString(),

                    IsActive = reader["IsActive"] == DBNull.Value
                        ? null
                        : Convert.ToBoolean(reader["IsActive"])
                });
            }

            return categories;
        }
    }
}