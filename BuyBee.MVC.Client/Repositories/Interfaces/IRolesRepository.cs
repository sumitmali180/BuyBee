using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface IRolesRepository
    {
        List<Roles> GetAll();
        Roles GetById(int id);
        void Insert(Roles model);
        void Update(Roles model);
        void Delete(int id);
    }
}
