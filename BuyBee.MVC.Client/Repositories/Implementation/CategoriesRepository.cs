using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class CategoriesRepository : ICategoriesRepository
    {
        private readonly IConfiguration _configuration;

        public CategoriesRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DBCS"));
        }

        public List<Categories> GetAll()
        {
            List<Categories> list = new();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Category", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "SELECTALL");

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    list.Add(new Categories
                    {
                        CategoryId = Convert.ToInt32(dr["CategoryId"]),
                        CategoryName = dr["CategoryName"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        CreatedOn = dr["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["CreatedOn"]),
                        UpdatedOn = dr["UpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedOn"])
                    });
                }
            }

            return list;
        }

        public Categories? GetById(int id)
        {
            Categories category = new();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Category", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "SELECT");
                cmd.Parameters.AddWithValue("@CategoryId", id);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    category.CategoryId = Convert.ToInt32(dr["CategoryId"]);
                    category.CategoryName = dr["CategoryName"].ToString();
                    category.IsActive = Convert.ToBoolean(dr["IsActive"]);
                }
            }

            return category;
        }

        public void Insert(Categories category)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Category", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "INSERT");
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                cmd.Parameters.AddWithValue("@IsActive", category.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy", 1);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public void Update(Categories category)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Category", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "UPDATE");
                cmd.Parameters.AddWithValue("@CategoryId", category.CategoryId);
                cmd.Parameters.AddWithValue("@CategoryName", category.CategoryName);
                cmd.Parameters.AddWithValue("@IsActive", category.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedBy", 1);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Category", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "DELETE");
                cmd.Parameters.AddWithValue("@CategoryId", id);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }
    }
}