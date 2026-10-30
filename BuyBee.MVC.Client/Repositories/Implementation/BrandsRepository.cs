using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class BrandsRepository : IBrandsRepository
    {
        private readonly IConfiguration _configuration;
        public BrandsRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private string ConnectionString => _configuration.GetConnectionString("DBCS");

        public void Delete(int id)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Brand", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "DELETE");
                cmd.Parameters.AddWithValue("@BrandId", id);
                con.Open();
                cmd.ExecuteNonQuery();
            }
        }

        public List<Brands> GetAll()
        {
            List<Brands> list = new List<Brands>();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Brand", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "SELECTALL");
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    bool logoColumnChecked = false;
                    int logoColIndex = -1;

                    while (dr.Read())
                    {
                        if (!logoColumnChecked)
                        {
                            logoColIndex = FindLogoColumnIndex(dr);
                            logoColumnChecked = true;
                        }

                        string? dbLogo = null;
                        if (logoColIndex >= 0 && dr[logoColIndex] != DBNull.Value)
                        {
                            dbLogo = dr[logoColIndex].ToString();
                        }

                        var brand = new Brands
                        {
                            BrandId = Convert.ToInt32(dr["BrandId"]),
                            BrandName = dr["BrandName"].ToString(),
                            BrandLogo = dbLogo,
                            IsActive = Convert.ToBoolean(dr["IsActive"]),
                            CreatedOn = dr["CreatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["CreatedOn"]),
                            UpdatedOn = dr["UpdatedOn"] == DBNull.Value ? null : Convert.ToDateTime(dr["UpdatedOn"])
                        };

                        list.Add(brand);
                    }
                }

                // If sp_Brand did not return BrandLogo, attempt reading from Brands table if column exists
                if (list.Any() && list.All(b => string.IsNullOrEmpty(b.BrandLogo)))
                {
                    TryHydrateBrandLogosDirect(con, list);
                }
            }
            return list;
        }

        public Brands? GetById(int id)
        {
            Brands brands = new Brands();
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                SqlCommand cmd = new SqlCommand("sp_Brand", con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Flag", "SELECT");
                cmd.Parameters.AddWithValue("@BrandId", id);
                con.Open();
                using (SqlDataReader dr = cmd.ExecuteReader())
                {
                    if (dr.Read())
                    {
                        int logoColIndex = FindLogoColumnIndex(dr);
                        string? dbLogo = null;
                        if (logoColIndex >= 0 && dr[logoColIndex] != DBNull.Value)
                        {
                            dbLogo = dr[logoColIndex].ToString();
                        }

                        brands.BrandId = Convert.ToInt32(dr["BrandId"]);
                        brands.BrandName = dr["BrandName"].ToString();
                        brands.BrandLogo = dbLogo;
                        brands.IsActive = Convert.ToBoolean(dr["IsActive"]);
                    }
                    else
                    {
                        return null;
                    }
                }

                if (string.IsNullOrEmpty(brands.BrandLogo))
                {
                    TryHydrateSingleBrandLogoDirect(con, brands);
                }
            }
            return brands;
        }

        public void Insert(Brands brands)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                // Attempt to call sp_Brand with @BrandLogo parameter
                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_Brand", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Flag", "INSERT");
                        cmd.Parameters.AddWithValue("@BrandName", brands.BrandName);
                        cmd.Parameters.AddWithValue("@BrandLogo", (object?)brands.BrandLogo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive", brands.IsActive);
                        cmd.Parameters.AddWithValue("@CreatedBy", 1);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex) when (ex.Message.Contains("too many arguments") || ex.Message.Contains("BrandLogo"))
                {
                    // Fallback to legacy sp_Brand signature without @BrandLogo
                    using (SqlCommand cmd = new SqlCommand("sp_Brand", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Flag", "INSERT");
                        cmd.Parameters.AddWithValue("@BrandName", brands.BrandName);
                        cmd.Parameters.AddWithValue("@IsActive", brands.IsActive);
                        cmd.Parameters.AddWithValue("@CreatedBy", 1);
                        cmd.ExecuteNonQuery();
                    }
                }

                // If BrandLogo was supplied and column exists in Brands table, persist directly
                if (!string.IsNullOrEmpty(brands.BrandLogo))
                {
                    TryDirectUpdateLogo(con, brands.BrandName, brands.BrandLogo);
                }
            }
        }

        public void Update(Brands brands)
        {
            using (SqlConnection con = new SqlConnection(ConnectionString))
            {
                con.Open();

                try
                {
                    using (SqlCommand cmd = new SqlCommand("sp_Brand", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Flag", "UPDATE");
                        cmd.Parameters.AddWithValue("@BrandId", brands.BrandId);
                        cmd.Parameters.AddWithValue("@BrandName", brands.BrandName);
                        cmd.Parameters.AddWithValue("@BrandLogo", (object?)brands.BrandLogo ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IsActive", brands.IsActive);
                        cmd.Parameters.AddWithValue("@UpdatedBy", 1);
                        cmd.ExecuteNonQuery();
                    }
                }
                catch (SqlException ex) when (ex.Message.Contains("too many arguments") || ex.Message.Contains("BrandLogo"))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_Brand", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@Flag", "UPDATE");
                        cmd.Parameters.AddWithValue("@BrandId", brands.BrandId);
                        cmd.Parameters.AddWithValue("@BrandName", brands.BrandName);
                        cmd.Parameters.AddWithValue("@IsActive", brands.IsActive);
                        cmd.Parameters.AddWithValue("@UpdatedBy", 1);
                        cmd.ExecuteNonQuery();
                    }
                }

                if (!string.IsNullOrEmpty(brands.BrandLogo))
                {
                    TryDirectUpdateLogoById(con, brands.BrandId, brands.BrandLogo);
                }
            }
        }

        #region Helpers

        private static int FindLogoColumnIndex(SqlDataReader dr)
        {
            for (int i = 0; i < dr.FieldCount; i++)
            {
                string name = dr.GetName(i);
                if (string.Equals(name, "BrandLogo", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "brand_logo", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(name, "LogoPath", StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }
            return -1;
        }

        private static void TryHydrateBrandLogosDirect(SqlConnection con, List<Brands> list)
        {
            try
            {
                using (SqlCommand checkCmd = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Brands' AND COLUMN_NAME = 'BrandLogo') " +
                    "SELECT BrandId, BrandLogo FROM [dbo].[Brands] WHERE BrandLogo IS NOT NULL;", con))
                {
                    using (SqlDataReader logoReader = checkCmd.ExecuteReader())
                    {
                        var logoMap = new Dictionary<int, string>();
                        while (logoReader.Read())
                        {
                            int bId = Convert.ToInt32(logoReader["BrandId"]);
                            string? logo = logoReader["BrandLogo"] == DBNull.Value ? null : logoReader["BrandLogo"].ToString();
                            if (!string.IsNullOrEmpty(logo))
                            {
                                logoMap[bId] = logo;
                            }
                        }

                        foreach (var b in list)
                        {
                            if (logoMap.TryGetValue(b.BrandId, out var directLogo))
                            {
                                b.BrandLogo = directLogo;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Graceful degradation: display helper DisplayLogo will provide vector assets
            }
        }

        private static void TryHydrateSingleBrandLogoDirect(SqlConnection con, Brands brand)
        {
            try
            {
                using (SqlCommand checkCmd = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Brands' AND COLUMN_NAME = 'BrandLogo') " +
                    "SELECT BrandLogo FROM [dbo].[Brands] WHERE BrandId = @Id;", con))
                {
                    checkCmd.Parameters.AddWithValue("@Id", brand.BrandId);
                    var result = checkCmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        brand.BrandLogo = result.ToString();
                    }
                }
            }
            catch
            {
                // Graceful fallback
            }
        }

        private static void TryDirectUpdateLogo(SqlConnection con, string? brandName, string brandLogo)
        {
            if (string.IsNullOrEmpty(brandName)) return;
            try
            {
                using (SqlCommand updateCmd = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Brands' AND COLUMN_NAME = 'BrandLogo') " +
                    "UPDATE [dbo].[Brands] SET [BrandLogo] = @Logo WHERE [BrandName] = @Name;", con))
                {
                    updateCmd.Parameters.AddWithValue("@Logo", brandLogo);
                    updateCmd.Parameters.AddWithValue("@Name", brandName);
                    updateCmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Silently continue
            }
        }

        private static void TryDirectUpdateLogoById(SqlConnection con, int brandId, string brandLogo)
        {
            try
            {
                using (SqlCommand updateCmd = new SqlCommand(
                    "IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Brands' AND COLUMN_NAME = 'BrandLogo') " +
                    "UPDATE [dbo].[Brands] SET [BrandLogo] = @Logo WHERE [BrandId] = @Id;", con))
                {
                    updateCmd.Parameters.AddWithValue("@Logo", brandLogo);
                    updateCmd.Parameters.AddWithValue("@Id", brandId);
                    updateCmd.ExecuteNonQuery();
                }
            }
            catch
            {
                // Silently continue
            }
        }

        #endregion
    }
}
