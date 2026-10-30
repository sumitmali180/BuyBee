using BuyBee.MVC.Client.ViewModels;
using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class VendorRepository : IVendorRepository
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<VendorRepository> _logger;

        public VendorRepository(IConfiguration configuration, ILogger<VendorRepository> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        private SqlConnection GetConnection()
        {
            string? connectionString = _configuration.GetConnectionString("DBCS");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DBCS' was not found.");
            }
            return new SqlConnection(connectionString);
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        // =========================================================================
        // 1. VENDOR ACCOUNT & PROFILE
        // =========================================================================

        public async Task<Vendors?> GetVendorByUserIdAsync(int userId)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM dbo.Vendors WHERE UserId = @UserId", con);
            cmd.Parameters.Add("@UserId", SqlDbType.Int).Value = userId;

            await con.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapVendor(reader);
            }
            return null;
        }

        public async Task<Vendors?> GetVendorByIdAsync(int vendorId)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SELECT TOP 1 * FROM dbo.Vendors WHERE VendorId = @VendorId", con);
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;

            await con.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapVendor(reader);
            }
            return null;
        }

        public async Task<bool> UpdateVendorProfileAsync(Vendors vendor)
        {
            using SqlConnection con = GetConnection();
            string sql = @"
                UPDATE dbo.Vendors
                SET StoreName = @StoreName,
                    StoreDescription = @StoreDescription,
                    StoreLogo = ISNULL(@StoreLogo, StoreLogo),
                    ContactEmail = @ContactEmail,
                    ContactPhone = @ContactPhone,
                    BusinessAddress = @BusinessAddress,
                    City = @City,
                    State = @State,
                    Pincode = @Pincode,
                    GstNumber = @GstNumber,
                    PanNumber = @PanNumber,
                    BankAccountNumber = @BankAccountNumber,
                    BankIfsc = @BankIfsc,
                    UpdatedOn = GETDATE()
                WHERE VendorId = @VendorId";

            using SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@StoreName", SqlDbType.VarChar, 150).Value = vendor.StoreName;
            cmd.Parameters.Add("@StoreDescription", SqlDbType.VarChar, 500).Value = (object?)vendor.StoreDescription ?? DBNull.Value;
            cmd.Parameters.Add("@StoreLogo", SqlDbType.VarChar, 300).Value = (object?)vendor.StoreLogo ?? DBNull.Value;
            cmd.Parameters.Add("@ContactEmail", SqlDbType.VarChar, 100).Value = (object?)vendor.ContactEmail ?? DBNull.Value;
            cmd.Parameters.Add("@ContactPhone", SqlDbType.VarChar, 20).Value = (object?)vendor.ContactPhone ?? DBNull.Value;
            cmd.Parameters.Add("@BusinessAddress", SqlDbType.VarChar, 300).Value = (object?)vendor.BusinessAddress ?? DBNull.Value;
            cmd.Parameters.Add("@City", SqlDbType.VarChar, 100).Value = (object?)vendor.City ?? DBNull.Value;
            cmd.Parameters.Add("@State", SqlDbType.VarChar, 100).Value = (object?)vendor.State ?? DBNull.Value;
            cmd.Parameters.Add("@Pincode", SqlDbType.VarChar, 20).Value = (object?)vendor.Pincode ?? DBNull.Value;
            cmd.Parameters.Add("@GstNumber", SqlDbType.VarChar, 50).Value = (object?)vendor.GstNumber ?? DBNull.Value;
            cmd.Parameters.Add("@PanNumber", SqlDbType.VarChar, 50).Value = (object?)vendor.PanNumber ?? DBNull.Value;
            cmd.Parameters.Add("@BankAccountNumber", SqlDbType.VarChar, 50).Value = (object?)vendor.BankAccountNumber ?? DBNull.Value;
            cmd.Parameters.Add("@BankIfsc", SqlDbType.VarChar, 20).Value = (object?)vendor.BankIfsc ?? DBNull.Value;
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendor.VendorId;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static Vendors MapVendor(SqlDataReader reader)
        {
            return new Vendors
            {
                VendorId = Convert.ToInt32(reader["VendorId"]),
                UserId = Convert.ToInt32(reader["UserId"]),
                StoreName = reader["StoreName"].ToString() ?? string.Empty,
                StoreSlug = reader["StoreSlug"].ToString() ?? string.Empty,
                StoreDescription = reader["StoreDescription"] != DBNull.Value ? reader["StoreDescription"].ToString() : null,
                StoreLogo = reader["StoreLogo"] != DBNull.Value ? reader["StoreLogo"].ToString() : null,
                ContactEmail = reader["ContactEmail"] != DBNull.Value ? reader["ContactEmail"].ToString() : null,
                ContactPhone = reader["ContactPhone"] != DBNull.Value ? reader["ContactPhone"].ToString() : null,
                BusinessAddress = reader["BusinessAddress"] != DBNull.Value ? reader["BusinessAddress"].ToString() : null,
                City = reader["City"] != DBNull.Value ? reader["City"].ToString() : null,
                State = reader["State"] != DBNull.Value ? reader["State"].ToString() : null,
                Pincode = reader["Pincode"] != DBNull.Value ? reader["Pincode"].ToString() : null,
                GstNumber = reader["GstNumber"] != DBNull.Value ? reader["GstNumber"].ToString() : null,
                PanNumber = reader["PanNumber"] != DBNull.Value ? reader["PanNumber"].ToString() : null,
                BankAccountNumber = reader["BankAccountNumber"] != DBNull.Value ? reader["BankAccountNumber"].ToString() : null,
                BankIfsc = reader["BankIfsc"] != DBNull.Value ? reader["BankIfsc"].ToString() : null,
                CommissionRate = Convert.ToDecimal(reader["CommissionRate"]),
                IsApproved = Convert.ToBoolean(reader["IsApproved"]),
                CreatedOn = Convert.ToDateTime(reader["CreatedOn"]),
                UpdatedOn = reader["UpdatedOn"] != DBNull.Value ? Convert.ToDateTime(reader["UpdatedOn"]) : null
            };
        }

        // =========================================================================
        // 2. DASHBOARD
        // =========================================================================

        public async Task<VendorDashboardViewModel> GetDashboardMetricsAsync(int vendorId)
        {
            var vm = new VendorDashboardViewModel { VendorId = vendorId };

            using SqlConnection con = GetConnection();
            await con.OpenAsync();

            // 1. Metrics
            using (SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Dashboard", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "METRICS";
                cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;

                using SqlDataReader r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    vm.TotalProducts = HasColumn(r, "TotalProducts") ? Convert.ToInt32(r["TotalProducts"]) : 0;
                    vm.ActiveProducts = HasColumn(r, "ActiveProducts") ? Convert.ToInt32(r["ActiveProducts"]) : 0;
                    vm.LowStockCount = HasColumn(r, "LowStockCount") ? Convert.ToInt32(r["LowStockCount"]) : 0;
                    vm.OutOfStockCount = HasColumn(r, "OutOfStockCount") ? Convert.ToInt32(r["OutOfStockCount"]) : 0;
                    vm.TotalOrders = HasColumn(r, "TotalOrders") ? Convert.ToInt32(r["TotalOrders"]) : 0;
                    vm.PendingOrders = HasColumn(r, "PendingOrders") ? Convert.ToInt32(r["PendingOrders"]) : 0;
                    vm.PackedOrders = HasColumn(r, "PackedOrders") ? Convert.ToInt32(r["PackedOrders"]) : 0;
                    vm.TotalRevenue = HasColumn(r, "TotalRevenue") ? Convert.ToDecimal(r["TotalRevenue"]) : 0m;
                    vm.TotalUnitsSold = HasColumn(r, "TotalUnitsSold") ? Convert.ToInt32(r["TotalUnitsSold"]) : 0;
                    vm.CommissionRate = HasColumn(r, "CommissionRate") ? Convert.ToDecimal(r["CommissionRate"]) : 10.00m;
                }
            }

            // 2. Stock Alerts
            using (SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Dashboard", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "STOCK_ALERTS";
                cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;

                using SqlDataReader r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.StockAlerts.Add(new VendorStockAlertItem
                    {
                        ProductId = Convert.ToInt32(r["ProductId"]),
                        ProductName = r["ProductName"].ToString() ?? string.Empty,
                        Price = Convert.ToDecimal(r["Price"]),
                        StockQuantity = Convert.ToInt32(r["StockQuantity"]),
                        LowStockThreshold = Convert.ToInt32(r["LowStockThreshold"]),
                        CategoryName = r["CategoryName"] != DBNull.Value ? r["CategoryName"].ToString() : null,
                        ImagePath = r["ImagePath"] != DBNull.Value ? r["ImagePath"].ToString() : null
                    });
                }
            }

            // 3. Recent Orders
            using (SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Dashboard", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "RECENT_ORDERS";
                cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;

                using SqlDataReader r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.RecentOrders.Add(new VendorRecentOrderSummary
                    {
                        OrderId = Convert.ToInt32(r["OrderId"]),
                        OrderNumber = r["OrderNumber"].ToString() ?? string.Empty,
                        OrderDate = Convert.ToDateTime(r["OrderDate"]),
                        OrderStatus = r["OrderStatus"].ToString() ?? string.Empty,
                        VendorItemCount = Convert.ToInt32(r["VendorItemCount"]),
                        VendorTotal = Convert.ToDecimal(r["VendorTotal"]),
                        CustomerName = r["CustomerName"].ToString() ?? string.Empty,
                        RecentItemStatus = r["RecentItemStatus"] != DBNull.Value ? r["RecentItemStatus"].ToString()! : "Pending"
                    });
                }
            }

            return vm;
        }

        // =========================================================================
        // 3. PRODUCTS CATALOG & INVENTORY
        // =========================================================================

        public async Task<List<VendorProductItemViewModel>> GetVendorProductsAsync(int vendorId, int? categoryId = null, string? searchTerm = null, string stockFilter = "ALL")
        {
            var list = new List<VendorProductItemViewModel>();

            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Products", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "GET_ALL";
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
            cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value = (object?)categoryId ?? DBNull.Value;
            cmd.Parameters.Add("@SearchTerm", SqlDbType.VarChar, 100).Value = (object?)searchTerm ?? DBNull.Value;
            cmd.Parameters.Add("@StockFilter", SqlDbType.VarChar, 20).Value = stockFilter;

            await con.OpenAsync();
            using SqlDataReader r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new VendorProductItemViewModel
                {
                    ProductId = Convert.ToInt32(r["ProductId"]),
                    ProductName = r["ProductName"].ToString() ?? string.Empty,
                    Description = r["Description"] != DBNull.Value ? r["Description"].ToString() : null,
                    CategoryId = r["CategoryId"] != DBNull.Value ? Convert.ToInt32(r["CategoryId"]) : null,
                    CategoryName = r["CategoryName"] != DBNull.Value ? r["CategoryName"].ToString() : null,
                    SubCategoryId = r["SubCategoryId"] != DBNull.Value ? Convert.ToInt32(r["SubCategoryId"]) : null,
                    BrandId = r["BrandId"] != DBNull.Value ? Convert.ToInt32(r["BrandId"]) : null,
                    BrandName = r["BrandName"] != DBNull.Value ? r["BrandName"].ToString() : null,
                    Price = r["Price"] != DBNull.Value ? Convert.ToDecimal(r["Price"]) : 0m,
                    StockQuantity = Convert.ToInt32(r["StockQuantity"]),
                    LowStockThreshold = Convert.ToInt32(r["LowStockThreshold"]),
                    IsActive = Convert.ToBoolean(r["IsActive"]),
                    ApprovalStatus = r["ApprovalStatus"].ToString() ?? "Approved",
                    RejectionReason = r["RejectionReason"] != DBNull.Value ? r["RejectionReason"].ToString() : null,
                    PrimaryImage = r["PrimaryImage"] != DBNull.Value ? r["PrimaryImage"].ToString() : null,
                    ImageCount = Convert.ToInt32(r["ImageCount"]),
                    CreatedOn = Convert.ToDateTime(r["CreatedOn"])
                });
            }

            return list;
        }

        public async Task<VendorProductFormViewModel?> GetProductForEditAsync(int productId, int vendorId)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Products", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "GET_BY_ID";
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
            cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

            await con.OpenAsync();
            using SqlDataReader r = await cmd.ExecuteReaderAsync();
            if (!await r.ReadAsync())
            {
                return null; // Not found or not owned by this vendor
            }

            var model = new VendorProductFormViewModel
            {
                ProductId = Convert.ToInt32(r["ProductId"]),
                ProductName = r["ProductName"].ToString() ?? string.Empty,
                Description = r["Description"] != DBNull.Value ? r["Description"].ToString() : null,
                CategoryId = r["CategoryId"] != DBNull.Value ? Convert.ToInt32(r["CategoryId"]) : null,
                SubCategoryId = r["SubCategoryId"] != DBNull.Value ? Convert.ToInt32(r["SubCategoryId"]) : null,
                BrandId = r["BrandId"] != DBNull.Value ? Convert.ToInt32(r["BrandId"]) : null,
                UnitId = r["UnitId"] != DBNull.Value ? Convert.ToInt32(r["UnitId"]) : null,
                Price = r["Price"] != DBNull.Value ? Convert.ToDecimal(r["Price"]) : 0m,
                Quantity = Convert.ToInt32(r["StockQuantity"]),
                LowStockThreshold = Convert.ToInt32(r["LowStockThreshold"]),
                IsActive = Convert.ToBoolean(r["IsActive"]),
                ApprovalStatus = r["ApprovalStatus"].ToString() ?? "Approved"
            };

            // Second result set: Product Images
            if (await r.NextResultAsync())
            {
                while (await r.ReadAsync())
                {
                    model.ExistingImages.Add(new ProductImages
                    {
                        ImageId = Convert.ToInt32(r["ImageId"]),
                        ProductId = Convert.ToInt32(r["ProductId"]),
                        ImagePath = r["ImagePath"].ToString() ?? string.Empty
                    });
                }
            }

            return model;
        }

        public async Task<int> CreateProductAsync(VendorProductFormViewModel model, int vendorId)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Products", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "INSERT";
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
            cmd.Parameters.Add("@ProductName", SqlDbType.VarChar, 200).Value = model.ProductName;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = (object?)model.Description ?? DBNull.Value;
            cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value = (object?)model.CategoryId ?? DBNull.Value;
            cmd.Parameters.Add("@SubCategoryId", SqlDbType.Int).Value = (object?)model.SubCategoryId ?? DBNull.Value;
            cmd.Parameters.Add("@BrandId", SqlDbType.Int).Value = (object?)model.BrandId ?? DBNull.Value;
            cmd.Parameters.Add("@UnitId", SqlDbType.Int).Value = (object?)model.UnitId ?? DBNull.Value;
            cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = (object?)model.Price ?? DBNull.Value;
            cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = (object?)model.Quantity ?? 0;
            cmd.Parameters.Add("@LowStockThreshold", SqlDbType.Int).Value = model.LowStockThreshold;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await con.OpenAsync();
            object? result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> UpdateProductAsync(VendorProductFormViewModel model, int vendorId)
        {
            if (!model.ProductId.HasValue) return false;

            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Products", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "UPDATE";
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
            cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = model.ProductId.Value;
            cmd.Parameters.Add("@ProductName", SqlDbType.VarChar, 200).Value = model.ProductName;
            cmd.Parameters.Add("@Description", SqlDbType.NVarChar, -1).Value = (object?)model.Description ?? DBNull.Value;
            cmd.Parameters.Add("@CategoryId", SqlDbType.Int).Value = (object?)model.CategoryId ?? DBNull.Value;
            cmd.Parameters.Add("@SubCategoryId", SqlDbType.Int).Value = (object?)model.SubCategoryId ?? DBNull.Value;
            cmd.Parameters.Add("@BrandId", SqlDbType.Int).Value = (object?)model.BrandId ?? DBNull.Value;
            cmd.Parameters.Add("@UnitId", SqlDbType.Int).Value = (object?)model.UnitId ?? DBNull.Value;
            cmd.Parameters.Add("@Price", SqlDbType.Decimal).Value = (object?)model.Price ?? DBNull.Value;
            cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = (object?)model.Quantity ?? 0;
            cmd.Parameters.Add("@LowStockThreshold", SqlDbType.Int).Value = model.LowStockThreshold;
            cmd.Parameters.Add("@IsActive", SqlDbType.Bit).Value = model.IsActive;

            await con.OpenAsync();
            object? rows = await cmd.ExecuteScalarAsync();
            return rows != null && Convert.ToInt32(rows) > 0;
        }

        public async Task<bool> ToggleProductStatusAsync(int productId, int vendorId)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Products", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "TOGGLE_STATUS";
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
            cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;

            await con.OpenAsync();
            object? result = await cmd.ExecuteScalarAsync();
            return result != null;
        }

        public async Task<int> RestockProductAsync(int productId, int vendorId, int quantity)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Products", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "RESTOCK";
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
            cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
            cmd.Parameters.Add("@Quantity", SqlDbType.Int).Value = quantity;

            await con.OpenAsync();
            object? result = await cmd.ExecuteScalarAsync();
            return result != null ? Convert.ToInt32(result) : 0;
        }

        public async Task<bool> SaveProductImagesAsync(int productId, List<string> imagePaths)
        {
            if (imagePaths == null || imagePaths.Count == 0) return true;

            using SqlConnection con = GetConnection();
            await con.OpenAsync();

            foreach (var path in imagePaths)
            {
                using SqlCommand cmd = new SqlCommand("INSERT INTO dbo.ProductImages (ProductId, ImagePath) VALUES (@ProductId, @ImagePath)", con);
                cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
                cmd.Parameters.Add("@ImagePath", SqlDbType.VarChar, 500).Value = path;
                await cmd.ExecuteNonQueryAsync();
            }

            return true;
        }

        public async Task<bool> DeleteProductImageAsync(int imageId, int productId, int vendorId)
        {
            using SqlConnection con = GetConnection();
            // Verify product belongs to vendor before deleting image
            string sql = @"
                DELETE PI 
                FROM dbo.ProductImages PI
                INNER JOIN dbo.Products P ON P.ProductId = PI.ProductId
                WHERE PI.ImageId = @ImageId AND P.ProductId = @ProductId AND P.VendorId = @VendorId";

            using SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.Add("@ImageId", SqlDbType.Int).Value = imageId;
            cmd.Parameters.Add("@ProductId", SqlDbType.Int).Value = productId;
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;

            await con.OpenAsync();
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        // =========================================================================
        // 4. ORDERS & FULFILLMENT
        // =========================================================================

        public async Task<List<VendorOrderSummaryViewModel>> GetVendorOrdersAsync(int vendorId, string? statusFilter = null)
        {
            var list = new List<VendorOrderSummaryViewModel>();

            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "GET_ORDERS";
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
            cmd.Parameters.Add("@StatusFilter", SqlDbType.VarChar, 30).Value = (object?)statusFilter ?? DBNull.Value;

            await con.OpenAsync();
            using SqlDataReader r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
            {
                list.Add(new VendorOrderSummaryViewModel
                {
                    OrderId = Convert.ToInt32(r["OrderId"]),
                    OrderNumber = r["OrderNumber"].ToString() ?? string.Empty,
                    OrderDate = Convert.ToDateTime(r["OrderDate"]),
                    OrderStatus = r["OrderStatus"].ToString() ?? string.Empty,
                    PaymentMethod = r["PaymentMethod"].ToString() ?? string.Empty,
                    CustomerName = r["CustomerName"].ToString() ?? string.Empty,
                    City = r["City"] != DBNull.Value ? r["City"].ToString() : null,
                    State = r["State"] != DBNull.Value ? r["State"].ToString() : null,
                    VendorItemCount = Convert.ToInt32(r["VendorItemCount"]),
                    VendorTotal = Convert.ToDecimal(r["VendorTotal"]),
                    ItemStatusSummary = r["ItemStatusSummary"] != DBNull.Value ? r["ItemStatusSummary"].ToString()! : "Pending"
                });
            }

            return list;
        }

        public async Task<VendorOrderDetailsViewModel?> GetVendorOrderDetailsAsync(int orderId, int vendorId)
        {
            using SqlConnection con = GetConnection();
            await con.OpenAsync();

            VendorOrderDetailsViewModel? vm = null;

            // 1. Order Header
            using (SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Orders", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "GET_ORDER_HEADER";
                cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

                using SqlDataReader r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    vm = new VendorOrderDetailsViewModel
                    {
                        OrderId = Convert.ToInt32(r["OrderId"]),
                        OrderNumber = r["OrderNumber"].ToString() ?? string.Empty,
                        OrderDate = Convert.ToDateTime(r["OrderDate"]),
                        OrderStatus = r["OrderStatus"].ToString() ?? string.Empty,
                        PaymentMethod = r["PaymentMethod"].ToString() ?? string.Empty,
                        CarrierName = r["CarrierName"] != DBNull.Value ? r["CarrierName"].ToString() : null,
                        TrackingNumber = r["TrackingNumber"] != DBNull.Value ? r["TrackingNumber"].ToString() : null,
                        CustomerName = r["CustomerName"].ToString() ?? string.Empty,
                        AddressLine1 = r["AddressLine1"] != DBNull.Value ? r["AddressLine1"].ToString() : null,
                        City = r["City"] != DBNull.Value ? r["City"].ToString() : null,
                        State = r["State"] != DBNull.Value ? r["State"].ToString() : null,
                        Pincode = r["Pincode"] != DBNull.Value ? r["Pincode"].ToString() : null,
                        CustomerPhoneMasked = r["CustomerPhoneMasked"] != DBNull.Value ? r["CustomerPhoneMasked"].ToString() : "******"
                    };
                }
            }

            if (vm == null) return null;

            // 2. Vendor Items
            using (SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Orders", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "GET_ORDER_ITEMS";
                cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;

                using SqlDataReader r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.Items.Add(new VendorOrderItemViewModel
                    {
                        OrderDetailId = Convert.ToInt32(r["OrderDetailId"]),
                        OrderId = Convert.ToInt32(r["OrderId"]),
                        ProductId = Convert.ToInt32(r["ProductId"]),
                        ProductName = r["ProductName"].ToString() ?? string.Empty,
                        UnitPrice = Convert.ToDecimal(r["UnitPrice"]),
                        Quantity = Convert.ToInt32(r["Quantity"]),
                        Total = Convert.ToDecimal(r["Total"]),
                        ItemStatus = r["ItemStatus"].ToString() ?? "Pending",
                        ImagePath = r["ImagePath"] != DBNull.Value ? r["ImagePath"].ToString() : null
                    });
                }
            }

            // 3. Commission Rate
            var vendor = await GetVendorByIdAsync(vendorId);
            if (vendor != null)
            {
                vm.CommissionRate = vendor.CommissionRate;
            }

            return vm;
        }

        public async Task<bool> UpdateItemStatusAsync(int orderDetailId, int vendorId, string newStatus)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "UPDATE_ITEM_STATUS";
            cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
            cmd.Parameters.Add("@OrderDetailId", SqlDbType.Int).Value = orderDetailId;
            cmd.Parameters.Add("@ItemStatus", SqlDbType.VarChar, 30).Value = newStatus;

            await con.OpenAsync();
            object? result = await cmd.ExecuteScalarAsync();
            return result != null && Convert.ToInt32(result) > 0;
        }

        // =========================================================================
        // 5. SALES REPORTS & ANALYTICS
        // =========================================================================

        public async Task<VendorReportsViewModel> GetVendorReportsAsync(int vendorId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var vm = new VendorReportsViewModel
            {
                StartDate = startDate ?? DateTime.Today.AddDays(-30),
                EndDate = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1)
            };

            using SqlConnection con = GetConnection();
            await con.OpenAsync();

            // 1. Sales Summary
            using (SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Reports", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "SALES_SUMMARY";
                cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = vm.StartDate;
                cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = vm.EndDate;

                using SqlDataReader r = await cmd.ExecuteReaderAsync();
                if (await r.ReadAsync())
                {
                    vm.GrossSales = Convert.ToDecimal(r["GrossSales"]);
                    vm.OrderCount = Convert.ToInt32(r["OrderCount"]);
                    vm.UnitsSold = Convert.ToInt32(r["UnitsSold"]);
                    vm.PlatformCommission = Convert.ToDecimal(r["PlatformCommission"]);
                    vm.NetPayout = Convert.ToDecimal(r["NetPayout"]);
                    vm.CommissionRate = Convert.ToDecimal(r["CommissionRate"]);
                }
            }

            // 2. Top Products
            using (SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Reports", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "TOP_PRODUCTS";
                cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;
                cmd.Parameters.Add("@StartDate", SqlDbType.DateTime).Value = vm.StartDate;
                cmd.Parameters.Add("@EndDate", SqlDbType.DateTime).Value = vm.EndDate;

                using SqlDataReader r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.TopProducts.Add(new VendorTopProductViewModel
                    {
                        ProductId = Convert.ToInt32(r["ProductId"]),
                        ProductName = r["ProductName"].ToString() ?? string.Empty,
                        UnitsSold = Convert.ToInt32(r["UnitsSold"]),
                        TotalRevenue = Convert.ToDecimal(r["TotalRevenue"]),
                        ImagePath = r["ImagePath"] != DBNull.Value ? r["ImagePath"].ToString() : null
                    });
                }
            }

            // 3. Daily Sales Curve
            using (SqlCommand cmd = new SqlCommand("dbo.SP_Vendor_Reports", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 50).Value = "DAILY_SALES";
                cmd.Parameters.Add("@VendorId", SqlDbType.Int).Value = vendorId;

                using SqlDataReader r = await cmd.ExecuteReaderAsync();
                while (await r.ReadAsync())
                {
                    vm.DailySales.Add(new VendorDailySalesViewModel
                    {
                        SaleDate = Convert.ToDateTime(r["SaleDate"]),
                        DailyOrders = Convert.ToInt32(r["DailyOrders"]),
                        DailyUnits = Convert.ToInt32(r["DailyUnits"]),
                        DailyRevenue = Convert.ToDecimal(r["DailyRevenue"])
                    });
                }
            }

            return vm;
        }
    }
}
