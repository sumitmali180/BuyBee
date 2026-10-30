using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class CartRepository : ICartRepository
    {
        private readonly IConfiguration _configuration;

        public CartRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DBCS"));
        }

        #region Add To Cart

        public async Task<bool> AddToCart(int customerId, int productId, int quantity)
        {
            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_Cart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "ADD");
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);
                    cmd.Parameters.AddWithValue("@ProductId", productId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return true;
        }

        #endregion

        #region Get Cart

        public async Task<List<CartItem>> GetCart(int customerId)
        {
            List<CartItem> cart = new();

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_Cart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "GETBYCUSTOMER");
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);

                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            cart.Add(new CartItem
                            {
                                CartId = Convert.ToInt32(reader["CartId"]),
                                CustomerId = Convert.ToInt32(reader["CustomerId"]),
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                ProductName = reader["ProductName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                ThumbnailImage = reader["ThumbnailImage"].ToString()
                            });
                        }
                    }
                }
            }

            return cart;
        }

        #endregion

        #region Update Quantity

        public async Task<bool> UpdateQuantity(int cartId, int quantity)
        {
            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_Cart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "UPDATE");
                    cmd.Parameters.AddWithValue("@CartId", cartId);
                    cmd.Parameters.AddWithValue("@Quantity", quantity);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return true;
        }

        #endregion

        #region Remove Item

        public async Task<bool> Remove(int cartId)
        {
            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_Cart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "DELETE");
                    cmd.Parameters.AddWithValue("@CartId", cartId);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return true;
        }

        #endregion

        #region Cart Count

        public async Task<int> GetCartCount(int customerId)
        {
            int count = 0;

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_Cart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "COUNT");
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);

                    await con.OpenAsync();

                    object? result = await cmd.ExecuteScalarAsync();

                    if (result != null)
                    {
                        count = Convert.ToInt32(result);
                    }
                }
            }

            return count;
        }

        #endregion

        #region Clear Cart

        public async Task<bool> ClearCart(int customerId)
        {
            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_Cart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "CLEAR");
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);

                    await con.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return true;
        }

        #endregion

        public async Task<List<CartItem>> GetMiniCart(int customerId)
        {
            List<CartItem> cart = new();

            using (SqlConnection con = GetConnection())
            {
                using (SqlCommand cmd = new SqlCommand("SP_Cart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@Flag", "MINICART");
                    cmd.Parameters.AddWithValue("@CustomerId", customerId);

                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            cart.Add(new CartItem
                            {
                                CartId = Convert.ToInt32(reader["CartId"]),
                                CustomerId = Convert.ToInt32(reader["CustomerId"]),
                                ProductId = Convert.ToInt32(reader["ProductId"]),
                                ProductName = reader["ProductName"].ToString(),
                                Price = Convert.ToDecimal(reader["Price"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                ThumbnailImage = reader["ThumbnailImage"].ToString()
                            });
                        }
                    }
                }
            }

            return cart;
        }
    }
}