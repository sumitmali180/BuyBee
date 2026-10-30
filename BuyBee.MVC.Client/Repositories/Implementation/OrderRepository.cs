using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IConfiguration _configuration;

        public OrderRepository(
            IConfiguration configuration)
        {
            _configuration = configuration;
        }


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


        #region Place Order

        public async Task<int> PlaceOrder(
            int customerId,
            int addressId,
            string paymentMethod,
            string? couponCode = null,
            decimal discountAmount = 0,
            int coinsRedeemed = 0,
            decimal coinsDiscount = 0,
            int coinsEarned = 0)
        {
            using SqlConnection con =
                GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Orders",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar, 30).Value =
                "PLACEORDER";


            cmd.Parameters.Add(
                "@CustomerId",
                SqlDbType.Int).Value =
                customerId;


            cmd.Parameters.Add(
                "@AddressId",
                SqlDbType.Int).Value =
                addressId;


            cmd.Parameters.Add(
                "@PaymentMethod",
                SqlDbType.VarChar, 30).Value =
                paymentMethod;


            cmd.Parameters.Add(
                "@CouponCode",
                SqlDbType.VarChar, 50).Value =
                (object?)couponCode ?? DBNull.Value;


            cmd.Parameters.Add(
                "@DiscountAmountParam",
                SqlDbType.Decimal).Value =
                discountAmount;

            cmd.Parameters.Add(
                "@CoinsRedeemed",
                SqlDbType.Int).Value =
                coinsRedeemed;

            cmd.Parameters.Add(
                "@CoinsDiscount",
                SqlDbType.Decimal).Value =
                coinsDiscount;

            cmd.Parameters.Add(
                "@CoinsEarned",
                SqlDbType.Int).Value =
                coinsEarned;


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


        #region Get By Id

        public async Task<Orders?> GetById(
            int orderId,
            int customerId)
        {
            Orders? order = null;


            using SqlConnection con =
                GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Orders",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar, 30).Value =
                "GETBYID";


            cmd.Parameters.Add(
                "@OrderId",
                SqlDbType.Int).Value =
                orderId;


            cmd.Parameters.Add(
                "@CustomerId",
                SqlDbType.Int).Value =
                customerId;


            await con.OpenAsync();


            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();


            if (await reader.ReadAsync())
            {
                order = MapOrder(reader);
            }


            return order;
        }

        #endregion


        #region Get By Customer

        public async Task<List<Orders>> GetByCustomerId(
            int customerId)
        {
            List<Orders> orders = new();


            using SqlConnection con =
                GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Orders",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar, 30).Value =
                "GETBYCUSTOMER";


            cmd.Parameters.Add(
                "@CustomerId",
                SqlDbType.Int).Value =
                customerId;


            await con.OpenAsync();


            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();


            while (await reader.ReadAsync())
            {
                orders.Add(
                    MapOrder(reader));
            }


            return orders;
        }

        #endregion


        #region Get Order Details

        public async Task<List<OrderDetails>> GetOrderDetails(
            int orderId,
            int customerId)
        {
            List<OrderDetails> details = new();


            using SqlConnection con =
                GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Orders",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar, 30).Value =
                "GETDETAILS";


            cmd.Parameters.Add(
                "@OrderId",
                SqlDbType.Int).Value =
                orderId;


            cmd.Parameters.Add(
                "@CustomerId",
                SqlDbType.Int).Value =
                customerId;


            await con.OpenAsync();


            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();


            while (await reader.ReadAsync())
            {
                details.Add(
                    new OrderDetails
                    {
                        OrderDetailId =
                            Convert.ToInt32(
                                reader["OrderDetailId"]),

                        OrderId =
                            Convert.ToInt32(
                                reader["OrderId"]),

                        ProductId =
                            Convert.ToInt32(
                                reader["ProductId"]),

                        ProductName =
                            reader["ProductName"]
                                ?.ToString() ?? "",

                        UnitPrice =
                            Convert.ToDecimal(
                                reader["UnitPrice"]),

                        Quantity =
                            Convert.ToInt32(
                                reader["Quantity"]),

                        Total =
                            Convert.ToDecimal(
                                reader["Total"])
                    });
            }


            return details;
        }

        #endregion


        #region Map Order

        private Orders MapOrder(
            SqlDataReader reader)
        {
            return new Orders
            {
                OrderId =
                    Convert.ToInt32(
                        reader["OrderId"]),

                OrderNumber =
                    reader["OrderNumber"]
                        ?.ToString() ?? "",

                CustomerId =
                    Convert.ToInt32(
                        reader["CustomerId"]),

                AddressId =
                    Convert.ToInt32(
                        reader["AddressId"]),

                OrderDate =
                    Convert.ToDateTime(
                        reader["OrderDate"]),

                SubTotal =
                    Convert.ToDecimal(
                        reader["SubTotal"]),

                ShippingAmount =
                    Convert.ToDecimal(
                        reader["ShippingAmount"]),

                DiscountAmount =
                    Convert.ToDecimal(
                        reader["DiscountAmount"]),

                GrandTotal =
                    Convert.ToDecimal(
                        reader["GrandTotal"]),

                OrderStatus =
                    reader["OrderStatus"]
                        ?.ToString() ?? "",

                PaymentMethod =
                    reader["PaymentMethod"]
                        ?.ToString() ?? "",

                CouponCode =
                    reader["CouponCode"] != DBNull.Value
                        ? reader["CouponCode"]?.ToString()
                        : null,

                CoinsRedeemed = HasColumn(reader, "CoinsRedeemed") && reader["CoinsRedeemed"] != DBNull.Value
                    ? Convert.ToInt32(reader["CoinsRedeemed"]) : 0,

                CoinsDiscount = HasColumn(reader, "CoinsDiscount") && reader["CoinsDiscount"] != DBNull.Value
                    ? Convert.ToDecimal(reader["CoinsDiscount"]) : 0,

                CoinsEarned = HasColumn(reader, "CoinsEarned") && reader["CoinsEarned"] != DBNull.Value
                    ? Convert.ToInt32(reader["CoinsEarned"]) : 0,

                CarrierName = HasColumn(reader, "CarrierName") && reader["CarrierName"] != DBNull.Value
                    ? reader["CarrierName"]?.ToString() : "Buybee Express (Surface)",

                TrackingNumber = HasColumn(reader, "TrackingNumber") && reader["TrackingNumber"] != DBNull.Value
                    ? reader["TrackingNumber"]?.ToString() : null,

                DeliveryOtp = HasColumn(reader, "DeliveryOtp") && reader["DeliveryOtp"] != DBNull.Value
                    ? reader["DeliveryOtp"]?.ToString() : null,

                DeliveryPartnerId = HasColumn(reader, "DeliveryPartnerId") && reader["DeliveryPartnerId"] != DBNull.Value
                    ? Convert.ToInt32(reader["DeliveryPartnerId"]) : null,

                DeliveryPartnerName = HasColumn(reader, "DeliveryPartnerName") && reader["DeliveryPartnerName"] != DBNull.Value
                    ? reader["DeliveryPartnerName"]?.ToString() : null,

                AssignedAt = HasColumn(reader, "AssignedAt") && reader["AssignedAt"] != DBNull.Value
                    ? Convert.ToDateTime(reader["AssignedAt"]) : null,

                DeliveredAt = HasColumn(reader, "DeliveredAt") && reader["DeliveredAt"] != DBNull.Value
                    ? Convert.ToDateTime(reader["DeliveredAt"]) : null,

                DeliveryAttempts = HasColumn(reader, "DeliveryAttempts") && reader["DeliveryAttempts"] != DBNull.Value
                    ? Convert.ToInt32(reader["DeliveryAttempts"]) : 0,

                DeliveryNotes = HasColumn(reader, "DeliveryNotes") && reader["DeliveryNotes"] != DBNull.Value
                    ? reader["DeliveryNotes"]?.ToString() : null,

                FailedReason = HasColumn(reader, "FailedReason") && reader["FailedReason"] != DBNull.Value
                    ? reader["FailedReason"]?.ToString() : null,

                CreatedOn =
                    Convert.ToDateTime(
                        reader["CreatedOn"])
            };
        }

        #endregion

        #region Admin - Get All Orders

        public async Task<List<Orders>> GetAllOrders()
        {
            List<Orders> orders = new();

            using SqlConnection con = GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Orders",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar,
                30).Value =
                "ADMIN_GETALL";


            await con.OpenAsync();


            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();


            while (await reader.ReadAsync())
            {
                orders.Add(
                    MapOrder(reader));
            }


            return orders;
        }

        #endregion

        #region Admin - Update Order Status

        public async Task<bool> UpdateOrderStatus(
            int orderId,
            string orderStatus)
        {
            using SqlConnection con =
                GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Orders",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar,
                30).Value =
                "ADMIN_UPDATESTATUS";


            cmd.Parameters.Add(
                "@OrderId",
                SqlDbType.Int).Value =
                orderId;


            cmd.Parameters.Add(
                "@OrderStatus",
                SqlDbType.VarChar,
                30).Value =
                orderStatus;


            await con.OpenAsync();


            int rows =
                await cmd.ExecuteNonQueryAsync();


            return rows > 0;
        }

        #endregion

        #region Admin - Get By Id

        public async Task<Orders?> GetByAdminId(
            int orderId)
        {
            Orders? order = null;

            using SqlConnection con =
                GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Orders",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar,
                30).Value =
                "ADMIN_GETBYID";


            cmd.Parameters.Add(
                "@OrderId",
                SqlDbType.Int).Value =
                orderId;


            await con.OpenAsync();


            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();


            if (await reader.ReadAsync())
            {
                order =
                    MapOrder(reader);
            }


            return order;
        }

        #endregion

        #region Admin - Get Order Details

        public async Task<List<OrderDetails>>
            GetOrderDetailsByAdmin(
                int orderId)
        {
            List<OrderDetails> details = new();

            using SqlConnection con =
                GetConnection();

            using SqlCommand cmd =
                new SqlCommand(
                    "SP_Orders",
                    con);

            cmd.CommandType =
                CommandType.StoredProcedure;


            cmd.Parameters.Add(
                "@Flag",
                SqlDbType.VarChar,
                30).Value =
                "ADMIN_GETDETAILS";


            cmd.Parameters.Add(
                "@OrderId",
                SqlDbType.Int).Value =
                orderId;


            await con.OpenAsync();


            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();


            while (await reader.ReadAsync())
            {
                details.Add(
                    new OrderDetails
                    {
                        OrderDetailId =
                            Convert.ToInt32(
                                reader["OrderDetailId"]),

                        OrderId =
                            Convert.ToInt32(
                                reader["OrderId"]),

                        ProductId =
                            Convert.ToInt32(
                                reader["ProductId"]),

                        ProductName =
                            reader["ProductName"]
                                ?.ToString() ?? "",

                        UnitPrice =
                            Convert.ToDecimal(
                                reader["UnitPrice"]),

                        Quantity =
                            Convert.ToInt32(
                                reader["Quantity"]),

                        Total =
                            Convert.ToDecimal(
                                reader["Total"]),
                        VendorId = HasColumn(reader, "VendorId") && reader["VendorId"] != DBNull.Value ? Convert.ToInt32(reader["VendorId"]) : null,
                        ItemStatus = HasColumn(reader, "ItemStatus") && reader["ItemStatus"] != DBNull.Value ? reader["ItemStatus"].ToString()! : "Pending",
                        ImagePath = HasColumn(reader, "ImagePath") && reader["ImagePath"] != DBNull.Value ? reader["ImagePath"].ToString() : null,
                        VendorStoreName = HasColumn(reader, "VendorStoreName") && reader["VendorStoreName"] != DBNull.Value ? reader["VendorStoreName"].ToString() : null
                    });
            }


            return details;
        }

        #endregion

        #region Admin - Update Shipment Details

        public async Task<bool> UpdateShipmentDetails(
            int orderId,
            string orderStatus,
            string? carrierName = null,
            string? trackingNumber = null)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SP_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "ADMIN_UPDATESTATUS";
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@OrderStatus", SqlDbType.VarChar, 30).Value = orderStatus;
            cmd.Parameters.Add("@CarrierName", SqlDbType.VarChar, 100).Value = (object?)carrierName ?? DBNull.Value;
            cmd.Parameters.Add("@TrackingNumber", SqlDbType.VarChar, 100).Value = (object?)trackingNumber ?? DBNull.Value;

            await con.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        #endregion

        #region Admin - Assign Delivery Partner

        public async Task<bool> AssignDeliveryPartner(
            int orderId,
            int deliveryPartnerId,
            string? notes = null)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SP_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "ADMIN_ASSIGN_DELIVERY";
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@DeliveryPartnerId", SqlDbType.Int).Value = deliveryPartnerId;
            cmd.Parameters.Add("@DeliveryNotes", SqlDbType.VarChar, 500).Value = (object?)notes ?? DBNull.Value;

            await con.OpenAsync();
            object? scalar = await cmd.ExecuteScalarAsync();
            if (scalar != null && int.TryParse(scalar.ToString(), out int affected) && affected > 0)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Admin - Get Active Delivery Partners

        public async Task<List<DriverOptionVM>> GetActiveDeliveryPartners()
        {
            List<DriverOptionVM> drivers = new();

            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SP_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "ADMIN_GET_DRIVERS";

            await con.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                drivers.Add(new DriverOptionVM
                {
                    UserId = Convert.ToInt32(reader["UserId"]),
                    FullName = reader["FullName"]?.ToString() ?? "",
                    Email = reader["Email"]?.ToString() ?? "",
                    IsActive = reader["IsActive"] != DBNull.Value && Convert.ToBoolean(reader["IsActive"])
                });
            }

            return drivers;
        }

        #endregion

        #region Helper - Has Column

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        #endregion
    }
}