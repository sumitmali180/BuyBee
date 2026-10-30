using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(AuthenticationSchemes = "VendorCookie", Roles = "Vendor")]
    public class DashboardController : Controller
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(IVendorRepository vendorRepository, ILogger<DashboardController> logger)
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
        public async Task<IActionResult> Index()
        {
            int vendorId = GetCurrentVendorId();
            var model = await _vendorRepository.GetDashboardMetricsAsync(vendorId);

            // In case store name claim was updated
            var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId);
            if (vendor != null)
            {
                model.StoreName = vendor.StoreName;
                model.CommissionRate = vendor.CommissionRate;
            }

            return View(model);
        }
    }
}
