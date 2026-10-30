namespace EMart.Models
{
    public class BeeCoinTransaction
    {
        public int TransactionId { get; set; }
        public int CustomerId { get; set; }
        public int? OrderId { get; set; }
        public string? OrderNumber { get; set; }
        public int Coins { get; set; }
        public string TransactionType { get; set; } = string.Empty; // 'WelcomeBonus', 'Earned', 'Redeemed', 'Refund'
        public string Description { get; set; } = string.Empty;
        public int BalanceAfter { get; set; }
        public DateTime CreatedOn { get; set; }

        public bool IsCredit => Coins > 0;
    }
}
