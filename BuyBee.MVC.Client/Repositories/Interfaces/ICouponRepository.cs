using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface ICouponRepository
    {
        Task<Coupon?> GetByCode(string code);
        Task<List<Coupon>> GetAll();
        Task<bool> Create(Coupon coupon);
        Task<bool> ToggleActive(int couponId);
        Task<bool> Delete(int couponId);
        Task<bool> IncrementUsage(string code);
        (bool IsValid, decimal DiscountAmount, string Message) ValidateCoupon(Coupon coupon, decimal cartSubtotal);
    }
}
