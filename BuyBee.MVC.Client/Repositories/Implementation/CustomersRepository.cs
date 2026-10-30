using EMart.Models;
using EMart.Repositories.Interfaces;

namespace EMart.Repositories.Implementation
{
    public class CustomersRepository : ICustomers
    {
        private readonly IConfiguration _configuration;
        public CustomersRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        private string ConnectionString => _configuration.GetConnectionString("DBCS");
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Customers> GetAll()
        {
            throw new NotImplementedException();
        }

        public Customers GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Customers model)
        {
            throw new NotImplementedException();
        }

        public void Update(Customers model)
        {
            throw new NotImplementedException();
        }
    }
}
