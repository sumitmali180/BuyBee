using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.Data.SqlClient;
using System.Data;

namespace EMart.Repositories.Implementation
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly IConfiguration _configuration;

        public DeliveryRepository(IConfiguration configuration)
        {
            _configuration = configuration;
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

        public async Task<DeliveryShiftMetrics> GetShiftMetrics(int deliveryPartnerId)
        {
            var metrics = new DeliveryShiftMetrics();

            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SP_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "DELIVERY_GETMETRICS";
            cmd.Parameters.Add("@DeliveryPartnerId", SqlDbType.Int).Value = deliveryPartnerId;

            await con.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                metrics.TotalAssigned = HasColumn(reader, "TotalAssigned") && reader["TotalAssigned"] != DBNull.Value
                    ? Convert.ToInt32(reader["TotalAssigned"]) : 0;
                metrics.TotalOutForDelivery = HasColumn(reader, "TotalOutForDelivery") && reader["TotalOutForDelivery"] != DBNull.Value
                    ? Convert.ToInt32(reader["TotalOutForDelivery"]) : 0;
                metrics.TotalDeliveredToday = HasColumn(reader, "TotalDeliveredToday") && reader["TotalDeliveredToday"] != DBNull.Value
                    ? Convert.ToInt32(reader["TotalDeliveredToday"]) : 0;
                metrics.TotalFailed = HasColumn(reader, "TotalFailed") && reader["TotalFailed"] != DBNull.Value
                    ? Convert.ToInt32(reader["TotalFailed"]) : 0;
                metrics.CashToCollect = HasColumn(reader, "CashToCollect") && reader["CashToCollect"] != DBNull.Value
                    ? Convert.ToDecimal(reader["CashToCollect"]) : 0;
                metrics.CashCollectedToday = HasColumn(reader, "CashCollectedToday") && reader["CashCollectedToday"] != DBNull.Value
                    ? Convert.ToDecimal(reader["CashCollectedToday"]) : 0;
            }

            return metrics;
        }

        public async Task<List<DeliveryOrderSummaryVM>> GetAssignedOrders(int deliveryPartnerId, string? status = null)
        {
            var orders = new List<DeliveryOrderSummaryVM>();

            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SP_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "DELIVERY_GETASSIGNED";
            cmd.Parameters.Add("@DeliveryPartnerId", SqlDbType.Int).Value = deliveryPartnerId;
            cmd.Parameters.Add("@OrderStatus", SqlDbType.VarChar, 30).Value = (object?)status ?? DBNull.Value;

            await con.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                orders.Add(MapSummary(reader));
            }

            return orders;
        }

        public async Task<DeliveryOrderDetailsVM?> GetDeliveryOrderDetails(int orderId, int deliveryPartnerId)
        {
            DeliveryOrderSummaryVM? summary = null;

            using SqlConnection con = GetConnection();
            await con.OpenAsync();

            // 1. Fetch Order Header
            using (SqlCommand cmd = new SqlCommand("SP_Orders", con))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "DELIVERY_GETDETAILS";
                cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                cmd.Parameters.Add("@DeliveryPartnerId", SqlDbType.Int).Value = deliveryPartnerId;

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    summary = MapSummary(reader);
                }
            }

            if (summary == null)
            {
                return null;
            }

            var result = new DeliveryOrderDetailsVM
            {
                Order = summary,
                Items = new List<DeliveryOrderItemVM>()
            };

            // 2. Fetch Order Items
            using (SqlCommand cmdItems = new SqlCommand("SP_Orders", con))
            {
                cmdItems.CommandType = CommandType.StoredProcedure;
                cmdItems.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "DELIVERY_GETDETAILS_ITEMS";
                cmdItems.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
                cmdItems.Parameters.Add("@DeliveryPartnerId", SqlDbType.Int).Value = deliveryPartnerId;

                using SqlDataReader itemReader = await cmdItems.ExecuteReaderAsync();
                while (await itemReader.ReadAsync())
                {
                    result.Items.Add(new DeliveryOrderItemVM
                    {
                        OrderDetailId = Convert.ToInt32(itemReader["OrderDetailId"]),
                        OrderId = Convert.ToInt32(itemReader["OrderId"]),
                        ProductId = Convert.ToInt32(itemReader["ProductId"]),
                        ProductName = itemReader["ProductName"]?.ToString() ?? "",
                        ImagePath = HasColumn(itemReader, "ImagePath") && itemReader["ImagePath"] != DBNull.Value
                            ? itemReader["ImagePath"]?.ToString() : null,
                        UnitPrice = Convert.ToDecimal(itemReader["UnitPrice"]),
                        Quantity = Convert.ToInt32(itemReader["Quantity"]),
                        Total = Convert.ToDecimal(itemReader["Total"])
                    });
                }
            }

            return result;
        }

        public async Task<bool> StartDelivery(int orderId, int deliveryPartnerId, string? notes = null)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SP_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "DELIVERY_UPDATESTATUS";
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@DeliveryPartnerId", SqlDbType.Int).Value = deliveryPartnerId;
            cmd.Parameters.Add("@OrderStatus", SqlDbType.VarChar, 30).Value = "Out for Delivery";
            cmd.Parameters.Add("@DeliveryNotes", SqlDbType.VarChar, 500).Value = (object?)notes ?? DBNull.Value;

            await con.OpenAsync();
            object? scalar = await cmd.ExecuteScalarAsync();
            if (scalar != null && int.TryParse(scalar.ToString(), out int affected) && affected > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<(bool success, string message)> VerifyOtpAndDeliver(int orderId, int deliveryPartnerId, string otp, string? notes = null)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SP_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "DELIVERY_VERIFY_OTP";
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@DeliveryPartnerId", SqlDbType.Int).Value = deliveryPartnerId;
            cmd.Parameters.Add("@EnteredOtp", SqlDbType.VarChar, 10).Value = otp;
            cmd.Parameters.Add("@DeliveryNotes", SqlDbType.VarChar, 500).Value = (object?)notes ?? DBNull.Value;

            await con.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool success = Convert.ToInt32(reader["Success"]) == 1;
                string message = reader["Message"]?.ToString() ?? "";
                return (success, message);
            }

            return (false, "Unable to complete delivery.");
        }

        public async Task<(bool success, string message)> ReportDeliveryFailure(int orderId, int deliveryPartnerId, string failedReason, string? notes = null)
        {
            using SqlConnection con = GetConnection();
            using SqlCommand cmd = new SqlCommand("SP_Orders", con);
            cmd.CommandType = CommandType.StoredProcedure;

            cmd.Parameters.Add("@Flag", SqlDbType.VarChar, 30).Value = "DELIVERY_REPORT_FAILURE";
            cmd.Parameters.Add("@OrderId", SqlDbType.Int).Value = orderId;
            cmd.Parameters.Add("@DeliveryPartnerId", SqlDbType.Int).Value = deliveryPartnerId;
            cmd.Parameters.Add("@FailedReason", SqlDbType.VarChar, 200).Value = failedReason;
            cmd.Parameters.Add("@DeliveryNotes", SqlDbType.VarChar, 500).Value = (object?)notes ?? DBNull.Value;

            await con.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                bool success = Convert.ToInt32(reader["Success"]) == 1;
                string message = reader["Message"]?.ToString() ?? "";
                return (success, message);
            }

            return (false, "Unable to report failure.");
        }

        private static DeliveryOrderSummaryVM MapSummary(SqlDataReader reader)
        {
            return new DeliveryOrderSummaryVM
            {
                OrderId = Convert.ToInt32(reader["OrderId"]),
                OrderNumber = reader["OrderNumber"]?.ToString() ?? "",
                OrderDate = Convert.ToDateTime(reader["OrderDate"]),
                GrandTotal = Convert.ToDecimal(reader["GrandTotal"]),
                OrderStatus = reader["OrderStatus"]?.ToString() ?? "",
                PaymentMethod = reader["PaymentMethod"]?.ToString() ?? "",
                PaymentStatus = HasColumn(reader, "PaymentStatus") && reader["PaymentStatus"] != DBNull.Value
                    ? reader["PaymentStatus"]?.ToString() ?? "Pending" : "Pending",

                CustomerId = Convert.ToInt32(reader["CustomerId"]),
                CustomerName = HasColumn(reader, "CustomerName") && reader["CustomerName"] != DBNull.Value
                    ? reader["CustomerName"]?.ToString() ?? "" : "",
                CustomerEmail = HasColumn(reader, "CustomerEmail") && reader["CustomerEmail"] != DBNull.Value
                    ? reader["CustomerEmail"]?.ToString() ?? "" : "",
                CustomerPhone = HasColumn(reader, "CustomerPhone") && reader["CustomerPhone"] != DBNull.Value
                    ? reader["CustomerPhone"]?.ToString() : null,

                AddressId = Convert.ToInt32(reader["AddressId"]),
                AddressLine1 = HasColumn(reader, "AddressLine1") && reader["AddressLine1"] != DBNull.Value
                    ? reader["AddressLine1"]?.ToString() ?? "" : "",
                City = HasColumn(reader, "City") && reader["City"] != DBNull.Value
                    ? reader["City"]?.ToString() ?? "" : "",
                State = HasColumn(reader, "State") && reader["State"] != DBNull.Value
                    ? reader["State"]?.ToString() ?? "" : "",
                PostalCode = HasColumn(reader, "PostalCode") && reader["PostalCode"] != DBNull.Value
                    ? reader["PostalCode"]?.ToString() ?? "" : "",

                DeliveryPartnerId = HasColumn(reader, "DeliveryPartnerId") && reader["DeliveryPartnerId"] != DBNull.Value
                    ? Convert.ToInt32(reader["DeliveryPartnerId"]) : null,
                AssignedAt = HasColumn(reader, "AssignedAt") && reader["AssignedAt"] != DBNull.Value
                    ? Convert.ToDateTime(reader["AssignedAt"]) : null,
                DeliveredAt = HasColumn(reader, "DeliveredAt") && reader["DeliveredAt"] != DBNull.Value
                    ? Convert.ToDateTime(reader["DeliveredAt"]) : null,
                DeliveryAttempts = HasColumn(reader, "DeliveryAttempts") && reader["DeliveryAttempts"] != DBNull.Value
                    ? Convert.ToInt32(reader["DeliveryAttempts"]) : 0,
                DeliveryNotes = HasColumn(reader, "DeliveryNotes") && reader["DeliveryNotes"] != DBNull.Value
                    ? reader["DeliveryNotes"]?.ToString() : null,
                FailedReason = HasColumn(reader, "FailedReason") && reader["FailedReason"] != DBNull.Value
                    ? reader["FailedReason"]?.ToString() : null
            };
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
    }
}
