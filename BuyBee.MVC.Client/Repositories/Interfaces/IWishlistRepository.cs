using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface IWishlistRepository
    {
        Task<bool> AddToWishlist(int customerId, int productId);
        Task<bool> RemoveFromWishlist(int customerId, int productId);
        Task<bool> IsInWishlist(int customerId, int productId);
        Task<List<Products>> GetWishlist(int customerId);
        Task<int> GetWishlistCount(int customerId);
        Task<List<int>> GetWishlistProductIds(int customerId);
    }
}
