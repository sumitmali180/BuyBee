using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface IBeeCoinsRepository
    {
        Task<CustomerWallet> GetWallet(int customerId);
        Task<List<BeeCoinTransaction>> GetTransactions(int customerId);
        Task<bool> AwardCoins(int customerId, int orderId, decimal orderAmount, string? description = null);
        Task<bool> RedeemCoins(int customerId, int orderId, int coins);
        Task<bool> CanRedeem(int customerId, int coins);
    }
}
