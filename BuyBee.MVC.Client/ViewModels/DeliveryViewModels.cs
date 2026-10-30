using System.ComponentModel.DataAnnotations;

namespace EMart.ViewModels
{
    public class DeliveryDashboardVM
    {
        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;
        public string DriverEmail { get; set; } = string.Empty;

        // KPI Shift Counters
        public int TotalAssigned { get; set; }
        public int TotalOutForDelivery { get; set; }
        public int TotalDeliveredToday { get; set; }
        public int TotalFailed { get; set; }
        public decimal CashToCollect { get; set; }
        public decimal CashCollectedToday { get; set; }

        public List<DeliveryOrderSummaryVM> ActiveDeliveries { get; set; } = new();
        public List<DeliveryOrderSummaryVM> AssignedQueue { get; set; } = new();
    }

    public class DeliveryOrderSummaryVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal GrandTotal { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = "Pending";

        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }

        public int AddressId { get; set; }
        public string AddressLine1 { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;

        public int? DeliveryPartnerId { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public int DeliveryAttempts { get; set; }
        public string? DeliveryNotes { get; set; }
        public string? FailedReason { get; set; }

        public string FullAddress => $"{AddressLine1}, {City}, {State} - {PostalCode}";

        public string NavigationUrl =>
            $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(FullAddress)}";
    }

    public class DeliveryOrderDetailsVM
    {
        public DeliveryOrderSummaryVM Order { get; set; } = new();
        public List<DeliveryOrderItemVM> Items { get; set; } = new();
    }

    public class DeliveryOrderItemVM
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImagePath { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
    }

    public class DeliveryCompletionVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public decimal GrandTotal { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string PaymentStatus { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please enter the 4-digit OTP from the customer")]
        [StringLength(4, MinimumLength = 4, ErrorMessage = "OTP must be exactly 4 digits")]
        [RegularExpression(@"^\d{4}$", ErrorMessage = "OTP must contain 4 numbers")]
        public string? EnteredOtp { get; set; }

        public bool IsBypass { get; set; }
        public string? BypassReason { get; set; }
        public string? DeliveryNotes { get; set; }
    }

    public class DeliveryFailureVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public int CurrentAttempts { get; set; }

        [Required(ErrorMessage = "Please select a failure reason")]
        public string FailedReason { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? DeliveryNotes { get; set; }
    }

    public class AdminAssignDriverVM
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string OrderStatus { get; set; } = string.Empty;
        public int? CurrentDriverId { get; set; }
        public string? CurrentDriverName { get; set; }

        [Required(ErrorMessage = "Please select a delivery partner")]
        public int SelectedDriverId { get; set; }

        public string? DeliveryNotes { get; set; }

        public List<DriverOptionVM> AvailableDrivers { get; set; } = new();
    }

    public class DriverOptionVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }

    public class DeliveryShiftMetrics
    {
        public int TotalAssigned { get; set; }
        public int TotalOutForDelivery { get; set; }
        public int TotalDeliveredToday { get; set; }
        public int TotalFailed { get; set; }
        public decimal CashToCollect { get; set; }
        public decimal CashCollectedToday { get; set; }
    }
}
