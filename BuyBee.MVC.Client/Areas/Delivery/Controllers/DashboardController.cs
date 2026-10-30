using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMart.Areas.Delivery.Controllers
{
    [Area("Delivery")]
    [Authorize(AuthenticationSchemes = "DeliveryCookie", Roles = "Delivery")]
    public class DashboardController : Controller
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public DashboardController(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        private int GetCurrentDriverId()
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claimValue, out int id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int driverId = GetCurrentDriverId();
            if (driverId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "Delivery" });
            }

            var metrics = await _deliveryRepository.GetShiftMetrics(driverId);
            var allOrders = await _deliveryRepository.GetAssignedOrders(driverId);

            var model = new DeliveryDashboardVM
            {
                DriverId = driverId,
                DriverName = User.Identity?.Name ?? "Delivery Partner",
                DriverEmail = User.FindFirstValue(ClaimTypes.Email) ?? "",
                TotalAssigned = metrics.TotalAssigned,
                TotalOutForDelivery = metrics.TotalOutForDelivery,
                TotalDeliveredToday = metrics.TotalDeliveredToday,
                TotalFailed = metrics.TotalFailed,
                CashToCollect = metrics.CashToCollect,
                CashCollectedToday = metrics.CashCollectedToday,
                ActiveDeliveries = allOrders.Where(o => o.OrderStatus == "Out for Delivery").ToList(),
                AssignedQueue = allOrders.Where(o => o.OrderStatus == "Assigned" || o.OrderStatus == "Shipped" || o.OrderStatus == "Processing").ToList()
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ShiftSummary()
        {
            int driverId = GetCurrentDriverId();
            if (driverId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "Delivery" });
            }

            var metrics = await _deliveryRepository.GetShiftMetrics(driverId);
            var allOrders = await _deliveryRepository.GetAssignedOrders(driverId);

            ViewBag.DeliveredToday = allOrders.Where(o => o.OrderStatus == "Delivered" && o.DeliveredAt?.Date == DateTime.Today).ToList();
            ViewBag.Metrics = metrics;

            return View(metrics);
        }
    }
}
