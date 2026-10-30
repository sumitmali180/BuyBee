using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface ICartRepository
    {
        Task<bool> AddToCart(int customerId, int productId, int quantity);

        Task<List<CartItem>> GetCart(int customerId);

        Task<List<CartItem>> GetMiniCart(int customerId);

        Task<bool> UpdateQuantity(int cartId, int quantity);

        Task<bool> Remove(int cartId);

        Task<int> GetCartCount(int customerId);

        Task<bool> ClearCart(int customerId);
    }
}