using EMart.ViewModels;

namespace EMart.Repositories.Interfaces
{
    public interface IDeliveryRepository
    {
        Task<DeliveryShiftMetrics> GetShiftMetrics(int deliveryPartnerId);

        Task<List<DeliveryOrderSummaryVM>> GetAssignedOrders(int deliveryPartnerId, string? status = null);

        Task<DeliveryOrderDetailsVM?> GetDeliveryOrderDetails(int orderId, int deliveryPartnerId);

        Task<bool> StartDelivery(int orderId, int deliveryPartnerId, string? notes = null);

        Task<(bool success, string message)> VerifyOtpAndDeliver(int orderId, int deliveryPartnerId, string otp, string? notes = null);

        Task<(bool success, string message)> ReportDeliveryFailure(int orderId, int deliveryPartnerId, string failedReason, string? notes = null);
    }
}
