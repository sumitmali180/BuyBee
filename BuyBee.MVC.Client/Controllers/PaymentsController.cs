using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    public class PaymentsController : Controller
    {
        private readonly IPayments _payments;
        public PaymentsController(IPayments payments)
        {
            _payments = payments;
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
