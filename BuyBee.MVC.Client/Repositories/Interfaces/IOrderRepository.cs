using EMart.Models;
using EMart.ViewModels;

namespace EMart.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<int> PlaceOrder(
            int customerId,
            int addressId,
            string paymentMethod,
            string? couponCode = null,
            decimal discountAmount = 0,
            int coinsRedeemed = 0,
            decimal coinsDiscount = 0,
            int coinsEarned = 0);

        Task<Orders?> GetById(
            int orderId,
            int customerId);

        Task<List<Orders>> GetByCustomerId(
            int customerId);

        Task<List<OrderDetails>> GetOrderDetails(int orderId, int customerId);

        // =========================================
        // ADMIN ORDER MANAGEMENT
        // =========================================

        Task<Orders?> GetByAdminId(
            int orderId);

        Task<List<OrderDetails>> GetOrderDetailsByAdmin(
            int orderId);

        Task<List<Orders>> GetAllOrders();

        Task<bool> UpdateOrderStatus(
            int orderId,
            string orderStatus);

        Task<bool> UpdateShipmentDetails(
            int orderId,
            string orderStatus,
            string? carrierName = null,
            string? trackingNumber = null);

        Task<bool> AssignDeliveryPartner(
            int orderId,
            int deliveryPartnerId,
            string? notes = null);

        Task<List<DriverOptionVM>> GetActiveDeliveryPartners();
    }
}