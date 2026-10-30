namespace EMart.Models
{
    public class CustomerWallet
    {
        public int WalletId { get; set; }
        public int CustomerId { get; set; }
        public int Balance { get; set; }
        public int TotalEarned { get; set; }
        public int TotalRedeemed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public decimal ValueInRupees => Balance; // 1 BeeCoin = 1 INR

        public string TierName
        {
            get
            {
                if (TotalEarned >= 5000) return "Platinum";
                if (TotalEarned >= 2000) return "Gold";
                if (TotalEarned >= 500) return "Silver";
                return "Bronze";
            }
        }

        public string NextTierName
        {
            get
            {
                if (TotalEarned >= 5000) return "Max Tier";
                if (TotalEarned >= 2000) return "Platinum";
                if (TotalEarned >= 500) return "Gold";
                return "Silver";
            }
        }

        public int CoinsToNextTier
        {
            get
            {
                if (TotalEarned >= 5000) return 0;
                if (TotalEarned >= 2000) return 5000 - TotalEarned;
                if (TotalEarned >= 500) return 2000 - TotalEarned;
                return 500 - TotalEarned;
            }
        }

        public int TierProgressPercentage
        {
            get
            {
                if (TotalEarned >= 5000) return 100;
                if (TotalEarned >= 2000) return Math.Min(100, (int)((TotalEarned - 2000) / 3000.0 * 100));
                if (TotalEarned >= 500) return Math.Min(100, (int)((TotalEarned - 500) / 1500.0 * 100));
                return Math.Min(100, (int)(TotalEarned / 500.0 * 100));
            }
        }

        public string TierBadgeClass
        {
            get
            {
                return TierName switch
                {
                    "Platinum" => "bg-secondary text-white",
                    "Gold" => "bg-warning text-dark",
                    "Silver" => "bg-light text-dark border",
                    _ => "bg-dark text-warning"
                };
            }
        }
    }
}
