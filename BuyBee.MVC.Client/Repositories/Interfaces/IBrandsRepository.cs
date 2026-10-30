using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface IBrandsRepository
    {
        List<Brands> GetAll();
        Brands? GetById(int id);
        void Insert(Brands model);
        void Update(Brands model);
        void Delete(int id);
    }
}
