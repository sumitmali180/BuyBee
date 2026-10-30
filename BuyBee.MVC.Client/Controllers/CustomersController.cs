using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    public class CustomersController : Controller
    {
        private readonly ICustomers _customers;
        public CustomersController(ICustomers customers)
        {
            _customers = customers;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
