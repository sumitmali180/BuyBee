using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    public class CouponController : Controller
    {
        private readonly ICouponRepository _couponRepository;
        private readonly ICartRepository _cartRepository;

        public CouponController(ICouponRepository couponRepository, ICartRepository cartRepository)
        {
            _couponRepository = couponRepository;
            _cartRepository = cartRepository;
        }

        private int GetCustomerId()
        {
            var claim = User.FindFirst("CustomerId")?.Value;
            if (int.TryParse(claim, out int cid) && cid > 0)
            {
                return cid;
            }
            return HttpContext.Session.GetInt32("CustomerId") ?? 0;
        }

        [HttpPost]
        public async Task<IActionResult> Apply(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                return Json(new { success = false, message = "Please enter a valid coupon code." });
            }

            int customerId = GetCustomerId();
            decimal subtotal = 0;

            if (customerId > 0)
            {
                var cart = await _cartRepository.GetCart(customerId);
                if (cart == null || !cart.Any())
                {
                    return Json(new { success = false, message = "Your cart is empty." });
                }
                subtotal = cart.Sum(x => x.Price * x.Quantity);
            }
            else
            {
                // Fallback for unauthenticated session cart or direct total
                if (decimal.TryParse(Request.Form["subtotal"], out decimal formSubtotal) && formSubtotal > 0)
                {
                    subtotal = formSubtotal;
                }
                else
                {
                    return Json(new { success = false, message = "Please sign in or add items to your cart first." });
                }
            }

            var coupon = await _couponRepository.GetByCode(code);
            if (coupon == null)
            {
                return Json(new { success = false, message = $"Coupon code '{code.ToUpper()}' not found or invalid." });
            }

            var validation = _couponRepository.ValidateCoupon(coupon, subtotal);
            if (!validation.IsValid)
            {
                return Json(new { success = false, message = validation.Message });
            }

            // Save to session
            HttpContext.Session.SetString("AppliedCouponCode", coupon.Code);
            HttpContext.Session.SetString("AppliedDiscount", validation.DiscountAmount.ToString("0.00"));

            decimal newTotal = Math.Max(0, subtotal - validation.DiscountAmount);

            return Json(new
            {
                success = true,
                message = validation.Message,
                code = coupon.Code,
                discount = validation.DiscountAmount,
                discountFormatted = "₹" + validation.DiscountAmount.ToString("N2"),
                newTotal = newTotal,
                newTotalFormatted = "₹" + newTotal.ToString("N2")
            });
        }

        [HttpPost]
        public async Task<IActionResult> Remove()
        {
            HttpContext.Session.Remove("AppliedCouponCode");
            HttpContext.Session.Remove("AppliedDiscount");

            int customerId = GetCustomerId();
            decimal subtotal = 0;
            if (customerId > 0)
            {
                var cart = await _cartRepository.GetCart(customerId);
                if (cart != null && cart.Any())
                {
                    subtotal = cart.Sum(x => x.Price * x.Quantity);
                }
            }

            return Json(new
            {
                success = true,
                message = "Coupon removed successfully.",
                subtotal = subtotal,
                subtotalFormatted = "₹" + subtotal.ToString("N2")
            });
        }

        [HttpGet]
        public IActionResult Current()
        {
            string? code = HttpContext.Session.GetString("AppliedCouponCode");
            string? discountStr = HttpContext.Session.GetString("AppliedDiscount");
            decimal discount = 0;
            decimal.TryParse(discountStr, out discount);

            return Json(new
            {
                hasCoupon = !string.IsNullOrEmpty(code),
                code = code ?? "",
                discount = discount,
                discountFormatted = "₹" + discount.ToString("N2")
            });
        }
    }
}
