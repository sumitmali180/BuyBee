namespace EMart.Models
{
    public class Payments
    {
       public int? PaymentId { get; set; }
       public int? OrderId { get; set; }
       public string? PaymentMode { get; set; }
       public double? Amount { get; set; }
       public string? TransactionId  { get; set; }
    }
}
