using BuyBee.MVC.Client.ViewModels;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(AuthenticationSchemes = "VendorCookie", Roles = "Vendor")]
    public class ProfileController : Controller
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(IVendorRepository vendorRepository, ILogger<ProfileController> logger)
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
            var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId);

            if (vendor == null)
            {
                return NotFound();
            }

            var vm = new VendorProfileViewModel
            {
                VendorId = vendor.VendorId,
                StoreName = vendor.StoreName,
                StoreSlug = vendor.StoreSlug,
                StoreDescription = vendor.StoreDescription,
                StoreLogo = vendor.StoreLogo,
                ContactEmail = vendor.ContactEmail,
                ContactPhone = vendor.ContactPhone,
                BusinessAddress = vendor.BusinessAddress,
                City = vendor.City,
                State = vendor.State,
                Pincode = vendor.Pincode,
                GstNumber = vendor.GstNumber,
                PanNumber = vendor.PanNumber,
                BankAccountNumber = vendor.BankAccountNumber,
                BankIfsc = vendor.BankIfsc,
                CommissionRate = vendor.CommissionRate,
                IsApproved = vendor.IsApproved
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(VendorProfileViewModel model)
        {
            int vendorId = GetCurrentVendorId();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var vendor = await _vendorRepository.GetVendorByIdAsync(vendorId);
            if (vendor == null)
            {
                return NotFound();
            }

            vendor.StoreName = model.StoreName;
            vendor.StoreDescription = model.StoreDescription;
            vendor.ContactEmail = model.ContactEmail;
            vendor.ContactPhone = model.ContactPhone;
            vendor.BusinessAddress = model.BusinessAddress;
            vendor.City = model.City;
            vendor.State = model.State;
            vendor.Pincode = model.Pincode;
            vendor.GstNumber = model.GstNumber;
            vendor.PanNumber = model.PanNumber;
            vendor.BankAccountNumber = model.BankAccountNumber;
            vendor.BankIfsc = model.BankIfsc;

            bool success = await _vendorRepository.UpdateVendorProfileAsync(vendor);
            if (success)
            {
                TempData["SuccessMessage"] = "Store profile and banking information updated successfully!";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update store profile.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
