using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMart.Areas.Delivery.Controllers
{
    [Area("Delivery")]
    [Authorize(AuthenticationSchemes = "DeliveryCookie", Roles = "Delivery")]
    public class OrderController : Controller
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public OrderController(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        private int GetCurrentDriverId()
        {
            var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(claimValue, out int id) ? id : 0;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? status = null)
        {
            int driverId = GetCurrentDriverId();
            if (driverId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "Delivery" });
            }

            // Status filter: normalize if provided
            string? filterStatus = null;
            if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "All", StringComparison.OrdinalIgnoreCase))
            {
                filterStatus = status.Trim();
            }

            var orders = await _deliveryRepository.GetAssignedOrders(driverId, filterStatus);

            ViewBag.CurrentFilter = status ?? "All";
            return View(orders);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int driverId = GetCurrentDriverId();
            if (driverId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "Delivery" });
            }

            var details = await _deliveryRepository.GetDeliveryOrderDetails(id, driverId);
            if (details == null)
            {
                TempData["Error"] = "Delivery waybill not found or not assigned to your route.";
                return RedirectToAction(nameof(Index));
            }

            return View(details);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartDelivery(int id, string? notes = null)
        {
            int driverId = GetCurrentDriverId();
            if (driverId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "Delivery" });
            }

            bool success = await _deliveryRepository.StartDelivery(id, driverId, notes);
            if (success)
            {
                TempData["Success"] = "Order is now marked 'Out for Delivery'! Navigate to the customer location.";
            }
            else
            {
                TempData["Error"] = "Failed to update delivery status. Please try again.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyAndDeliver(DeliveryCompletionVM model)
        {
            int driverId = GetCurrentDriverId();
            if (driverId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "Delivery" });
            }

            string otpToVerify = model.EnteredOtp?.Trim() ?? "";

            if (model.IsBypass)
            {
                if (string.IsNullOrWhiteSpace(model.BypassReason))
                {
                    TempData["Error"] = "A detailed justification note is required for OTP manual bypass.";
                    return RedirectToAction(nameof(Details), new { id = model.OrderId });
                }
                otpToVerify = "BYPASS";
                model.DeliveryNotes = $"[MANUAL BYPASS] Reason: {model.BypassReason}. Note: {model.DeliveryNotes}";
            }
            else
            {
                if (string.IsNullOrWhiteSpace(otpToVerify) || otpToVerify.Length != 4)
                {
                    TempData["Error"] = "Please enter the valid 4-digit OTP provided by the customer.";
                    return RedirectToAction(nameof(Details), new { id = model.OrderId });
                }
            }

            var (success, message) = await _deliveryRepository.VerifyOtpAndDeliver(
                model.OrderId,
                driverId,
                otpToVerify,
                model.DeliveryNotes);

            if (success)
            {
                TempData["Success"] = message;
                return RedirectToAction(nameof(Details), new { id = model.OrderId });
            }
            else
            {
                TempData["Error"] = message;
                return RedirectToAction(nameof(Details), new { id = model.OrderId });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReportFailure(DeliveryFailureVM model)
        {
            int driverId = GetCurrentDriverId();
            if (driverId == 0)
            {
                return RedirectToAction("Login", "Account", new { area = "Delivery" });
            }

            if (string.IsNullOrWhiteSpace(model.FailedReason))
            {
                TempData["Error"] = "Please specify a reason for the delivery failure.";
                return RedirectToAction(nameof(Details), new { id = model.OrderId });
            }

            var (success, message) = await _deliveryRepository.ReportDeliveryFailure(
                model.OrderId,
                driverId,
                model.FailedReason,
                model.DeliveryNotes);

            if (success)
            {
                TempData["Error"] = message;
            }
            else
            {
                TempData["Error"] = message;
            }

            return RedirectToAction(nameof(Details), new { id = model.OrderId });
        }
    }
}
