using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class AddressRepository : IAddressRepository
    {
        private readonly IConfiguration _configuration;

        public AddressRepository(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }


        #region Connection

        private SqlConnection GetConnection()
        {
            string? connectionString =
                _configuration.GetConnectionString("DBCS");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "Connection string 'DBCS' was not found.");
            }

            return new SqlConnection(connectionString);
        }

        #endregion


        #region Get By Customer

        public async Task<List<Addresses>> GetByCustomerId(
            int customerId)
        {
            List<Addresses> addresses = new();

            using SqlConnection con = GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Addresses",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar, 30).Value =
                "SELECTBYCUSTOMER";


            cmd.Parameters.Add(
                "@CustomerId",
                SqlDbType.Int).Value =
                customerId;


            await con.OpenAsync();


            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();


            while (await reader.ReadAsync())
            {
                addresses.Add(new Addresses
                {
                    AddressId =
                        Convert.ToInt32(
                            reader["AddressId"]),

                    CustomerId =
                        Convert.ToInt32(
                            reader["CustomerId"]),

                    AddressLine1 =
                        reader["AddressLine1"] == DBNull.Value
                            ? null
                            : reader["AddressLine1"].ToString(),

                    City =
                        reader["City"] == DBNull.Value
                            ? null
                            : reader["City"].ToString(),

                    State =
                        reader["State"] == DBNull.Value
                            ? null
                            : reader["State"].ToString(),

                    Pincode =
                        reader["Pincode"] == DBNull.Value
                            ? null
                            : reader["Pincode"].ToString()
                });
            }

            return addresses;
        }

        #endregion


        #region Get By Id

        public async Task<Addresses?> GetById(
            int addressId,
            int customerId)
        {
            Addresses? model = null;

            using SqlConnection con = GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Addresses",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar, 30).Value =
                "SELECTBYID";


            cmd.Parameters.Add(
                "@AddressId",
                SqlDbType.Int).Value =
                addressId;


            cmd.Parameters.Add(
                "@CustomerId",
                SqlDbType.Int).Value =
                customerId;


            await con.OpenAsync();


            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();


            if (await reader.ReadAsync())
            {
                model = new Addresses
                {
                    AddressId =
                        Convert.ToInt32(
                            reader["AddressId"]),

                    CustomerId =
                        Convert.ToInt32(
                            reader["CustomerId"]),

                    AddressLine1 =
                        reader["AddressLine1"] == DBNull.Value
                            ? null
                            : reader["AddressLine1"].ToString(),

                    City =
                        reader["City"] == DBNull.Value
                            ? null
                            : reader["City"].ToString(),

                    State =
                        reader["State"] == DBNull.Value
                            ? null
                            : reader["State"].ToString(),

                    Pincode =
                        reader["Pincode"] == DBNull.Value
                            ? null
                            : reader["Pincode"].ToString()
                };
            }

            return model;
        }

        #endregion


        #region Add

        public async Task<int> Add(
            Addresses model)
        {
            if (model == null)
            {
                throw new ArgumentNullException(
                    nameof(model));
            }


            if (model.CustomerId <= 0)
            {
                throw new ArgumentException(
                    "Invalid CustomerId.",
                    nameof(model));
            }


            using SqlConnection con = GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Addresses",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            // -----------------------------------------
            // Parameters
            // -----------------------------------------

            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar, 30).Value =
                "INSERT";


            cmd.Parameters.Add(
                "@CustomerId",
                SqlDbType.Int).Value =
                model.CustomerId;


            cmd.Parameters.Add(
                "@AddressLine1",
                SqlDbType.VarChar, 500).Value =
                (object?)model.AddressLine1 ?? DBNull.Value;


            cmd.Parameters.Add(
                "@City",
                SqlDbType.VarChar, 100).Value =
                (object?)model.City ?? DBNull.Value;


            cmd.Parameters.Add(
                "@State",
                SqlDbType.VarChar, 100).Value =
                (object?)model.State ?? DBNull.Value;


            cmd.Parameters.Add(
                "@Pincode",
                SqlDbType.VarChar, 20).Value =
                (object?)model.Pincode ?? DBNull.Value;


            // -----------------------------------------
            // Execute
            // -----------------------------------------

            await con.OpenAsync();


            object? result =
                await cmd.ExecuteScalarAsync();


            if (result == null ||
                result == DBNull.Value)
            {
                return 0;
            }


            return Convert.ToInt32(result);
        }

        #endregion


        #region Update

        public async Task<bool> Update(
            Addresses model)
        {
            using SqlConnection con = GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Addresses",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar, 30).Value =
                "UPDATE";


            cmd.Parameters.Add(
                "@AddressId",
                SqlDbType.Int).Value =
                model.AddressId;


            cmd.Parameters.Add(
                "@CustomerId",
                SqlDbType.Int).Value =
                model.CustomerId;


            cmd.Parameters.Add(
                "@AddressLine1",
                SqlDbType.VarChar, 500).Value =
                (object?)model.AddressLine1 ?? DBNull.Value;


            cmd.Parameters.Add(
                "@City",
                SqlDbType.VarChar, 100).Value =
                (object?)model.City ?? DBNull.Value;


            cmd.Parameters.Add(
                "@State",
                SqlDbType.VarChar, 100).Value =
                (object?)model.State ?? DBNull.Value;


            cmd.Parameters.Add(
                "@Pincode",
                SqlDbType.VarChar, 20).Value =
                (object?)model.Pincode ?? DBNull.Value;


            await con.OpenAsync();


            int rows =
                await cmd.ExecuteNonQueryAsync();


            return rows > 0;
        }

        #endregion


        #region Delete

        public async Task<bool> Delete(
            int addressId,
            int customerId)
        {
            using SqlConnection con = GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Addresses",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar, 30).Value =
                "DELETE";


            cmd.Parameters.Add(
                "@AddressId",
                SqlDbType.Int).Value =
                addressId;


            cmd.Parameters.Add(
                "@CustomerId",
                SqlDbType.Int).Value =
                customerId;


            await con.OpenAsync();


            int rows =
                await cmd.ExecuteNonQueryAsync();


            return rows > 0;
        }

        #endregion
    }
}