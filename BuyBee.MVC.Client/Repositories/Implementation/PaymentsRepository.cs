using EMart.Models;
using EMart.Repositories.Interfaces;

namespace EMart.Repositories.Implementation
{
    public class PaymentsRepository : IPayments
    {
        private readonly IConfiguration _configuration;
        public PaymentsRepository(IConfiguration configuration)
        {
            _configuration= configuration;
        }

        private string ConnectionString => _configuration.GetConnectionString("DBCS");
      
        public void Delete(int id)
        {
            throw new NotImplementedException();
        }

        public List<Payments> GetAll()
        {
            throw new NotImplementedException();
        }

        public Payments GetById(int id)
        {
            throw new NotImplementedException();
        }

        public void Insert(Payments model)
        {
            throw new NotImplementedException();
        }

        public void Update(Payments model)
        {
            throw new NotImplementedException();
        }
    }
}
