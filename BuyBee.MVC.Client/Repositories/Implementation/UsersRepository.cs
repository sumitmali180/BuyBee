using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class UsersRepository : IUsersRepository
    {
        private readonly IConfiguration _configuration;
        public UsersRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DBCS"));
        }
        public void Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Users", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "DELETE");
                cmd.Parameters.AddWithValue("@UserId", id);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        public bool EmailExists(string email, int? userId = null)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Users", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "EMAILEXISTS");
                cmd.Parameters.AddWithValue("@Email", email);

                if (userId.HasValue)
                    cmd.Parameters.AddWithValue("@UserId", userId.Value);
                else
                    cmd.Parameters.AddWithValue("@UserId", DBNull.Value);

                con.Open();

                int count = Convert.ToInt32(cmd.ExecuteScalar());

                con.Close();

                return count > 0;
            }
        }

        public IEnumerable<UserVM> GetAll()
        {
            List<UserVM> users = new List<UserVM>();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Users", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "SELECT");

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    users.Add(new UserVM
                    {
                        UserId = Convert.ToInt32(dr["UserId"]),
                        FullName = dr["FullName"].ToString(),
                        Email = dr["Email"].ToString(),
                        RoleId = Convert.ToInt32(dr["RoleId"]),
                        RoleName = dr["RoleName"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        CreatedOn = dr["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["CreatedOn"]),
                        UpdatedOn = dr["UpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedOn"]),
                        CreatedBy = dr["CreatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["CreatedBy"]),
                        UpdatedBy = dr["UpdatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["UpdatedBy"])
                    });
                }

                con.Close();
            }

            return users;
        }

        public UserVM? GetById(int id)
        {
            UserVM user = null;

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Users", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "SELECTBYID");
                cmd.Parameters.AddWithValue("@UserId", id);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    user = new UserVM
                    {
                        UserId = Convert.ToInt32(dr["UserId"]),
                        FullName = dr["FullName"].ToString(),
                        Email = dr["Email"].ToString(),
                        Password = dr["PasswordHash"].ToString(),
                        RoleId = Convert.ToInt32(dr["RoleId"]),
                        RoleName = dr["RoleName"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        CreatedOn = dr["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["CreatedOn"]),
                        UpdatedOn = dr["UpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedOn"]),
                        CreatedBy = dr["CreatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["CreatedBy"]),
                        UpdatedBy = dr["UpdatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["UpdatedBy"])
                    };
                }

                con.Close();
            }

            return user;
        }

        public void Insert(UserVM user)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Users", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "INSERT");
                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", user.Password);
                cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy",
                    user.CreatedBy.HasValue ? user.CreatedBy.Value : DBNull.Value);

                con.Open();

                cmd.ExecuteNonQuery();

                con.Close();
            }
        }

        public void Update(UserVM user)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Users", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "UPDATE");
                cmd.Parameters.AddWithValue("@UserId", user.UserId);
                cmd.Parameters.AddWithValue("@FullName", user.FullName);
                cmd.Parameters.AddWithValue("@Email", user.Email);
                cmd.Parameters.AddWithValue("@RoleId", user.RoleId);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedBy",
                    user.UpdatedBy.HasValue ? user.UpdatedBy.Value : DBNull.Value);

                con.Open();
                cmd.ExecuteNonQuery();
                con.Close();
            }
        }

        public UserVM? Login(string email, string password)
        {
            UserVM? user = null;

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("sp_Users", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "LOGIN");
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@PasswordHash", password);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    user = new UserVM
                    {
                        UserId = Convert.ToInt32(dr["UserId"]),

                        FullName = dr["FullName"].ToString(),

                        Email = dr["Email"].ToString(),

                        RoleId = Convert.ToInt32(dr["RoleId"]),

                        RoleName = dr["RoleName"].ToString(),

                        IsActive = Convert.ToBoolean(dr["IsActive"])
                    };
                }

                con.Close();
            }

            return user;
        }

        public UserVM? GetByEmail(string email)
        {
            UserVM? user = null;

            using (SqlConnection con = GetConnection())
            {
                string sql = @"
                    SELECT u.UserId, u.FullName, u.Email, u.PasswordHash, u.RoleId, r.RoleName, u.IsActive 
                    FROM Users u 
                    LEFT JOIN Roles r ON u.RoleId = r.RoleId 
                    WHERE u.Email = @Email";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            user = new UserVM
                            {
                                UserId = Convert.ToInt32(dr["UserId"]),
                                FullName = dr["FullName"]?.ToString(),
                                Email = dr["Email"]?.ToString(),
                                Password = dr["PasswordHash"]?.ToString(),
                                RoleId = dr["RoleId"] == DBNull.Value ? 0 : Convert.ToInt32(dr["RoleId"]),
                                RoleName = dr["RoleName"]?.ToString(),
                                IsActive = dr["IsActive"] != DBNull.Value && Convert.ToBoolean(dr["IsActive"])
                            };
                        }
                    }
                }
            }

            return user;
        }

        public void UpdatePassword(int userId, string passwordHash)
        {
            using (SqlConnection con = GetConnection())
            {
                string sql = "UPDATE Users SET PasswordHash = @PasswordHash, UpdatedOn = GETDATE() WHERE UserId = @UserId";
                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}