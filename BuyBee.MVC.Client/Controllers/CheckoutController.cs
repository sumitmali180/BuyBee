using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    [Authorize(AuthenticationSchemes = "CustomerCookie")]
    public class CheckoutController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly IAddressRepository _addressRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly ICouponRepository _couponRepository;
        private readonly IBeeCoinsRepository _beeCoinsRepository;

        public CheckoutController(
            ICartRepository cartRepository, 
            IAddressRepository addressRepository, 
            IOrderRepository orderRepository,
            ICouponRepository couponRepository,
            IBeeCoinsRepository beeCoinsRepository)
        {
            _cartRepository = cartRepository;
            _addressRepository = addressRepository;
            _orderRepository = orderRepository;
            _couponRepository = couponRepository;
            _beeCoinsRepository = beeCoinsRepository;
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

        #region Checkout

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });
            }


            // =========================================
            // 3. GET CART
            // =========================================

            var cart =
                await _cartRepository.GetCart(
                    customerId);


            // =========================================
            // 4. CHECK CART
            // =========================================

            if (cart == null || cart.Count == 0)
            {
                TempData["Error"] =
                    "Your cart is empty.";

                return RedirectToAction(
                    "Index",
                    "Cart");
            }


            // =========================================
            // 5. GET ADDRESSES
            // =========================================

            var addresses =
                await _addressRepository.GetByCustomerId(
                    customerId);


            // =========================================
            // 6. CALCULATE TOTAL & PROMOS
            // =========================================

            decimal subtotal =
                cart.Sum(x => x.Price * x.Quantity);

            decimal shipping = 0;

            decimal discount = 0;
            string? appliedCoupon = HttpContext.Session.GetString("AppliedCouponCode");
            if (!string.IsNullOrEmpty(appliedCoupon))
            {
                var coupon = await _couponRepository.GetByCode(appliedCoupon);
                if (coupon != null)
                {
                    var val = _couponRepository.ValidateCoupon(coupon, subtotal);
                    if (val.IsValid)
                    {
                        discount = val.DiscountAmount;
                    }
                    else
                    {
                        HttpContext.Session.Remove("AppliedCouponCode");
                        HttpContext.Session.Remove("AppliedDiscount");
                        appliedCoupon = null;
                    }
                }
                else
                {
                    HttpContext.Session.Remove("AppliedCouponCode");
                    HttpContext.Session.Remove("AppliedDiscount");
                    appliedCoupon = null;
                }
            }

            // =========================================
            // BEECOINS LOYALTY CALCULATION
            // =========================================
            var wallet = await _beeCoinsRepository.GetWallet(customerId);
            int availableCoins = wallet.Balance;
            int sessionCoins = HttpContext.Session.GetInt32("AppliedBeeCoins") ?? 0;
            if (sessionCoins > availableCoins) sessionCoins = availableCoins;

            decimal grandTotalBeforeCoins = Math.Max(0, subtotal + shipping - discount);
            decimal coinsDiscount = Math.Min(grandTotalBeforeCoins, (decimal)sessionCoins);
            decimal grandTotal = Math.Max(0, grandTotalBeforeCoins - coinsDiscount);
            int estimatedCoinsToEarn = (int)(grandTotal / 100) * 5;

            // =========================================
            // 7. CREATE CHECKOUT MODEL
            // =========================================

            CheckoutVM model = new CheckoutVM
            {
                CartItems = cart,

                Addresses = addresses,

                SelectedAddressId =
                    addresses.FirstOrDefault()?.AddressId,

                PaymentMethod = "COD",

                Subtotal = subtotal,

                Shipping = shipping,

                Discount = discount,

                CouponCode = appliedCoupon,

                AvailableBeeCoins = availableCoins,

                CoinsToRedeem = sessionCoins,

                BeeCoinsDiscount = coinsDiscount,

                EstimatedCoinsToEarn = estimatedCoinsToEarn,

                GrandTotal = grandTotal
            };


            // =========================================
            // 8. RETURN VIEW
            // =========================================

            return View(model);
        }

        #endregion


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutVM model)
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                TempData["Error"] = "Customer session expired. Please login again.";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Checkout" });
            }


            // =========================================
            // GET CART
            // =========================================

            var cart =
                await _cartRepository.GetCart(
                    customerId);


            if (cart == null ||
                !cart.Any())
            {
                TempData["Error"] =
                    "Your cart is empty.";

                return RedirectToAction(
                    "Index",
                    "Cart");
            }


            // =========================================
            // VALIDATE ADDRESS
            // =========================================

            if (!model.SelectedAddressId.HasValue)
            {
                ModelState.AddModelError(
                    nameof(model.SelectedAddressId),
                    "Please select a delivery address.");
            }


            // =========================================
            // VALIDATE PAYMENT
            // =========================================

            if (string.IsNullOrWhiteSpace(
                model.PaymentMethod))
            {
                ModelState.AddModelError(
                    nameof(model.PaymentMethod),
                    "Please select a payment method.");
            }


            // =========================================
            // IF INVALID
            // =========================================

            if (!ModelState.IsValid)
            {
                model.CartItems = cart;

                model.Addresses =
                    await _addressRepository.GetByCustomerId(
                        customerId);

                model.Subtotal =
                    cart.Sum(x =>
                        x.Price * x.Quantity);

                model.Shipping = 0;

                model.Discount = 0;

                model.GrandTotal =
                    model.Subtotal;

                return View(
                    "Index",
                    model);
            }


            // =========================================
            // VERIFY ADDRESS
            // =========================================

            var address =
                await _addressRepository.GetById(
                    model.SelectedAddressId.Value,
                    customerId);


            if (address == null)
            {
                ModelState.AddModelError(
                    nameof(model.SelectedAddressId),
                    "Invalid delivery address.");

                model.CartItems = cart;

                model.Addresses =
                    await _addressRepository.GetByCustomerId(
                        customerId);

                model.Subtotal =
                    cart.Sum(x =>
                        x.Price * x.Quantity);

                model.GrandTotal =
                    model.Subtotal;

                return View(
                    "Index",
                    model);
            }


            // =========================================
            // PLACE ORDER
            // =========================================

            try
            {
                string? couponCode = model.CouponCode ?? HttpContext.Session.GetString("AppliedCouponCode");
                decimal discount = model.Discount;
                decimal subtotal = cart.Sum(x => x.Price * x.Quantity);

                if (!string.IsNullOrEmpty(couponCode) && discount <= 0)
                {
                    var coupon = await _couponRepository.GetByCode(couponCode);
                    if (coupon != null)
                    {
                        var val = _couponRepository.ValidateCoupon(coupon, subtotal);
                        if (val.IsValid) discount = val.DiscountAmount;
                    }
                }

                // BeeCoins processing
                int sessionCoins = HttpContext.Session.GetInt32("AppliedBeeCoins") ?? model.CoinsToRedeem;
                var wallet = await _beeCoinsRepository.GetWallet(customerId);
                if (sessionCoins > wallet.Balance) sessionCoins = wallet.Balance;

                decimal remainingAfterPromo = Math.Max(0, subtotal - discount);
                decimal coinsDiscount = Math.Min(remainingAfterPromo, (decimal)sessionCoins);
                decimal finalAmountPaid = Math.Max(0, remainingAfterPromo - coinsDiscount);
                int coinsToEarn = (int)(finalAmountPaid / 100) * 5;

                int orderId =
                    await _orderRepository.PlaceOrder(
                        customerId,
                        model.SelectedAddressId.Value,
                        model.PaymentMethod,
                        couponCode,
                        discount,
                        sessionCoins,
                        coinsDiscount,
                        coinsToEarn);


                if (orderId <= 0)
                {
                    TempData["Error"] =
                        "Unable to place order.";

                    return View(
                        "Index",
                        model);
                }

                // Increment coupon usage and clear from session
                if (!string.IsNullOrEmpty(couponCode))
                {
                    await _couponRepository.IncrementUsage(couponCode);
                    HttpContext.Session.Remove("AppliedCouponCode");
                    HttpContext.Session.Remove("AppliedDiscount");
                }

                // Execute loyalty coins deduction and reward credit
                if (sessionCoins > 0)
                {
                    await _beeCoinsRepository.RedeemCoins(customerId, orderId, sessionCoins);
                }
                if (coinsToEarn > 0)
                {
                    await _beeCoinsRepository.AwardCoins(customerId, orderId, finalAmountPaid);
                }
                HttpContext.Session.Remove("AppliedBeeCoins");


                // =========================================
                // SUCCESS
                // =========================================

                return RedirectToAction(
                    "Confirmation",
                    "Order",
                    new
                    {
                        id = orderId
                    });
            }
            catch (Exception ex)
            {
                Log.WriteLog(ex);
                ModelState.AddModelError(
                    "",
                    ex.Message);

                model.CartItems = cart;

                model.Addresses =
                    await _addressRepository.GetByCustomerId(
                        customerId);

                model.Subtotal =
                    cart.Sum(x =>
                        x.Price * x.Quantity);

                model.Shipping = 0;

                model.Discount = 0;

                model.GrandTotal =
                    model.Subtotal;

                return View(
                    "Index",
                    model);
            }
        }
    }
}