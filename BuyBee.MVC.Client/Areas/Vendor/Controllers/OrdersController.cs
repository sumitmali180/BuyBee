using BuyBee.MVC.Client.ViewModels;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(AuthenticationSchemes = "VendorCookie", Roles = "Vendor")]
    public class OrdersController : Controller
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IVendorRepository vendorRepository, ILogger<OrdersController> logger)
        {
            _vendorRepository = vendorRepository;
            _logger = logger;
        }

        private int GetCurrentVendorId()
        {
            var claim = User.FindFirst("VendorId")?.Value;
            if (int.TryParse(claim, out int vendorId))
            {
                return vendorId;
            }
            throw new UnauthorizedAccessException("Vendor identity claim not found in authentication cookie.");
        }

        [HttpGet]
        public async Task<IActionResult> Index(string? statusFilter = null)
        {
            int vendorId = GetCurrentVendorId();
            var orders = await _vendorRepository.GetVendorOrdersAsync(vendorId, statusFilter);

            var vm = new VendorOrderListViewModel
            {
                Orders = orders,
                StatusFilter = statusFilter ?? "All",
                TotalCount = orders.Count
            };

            return View(vm);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            int vendorId = GetCurrentVendorId();
            var model = await _vendorRepository.GetVendorOrderDetailsAsync(id, vendorId);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> PackingSlip(int id)
        {
            int vendorId = GetCurrentVendorId();
            var model = await _vendorRepository.GetVendorOrderDetailsAsync(id, vendorId);

            if (model == null)
            {
                return NotFound();
            }

            var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId);
            ViewBag.StoreName = vendor?.StoreName ?? "Merchant Partner";
            ViewBag.ContactEmail = vendor?.ContactEmail ?? "";
            ViewBag.ContactPhone = vendor?.ContactPhone ?? "";
            ViewBag.GstNumber = vendor?.GstNumber ?? "";

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateItemStatus(int orderDetailId, int orderId, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(newStatus))
            {
                TempData["ErrorMessage"] = "Invalid fulfillment status specified.";
                return RedirectToAction(nameof(Details), new { id = orderId });
            }

            int vendorId = GetCurrentVendorId();
            bool success = await _vendorRepository.UpdateItemStatusAsync(orderDetailId, vendorId, newStatus);

            if (success)
            {
                TempData["SuccessMessage"] = $"Item status successfully updated to '{newStatus}'.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not update item status or line item is not owned by your store.";
            }

            return RedirectToAction(nameof(Details), new { id = orderId });
        }
    }
}
