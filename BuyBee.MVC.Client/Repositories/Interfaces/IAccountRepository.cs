using EMart.ViewModels;

namespace EMart.Repositories.Interfaces
{
    public interface IAccountRepository
    {
        Task<CustomerAccountVM?> Login(string email);

        Task<bool> EmailExists(string email);

        Task<int> RegisterCustomer(RegisterVM model, string passwordHash);

        Task<CustomerAccountVM?> GetCustomerByUserId(int userId);
    }
}