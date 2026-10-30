using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface ICategoriesRepository
    {
        List<Categories> GetAll();
        Categories GetById(int id);
        void Insert(Categories model);
        void Update(Categories model);
        void Delete(int id);
    }
}
