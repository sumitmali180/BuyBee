using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class CouponController : Controller
    {
        private readonly ICouponRepository _couponRepository;

        public CouponController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        public async Task<IActionResult> Index()
        {
            var coupons = await _couponRepository.GetAll();
            return View(coupons);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon model)
        {
            if (string.IsNullOrWhiteSpace(model.Code))
            {
                TempData["Error"] = "Coupon code is required.";
                return RedirectToAction(nameof(Index));
            }

            model.Code = model.Code.Trim().ToUpper();
            var existing = await _couponRepository.GetByCode(model.Code);
            if (existing != null)
            {
                TempData["Error"] = $"Coupon with code '{model.Code}' already exists.";
                return RedirectToAction(nameof(Index));
            }

            if (model.DiscountValue <= 0)
            {
                TempData["Error"] = "Discount value must be greater than zero.";
                return RedirectToAction(nameof(Index));
            }

            bool success = await _couponRepository.Create(model);
            if (success)
            {
                TempData["Success"] = $"Coupon '{model.Code}' created successfully!";
            }
            else
            {
                TempData["Error"] = "Failed to create coupon.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id)
        {
            await _couponRepository.ToggleActive(id);
            TempData["Success"] = "Coupon status updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _couponRepository.Delete(id);
            TempData["Success"] = "Coupon deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}
