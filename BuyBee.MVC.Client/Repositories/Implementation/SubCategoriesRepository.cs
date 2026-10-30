using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class SubCategoriesRepository : ISubCategoriesRepository
    {
        private readonly IConfiguration _configuration;

        public SubCategoriesRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DBCS"));
        }

        public PagedResult<SubCategories> GetAll(string? searchText, int pageNumber, int pageSize)
        {
            List<SubCategories> list = new();
            
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_SubCategory", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "SELECTALL");
                cmd.Parameters.AddWithValue("@SearchText", searchText ?? "");
                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();
                

                while (dr.Read())
                {
                   
                    list.Add(new SubCategories
                    {
                        SubCategoryId = Convert.ToInt32(dr["SubCategoryId"]),
                        CategoryId = Convert.ToInt32(dr["CategoryId"]),
                        CategoryName = dr["CategoryName"].ToString(),
                        SubCategoryName = dr["SubCategoryName"].ToString() ?? "",
                        IsActive = Convert.ToBoolean(dr["IsActive"])
                    });
                }
            }

            PagedResult<SubCategories> result = new PagedResult<SubCategories>
            {
                Items = list,
                SearchText = searchText ?? "",
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = GetTotalCount(searchText)
            };
            return result;
        }

        public SubCategories? GetById(int id)
        {
            SubCategories subCategory = new();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_SubCategory", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "SELECT");
                cmd.Parameters.AddWithValue("@SubCategoryId", id);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    subCategory.SubCategoryId = Convert.ToInt32(dr["SubCategoryId"]);
                    subCategory.CategoryId = Convert.ToInt32(dr["CategoryId"]);
                    subCategory.SubCategoryName = dr["SubCategoryName"].ToString() ?? "";
                    subCategory.CategoryName = dr["CategoryName"].ToString();
                    subCategory.IsActive = Convert.ToBoolean(dr["IsActive"]);
                }
            }

            return subCategory;
        }

        public void Insert(SubCategories subCategory)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_SubCategory", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "INSERT");
                cmd.Parameters.AddWithValue("@CategoryId", subCategory.CategoryId);
                cmd.Parameters.AddWithValue("@SubCategoryName", subCategory.SubCategoryName);
                cmd.Parameters.AddWithValue("@IsActive", subCategory.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy", 1);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Update(SubCategories subCategory)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_SubCategory", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "UPDATE");
                cmd.Parameters.AddWithValue("@SubCategoryId", subCategory.SubCategoryId);
                cmd.Parameters.AddWithValue("@CategoryId", subCategory.CategoryId);
                cmd.Parameters.AddWithValue("@SubCategoryName", subCategory.SubCategoryName);
                cmd.Parameters.AddWithValue("@IsActive", subCategory.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedBy", 1);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public void Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_SubCategory", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "DELETE");
                cmd.Parameters.AddWithValue("@SubCategoryId", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Categories> GetCategories()
        {
            List<Categories> list = new();
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_SubCategory", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "SELECTCATEGORY");
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new Categories
                    {
                        CategoryId = Convert.ToInt32(dr["CategoryId"]),
                        CategoryName = dr["CategoryName"].ToString()
                    });
                }
            }
            return list;
        }

        public int GetTotalCount(string? searchText)
        {
            int total = 0;
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_SubCategory", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "TOTALCOUNT");
                cmd.Parameters.AddWithValue("@SearchText", searchText ?? "");
                con.Open();
                total = Convert.ToInt32(cmd.ExecuteScalar());
            }
            return total;
        }

        //public List<SubCategories> GetByCategory(int? categoryId)
        //{
        //    List<SubCategories> list = new List<SubCategories>();

        //    using (SqlConnection con = GetConnection())
        //    {
        //        SqlCommand cmd = new SqlCommand("sp_SubCategory", con);
        //        cmd.CommandType = CommandType.StoredProcedure;

        //        cmd.Parameters.AddWithValue("@Flag", "SELECTBYCATEGORY");
        //        cmd.Parameters.AddWithValue("@CategoryId", categoryId);

        //        con.Open();

        //        SqlDataReader dr = cmd.ExecuteReader();

        //        while (dr.Read())
        //        {
        //            list.Add(new SubCategories
        //            {
        //                SubCategoryId = Convert.ToInt32(dr["SubCategoryId"]),
        //                SubCategoryName = dr["SubCategoryName"].ToString()
        //            });
        //        }
        //    }

        //    return list;
        //}

        public PagedResult<SubCategoryVM> GetByCategory(int? categoryId)
        {
            PagedResult<SubCategoryVM> result = new PagedResult<SubCategoryVM>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_SubCategory", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "SELECTBYCATEGORY");
                cmd.Parameters.AddWithValue("@CategoryId", categoryId);

                con.Open();

                SqlDataReader reader = cmd.ExecuteReader();

                List<SubCategoryVM> list = new List<SubCategoryVM>();

                while (reader.Read())
                {
                    list.Add(new SubCategoryVM
                    {
                        SubCategoryId = Convert.ToInt32(reader["SubCategoryId"]),
                        SubCategoryName = reader["SubCategoryName"].ToString()
                    });
                }

                result.Items = list;
            }

            return result;
        }
    }
}