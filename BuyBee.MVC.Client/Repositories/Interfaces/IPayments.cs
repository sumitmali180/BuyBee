using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface IPayments
    {
        List<Payments> GetAll();
        Payments GetById(int id);
        void Insert(Payments model);
        void Update(Payments model);
        void Delete(int id);
    }
}
