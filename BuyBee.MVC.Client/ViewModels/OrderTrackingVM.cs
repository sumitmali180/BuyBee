using EMart.Models;

namespace EMart.ViewModels
{
    public class MilestoneCheckpoint
    {
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public bool IsCompleted { get; set; }
        public bool IsCurrent { get; set; }
        public string Icon { get; set; } = "bi-check-circle";
    }

    public class DeliveryAgentInfo
    {
        public string Name { get; set; } = "Rajesh Kumar";
        public string Phone { get; set; } = "+91 98201 44820";
        public decimal Rating { get; set; } = 4.9m;
        public string TotalDeliveries { get; set; } = "1,420+";
        public string VehicleNumber { get; set; } = "MH-02-EA-9421";
        public string VehicleType { get; set; } = "Buybee EV Cargo Van";
        public string AvatarUrl { get; set; } = "/images/delivery-agent.png";
    }

    public class OrderTrackingVM
    {
        public Orders Order { get; set; } = new();
        public List<OrderDetails> OrderDetails { get; set; } = new();
        public Addresses? Address { get; set; }
        public int CurrentMilestoneStep { get; set; } // 1 to 6
        public string StatusName { get; set; } = "Pending";
        public bool IsCancelled { get; set; }
        public string DeliveryOtp { get; set; } = string.Empty;
        public string CarrierName { get; set; } = "Buybee Express";
        public string TrackingNumber { get; set; } = string.Empty;
        public DateTime EstimatedDeliveryDate { get; set; }
        public string EstimatedTimeWindow { get; set; } = "By 2:00 PM - 5:00 PM";
        public string DistanceRemaining { get; set; } = "1.8 km away";
        public DeliveryAgentInfo DeliveryAgent { get; set; } = new();
        public List<MilestoneCheckpoint> Checkpoints { get; set; } = new();
    }
}
