namespace EMart.Services
{
    public class PincodeDeliveryResult
    {
        public bool IsValid { get; set; }
        public string Pincode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string EstimatedDelivery { get; set; } = string.Empty;
        public string DeliveryTier { get; set; } = string.Empty; // "Express" or "Standard"
        public bool IsExpressAvailable { get; set; }
        public bool IsCodAvailable { get; set; } = true;
        public decimal ShippingFee { get; set; } = 0;
        public string Message { get; set; } = string.Empty;
    }

    public interface IPincodeService
    {
        PincodeDeliveryResult CheckDelivery(string? pincode);
    }
}
