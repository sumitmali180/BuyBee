using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(
        AuthenticationSchemes = "AdminCookie",
        Roles = "Admin"
    )]
    public class DashboardController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductsRepository _productsRepository;
        private readonly IUsersRepository _usersRepository;

        public DashboardController(
            IOrderRepository orderRepository,
            IProductsRepository productsRepository,
            IUsersRepository usersRepository)
        {
            _orderRepository = orderRepository;
            _productsRepository = productsRepository;
            _usersRepository = usersRepository;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _orderRepository.GetAllOrders() ?? new List<Orders>();
            var productsResult = await _productsRepository.GetAll("", 1, 100);
            var users = _usersRepository.GetAll()?.ToList() ?? new List<UserVM>();

            var lowStock = productsResult.Items.Where(p => (p.Quantity ?? 0) <= 5).ToList();

            var vm = new AdminDashboardVM
            {
                TotalRevenue = orders.Where(o => o.OrderStatus != "Cancelled").Sum(o => o.GrandTotal),
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.OrderStatus == "Pending" || o.OrderStatus == "Processing"),
                TotalProducts = productsResult.TotalRecords,
                LowStockProductsCount = lowStock.Count,
                LowStockProducts = lowStock.Take(5).ToList(),
                TotalCustomers = users.Count,
                RecentOrders = orders.OrderByDescending(o => o.OrderId).Take(8).ToList()
            };

            return View(vm);
        }
    }
}