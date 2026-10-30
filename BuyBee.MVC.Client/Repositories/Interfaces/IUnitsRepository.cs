using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface IUnitsRepository
    {
        List<Units> GetAll();

        Units GetById(int id);

        void Insert(Units unit);

        void Update(Units unit);

        void Delete(int id);
    }
}
    