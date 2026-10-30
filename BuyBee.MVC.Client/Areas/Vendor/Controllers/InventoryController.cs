using BuyBee.MVC.Client.ViewModels;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(AuthenticationSchemes = "VendorCookie", Roles = "Vendor")]
    public class InventoryController : Controller
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IVendorRepository vendorRepository, ILogger<InventoryController> logger)
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
        public async Task<IActionResult> Index(string stockFilter = "ALL", string? searchTerm = null)
        {
            int vendorId = GetCurrentVendorId();

            var allProducts = await _vendorRepository.GetVendorProductsAsync(vendorId, stockFilter: "ALL");
            var filteredProducts = await _vendorRepository.GetVendorProductsAsync(vendorId, stockFilter: stockFilter, searchTerm: searchTerm);

            var vm = new VendorInventoryViewModel
            {
                Products = filteredProducts,
                StockFilter = stockFilter,
                SearchTerm = searchTerm,
                TotalCount = allProducts.Count,
                LowStockCount = allProducts.Count(p => p.IsLowStock),
                OutOfStockCount = allProducts.Count(p => p.IsOutOfStock)
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restock([FromBody] VendorRestockRequest request)
        {
            if (request == null || request.ProductId <= 0 || request.AddQuantity <= 0)
            {
                return Json(new { success = false, message = "Invalid product or quantity specified." });
            }

            int vendorId = GetCurrentVendorId();
            int newQuantity = await _vendorRepository.RestockProductAsync(request.ProductId, vendorId, request.AddQuantity);

            if (newQuantity > 0)
            {
                return Json(new { success = true, newStock = newQuantity, message = $"Successfully added {request.AddQuantity} units. New stock is {newQuantity}." });
            }

            return Json(new { success = false, message = "Could not update stock for the requested product." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestockForm(int productId, int addQuantity, string? returnUrl = null)
        {
            if (productId <= 0 || addQuantity <= 0)
            {
                TempData["ErrorMessage"] = "Please enter a valid positive restock quantity.";
                return RedirectToAction(nameof(Index));
            }

            int vendorId = GetCurrentVendorId();
            int newQuantity = await _vendorRepository.RestockProductAsync(productId, vendorId, addQuantity);

            if (newQuantity > 0)
            {
                TempData["SuccessMessage"] = $"Stock successfully updated. Current inventory is {newQuantity} units.";
            }
            else
            {
                TempData["ErrorMessage"] = "Product not found or not owned by your vendor account.";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
