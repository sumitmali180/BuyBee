using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMart.Controllers
{
    [Authorize(AuthenticationSchemes = "CustomerCookie")]
    public class CartController : Controller
    {
        private readonly ICartRepository _cartRepository;
        private readonly IWishlistRepository _wishlistRepository;

        public CartController(ICartRepository cartRepository, IWishlistRepository wishlistRepository)
        {
            _cartRepository = cartRepository;
            _wishlistRepository = wishlistRepository;
        }

        #region Helper: Customer ID

        private int GetCustomerId()
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            if (int.TryParse(customerIdClaim, out int cid) && cid > 0)
            {
                return cid;
            }

            int? sessionCid = HttpContext.Session.GetInt32("CustomerId");
            if (sessionCid.HasValue && sessionCid.Value > 0)
            {
                return sessionCid.Value;
            }

            return 0;
        }

        private bool IsAjaxRequest()
        {
            return Request.Headers["X-Requested-With"] == "XMLHttpRequest" ||
                   (Request.Headers.Accept.ToString().Contains("application/json"));
        }

        #endregion

        #region Cart List

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return View(new List<CartItem>());
            }

            var cart = await _cartRepository.GetCart(customerId);

            // Return the view directly (empty list if cart is null/empty) to prevent infinite redirect loop
            return View(cart ?? new List<CartItem>());
        }

        #endregion

        #region Add To Cart Fallback GET

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AddToCart()
        {
            return RedirectToAction("Index", "Products");
        }

        #endregion

        #region Add To Cart

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                string targetReturn = !string.IsNullOrWhiteSpace(returnUrl)
                    ? returnUrl
                    : (Request.Headers.Referer.ToString());

                if (string.IsNullOrWhiteSpace(targetReturn) || targetReturn.Contains("AddToCart"))
                {
                    targetReturn = Url.Action("Details", "Products", new { id = productId }) ?? "/Products";
                }

                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        requiresAuth = true,
                        message = "Please sign in to add items to your cart.",
                        redirect = Url.Action("Login", "Account", new { returnUrl = targetReturn })
                    });
                }

                return RedirectToAction("Login", "Account", new { returnUrl = targetReturn });
            }

            if (quantity <= 0)
            {
                quantity = 1;
            }

            bool result = await _cartRepository.AddToCart(customerId, productId, quantity);
            int newCount = await _cartRepository.GetCartCount(customerId);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = result,
                    message = result ? "Product added to cart successfully!" : "Unable to add product to cart.",
                    cartCount = newCount
                });
            }

            if (result)
            {
                TempData["Success"] = "Product added to cart successfully.";
            }
            else
            {
                TempData["Error"] = "Unable to add product to cart.";
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Update Quantity

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int cartId, int quantity)
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            if (quantity <= 0)
            {
                quantity = 1;
            }

            await _cartRepository.UpdateQuantity(cartId, quantity);

            if (IsAjaxRequest())
            {
                var cart = await _cartRepository.GetCart(customerId);
                decimal subtotal = cart.Sum(x => x.Price * x.Quantity);
                int count = cart.Sum(x => x.Quantity);

                return Json(new
                {
                    success = true,
                    cartCount = count,
                    subtotal = subtotal
                });
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Remove Item

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> Remove(int cartId)
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            await _cartRepository.Remove(cartId);

            if (IsAjaxRequest())
            {
                int count = await _cartRepository.GetCartCount(customerId);
                return Json(new { success = true, cartCount = count, message = "Item removed from cart." });
            }

            TempData["Success"] = "Item removed from cart.";
            return RedirectToAction("Index");
        }

        #endregion

        #region Clear Cart

        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ClearCart()
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            await _cartRepository.ClearCart(customerId);

            TempData["Success"] = "Cart cleared successfully.";
            return RedirectToAction("Index");
        }

        #endregion

        #region Cart Count (AJAX)

        [HttpGet]
        [AllowAnonymous]
        public async Task<JsonResult> GetCartCount()
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return Json(0);
            }

            int count = await _cartRepository.GetCartCount(customerId);
            return Json(count);
        }

        #endregion

        #region Mini Cart (AJAX)

        [HttpGet]
        public async Task<IActionResult> MiniCart()
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return PartialView("_MiniCart", new List<CartItem>());
            }

            var items = await _cartRepository.GetMiniCart(customerId);
            return PartialView("_MiniCart", items ?? new List<CartItem>());
        }

        #endregion

        #region Move To Wishlist (Save For Later)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToWishlist(int cartId, int productId)
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            await _wishlistRepository.AddToWishlist(customerId, productId);
            await _cartRepository.Remove(cartId);

            int cartCount = await _cartRepository.GetCartCount(customerId);
            int wishlistCount = await _wishlistRepository.GetWishlistCount(customerId);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = "Item moved to your Wishlist.",
                    cartCount = cartCount,
                    wishlistCount = wishlistCount
                });
            }

            TempData["Success"] = "Item moved to your Wishlist.";
            return RedirectToAction("Index");
        }

        #endregion
    }
}