using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface IAddressRepository
    {
        Task<List<Addresses>> GetByCustomerId(int customerId);

        Task<Addresses?> GetById(int addressId, int customerId);

        Task<int> Add(Addresses model);

        Task<bool> Update(Addresses model);

        Task<bool> Delete(int addressId, int customerId);
    }
}