using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface ICustomers
    {
        List<Customers> GetAll();
        Customers GetById(int id);
        void Insert(Customers model);
        void Update(Customers model);
        void Delete(int id);
    }
}
