using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class RolesRepository : IRolesRepository
    {
        private readonly IConfiguration _configuration;
        public RolesRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string ConnectionString => _configuration.GetConnectionString("DBCS");
        public void Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Role", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "DELETE");
                cmd.Parameters.AddWithValue("@RoleId", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Roles> GetAll()
        {
            List<Roles> list = new List<Roles>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Role", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "SELECTALL");
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    list.Add(new Roles
                    {
                        RoleId = Convert.ToInt32(dr["RoleId"]),
                        RoleName = dr["RoleName"].ToString(),
                        CreatedOn = dr["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["CreatedOn"]),
                        UpdatedOn = dr["UpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedOn"])
                    });
                }
            }
            return list;
        }

        public Roles GetById(int id)
        {
            Roles roles = new Roles();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Role", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "SELECT");
                cmd.Parameters.AddWithValue("@RoleId", id);
                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    roles.RoleId = Convert.ToInt32(dr["RoleId"]);
                    roles.RoleName = dr["RoleName"].ToString();
                    
                }
            }
            return roles;
        }

        public void Insert(Roles model)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Role", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "INSERT");
                cmd.Parameters.AddWithValue("@RoleName", model.RoleName);
                cmd.Parameters.AddWithValue("@CreatedBy", 1);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        public void Update(Roles model)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Role", con);

                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "UPDATE");
                cmd.Parameters.AddWithValue("@RoleId", model.RoleId);
                cmd.Parameters.AddWithValue("@RoleName", model.RoleName);
                cmd.Parameters.AddWithValue("@UpdatedBy", 1);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }
    }
}
