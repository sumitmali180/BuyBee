using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    [Authorize(AuthenticationSchemes = "CustomerCookie")]
    public class AddressController : Controller
    {
        private readonly IAddressRepository _addressRepository;

        public AddressController(
            IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }


        #region Customer ID

        private int? GetCustomerId()
        {
            var claim = User.FindFirst("CustomerId")?.Value;
            if (int.TryParse(claim, out int cid) && cid > 0)
            {
                return cid;
            }

            return HttpContext.Session.GetInt32("CustomerId");
        }

        #endregion


        #region Index

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? customerId = GetCustomerId();

            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        returnUrl = Url.Action(
                            "Index",
                            "Address")
                    });
            }

            var addresses =
                await _addressRepository.GetByCustomerId(
                    customerId.Value);

            return View(addresses);
        }

        #endregion


        #region Create GET

        [HttpGet]
        public IActionResult Create()
        {
            int? customerId = GetCustomerId();

            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        returnUrl = Url.Action(
                            "Create",
                            "Address")
                    });
            }

            return View(new Addresses());
        }

        #endregion


        #region Create POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            Addresses model)
        {
            // =========================================
            // Get CustomerId from Session
            // =========================================

            int? customerId = GetCustomerId();

            if (!customerId.HasValue || customerId.Value <= 0)
            {
                TempData["Error"] =
                    "Customer session expired. Please login again.";

                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        returnUrl = Url.Action(
                            "Create",
                            "Address")
                    });
            }


            // =========================================
            // IMPORTANT
            // Set CustomerId BEFORE ModelState validation
            // =========================================

            model.CustomerId = customerId.Value;


            // =========================================
            // Validate Model
            // =========================================

            //if (!ModelState.IsValid)
            //{
            //    return View(model);
            //}


            // =========================================
            // Save Address
            // =========================================

            int addressId =
                await _addressRepository.Add(model);


            // =========================================
            // Check Result
            // =========================================

            if (addressId > 0)
            {
                TempData["Success"] =
                    "Delivery address added successfully.";

                return RedirectToAction(
                    nameof(Index));
            }


            ModelState.AddModelError(
                "",
                "Unable to save delivery address.");

            return View(model);
        }

        #endregion


        #region Edit GET

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int? customerId = GetCustomerId();

            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return RedirectToAction(
                    "Login",
                    "Account",
                    new
                    {
                        returnUrl = Url.Action(
                            "Edit",
                            "Address",
                            new { id })
                    });
            }

            var model =
                await _addressRepository.GetById(
                    id,
                    customerId.Value);

            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        #endregion


        #region Edit POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Addresses model)
        {
            int? customerId = GetCustomerId();

            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            // CustomerId must come from Session

            model.CustomerId =
                customerId.Value;


            if (!ModelState.IsValid)
            {
                return View(model);
            }


            bool result =
                await _addressRepository.Update(model);


            if (result)
            {
                TempData["Success"] =
                    "Delivery address updated successfully.";

                return RedirectToAction(
                    nameof(Index));
            }


            ModelState.AddModelError(
                "",
                "Unable to update address.");

            return View(model);
        }

        #endregion


        #region Delete

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            int? customerId = GetCustomerId();

            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            bool result =
                await _addressRepository.Delete(
                    id,
                    customerId.Value);


            if (result)
            {
                TempData["Success"] =
                    "Address deleted successfully.";
            }
            else
            {
                TempData["Error"] =
                    "Unable to delete address.";
            }


            return RedirectToAction(
                nameof(Index));
        }

        #endregion
    }
}