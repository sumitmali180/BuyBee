using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class UnitsRepository : IUnitsRepository
    {
        private readonly IConfiguration _configuration;

        public UnitsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DBCS"));
        }

        #region Get All

        public List<Units> GetAll()
        {
            List<Units> units = new();

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("SP_Units", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "SELECTALL");

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    units.Add(new Units
                    {
                        UnitId = Convert.ToInt32(dr["UnitId"]),
                        UnitName = dr["UnitName"].ToString(),
                        UnitShortName = dr["UnitShortName"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        CreatedOn = dr["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["CreatedOn"]),
                        UpdatedOn = dr["UpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedOn"]),
                        CreatedBy = dr["CreatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["CreatedBy"]),
                        UpdatedBy = dr["UpdatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["UpdatedBy"])
                    });
                }
            }

            return units;
        }

        #endregion

        #region Get By Id

        public Units GetById(int id)
        {
            Units unit = null;

            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("SP_Units", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "SELECTBYID");
                cmd.Parameters.AddWithValue("@UnitId", id);

                con.Open();

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    unit = new Units
                    {
                        UnitId = Convert.ToInt32(dr["UnitId"]),
                        UnitName = dr["UnitName"].ToString(),
                        UnitShortName = dr["UnitShortName"].ToString(),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        CreatedOn = dr["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["CreatedOn"]),
                        UpdatedOn = dr["UpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedOn"]),
                        CreatedBy = dr["CreatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["CreatedBy"]),
                        UpdatedBy = dr["UpdatedBy"] == DBNull.Value ? null : Convert.ToInt32(dr["UpdatedBy"])
                    };
                }
            }

            return unit;
        }

        #endregion

        #region Insert

        public void Insert(Units unit)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("SP_Units", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "INSERT");
                cmd.Parameters.AddWithValue("@UnitName", unit.UnitName);
                cmd.Parameters.AddWithValue("@UnitShortName", unit.UnitShortName ?? "");
                cmd.Parameters.AddWithValue("@IsActive", unit.IsActive);
                cmd.Parameters.AddWithValue("@CreatedBy", unit.CreatedBy ?? 1);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Update

        public void Update(Units unit)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("SP_Units", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "UPDATE");
                cmd.Parameters.AddWithValue("@UnitId", unit.UnitId);
                cmd.Parameters.AddWithValue("@UnitName", unit.UnitName);
                cmd.Parameters.AddWithValue("@UnitShortName", unit.UnitShortName ?? "");
                cmd.Parameters.AddWithValue("@IsActive", unit.IsActive);
                cmd.Parameters.AddWithValue("@UpdatedBy", unit.UpdatedBy ?? 1);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        #endregion

        #region Delete

        public void Delete(int id)
        {
            using (SqlConnection con = GetConnection())
            {
                SqlCommand cmd = new SqlCommand("SP_Units", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@Flag", "DELETE");
                cmd.Parameters.AddWithValue("@UnitId", id);

                con.Open();

                cmd.ExecuteNonQuery();
            }
        }

        #endregion
    }
}