using EMart.Models;
using EMart.ViewModels;

namespace EMart.Repositories.Interfaces
{
    public interface IUsersRepository
    {
        IEnumerable<UserVM> GetAll();
        UserVM? GetById(int id);
        void Insert(UserVM user);
        void Update(UserVM user);
        void Delete(int id);
        bool EmailExists(string email, int? userId = null);

        UserVM? Login(string email, string password);
        UserVM? GetByEmail(string email);
        void UpdatePassword(int userId, string passwordHash);
    }
}
