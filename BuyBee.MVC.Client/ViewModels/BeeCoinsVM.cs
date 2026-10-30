using EMart.Models;

namespace EMart.ViewModels
{
    public class BeeCoinsDashboardVM
    {
        public CustomerWallet Wallet { get; set; } = new();
        public List<BeeCoinTransaction> Transactions { get; set; } = new();
        public decimal LifetimeSavings => Wallet.TotalRedeemed; // 1 Coin = 1 INR
        public int TotalOrdersWithCoins { get; set; }
    }

    public class RedeemCoinsRequestVM
    {
        public int Coins { get; set; }
    }

    public class BeeCoinsBalanceResponseVM
    {
        public int Balance { get; set; }
        public decimal ValueInRupees { get; set; }
        public string Tier { get; set; } = string.Empty;
        public string TierBadgeClass { get; set; } = string.Empty;
    }
}
