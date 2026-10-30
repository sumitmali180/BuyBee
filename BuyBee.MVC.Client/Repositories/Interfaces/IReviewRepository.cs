using EMart.Models;
using EMart.ViewModels;

namespace EMart.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<List<ProductReviewItemVM>> GetReviewsByProductId(int productId);
        Task<ProductRatingSummaryVM> GetRatingSummary(int productId);
        Task<bool> AddReview(ProductReview review);
        Task<bool> HasCustomerPurchased(int customerId, int productId);
        Task<List<ProductReview>> GetAllReviewsForAdmin();
        Task<bool> ToggleApproval(int reviewId);
        Task<bool> DeleteReview(int reviewId);
    }
}
