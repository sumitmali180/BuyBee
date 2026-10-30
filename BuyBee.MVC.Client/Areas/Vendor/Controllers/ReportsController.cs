using BuyBee.MVC.Client.ViewModels;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(AuthenticationSchemes = "VendorCookie", Roles = "Vendor")]
    public class ReportsController : Controller
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(IVendorRepository vendorRepository, ILogger<ReportsController> logger)
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
        public async Task<IActionResult> Index(DateTime? startDate = null, DateTime? endDate = null)
        {
            int vendorId = GetCurrentVendorId();

            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            var model = await _vendorRepository.GetVendorReportsAsync(vendorId, start, end);
            return View(model);
        }
    }
}
