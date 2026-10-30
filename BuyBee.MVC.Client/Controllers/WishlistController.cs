using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    public class WishlistController : Controller
    {
        private readonly IWishlistRepository _wishlistRepository;
        private readonly ICartRepository _cartRepository;

        public WishlistController(IWishlistRepository wishlistRepository, ICartRepository cartRepository)
        {
            _wishlistRepository = wishlistRepository;
            _cartRepository = cartRepository;
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

        #region Wishlist Page

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                // If not logged in, prompt to log in
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Wishlist") });
            }

            var items = await _wishlistRepository.GetWishlist(customerId);
            return View(items ?? new List<Products>());
        }

        #endregion

        #region Toggle Wishlist (AJAX or Form)

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int productId, string? returnUrl = null)
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                string targetReturn = !string.IsNullOrWhiteSpace(returnUrl)
                    ? returnUrl
                    : (Request.Headers.Referer.ToString());

                if (string.IsNullOrWhiteSpace(targetReturn) || targetReturn.Contains("Wishlist/Toggle"))
                {
                    targetReturn = Url.Action("Details", "Products", new { id = productId }) ?? "/Products";
                }

                if (IsAjaxRequest())
                {
                    return Json(new
                    {
                        success = false,
                        requiresAuth = true,
                        message = "Please sign in to save items to your wishlist.",
                        redirect = Url.Action("Login", "Account", new { returnUrl = targetReturn })
                    });
                }

                return RedirectToAction("Login", "Account", new { returnUrl = targetReturn });
            }

            bool isCurrentlyIn = await _wishlistRepository.IsInWishlist(customerId, productId);
            bool isWishlisted;
            string message;

            if (isCurrentlyIn)
            {
                await _wishlistRepository.RemoveFromWishlist(customerId, productId);
                isWishlisted = false;
                message = "Removed from your Wishlist.";
            }
            else
            {
                await _wishlistRepository.AddToWishlist(customerId, productId);
                isWishlisted = true;
                message = "Added to your Wishlist!";
            }

            int count = await _wishlistRepository.GetWishlistCount(customerId);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    isWishlisted = isWishlisted,
                    message = message,
                    wishlistCount = count
                });
            }

            TempData["Success"] = message;
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Remove From Wishlist

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remove(int productId, string? returnUrl = null)
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            await _wishlistRepository.RemoveFromWishlist(customerId, productId);
            int count = await _wishlistRepository.GetWishlistCount(customerId);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = "Item removed from your Wishlist.",
                    wishlistCount = count
                });
            }

            TempData["Success"] = "Item removed from your Wishlist.";
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Move To Cart

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MoveToCart(int productId)
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = Url.Action("Index", "Wishlist") });
            }

            // 1. Add to cart
            await _cartRepository.AddToCart(customerId, productId, 1);

            // 2. Remove from wishlist
            await _wishlistRepository.RemoveFromWishlist(customerId, productId);

            int newCartCount = await _cartRepository.GetCartCount(customerId);
            int newWishlistCount = await _wishlistRepository.GetWishlistCount(customerId);

            if (IsAjaxRequest())
            {
                return Json(new
                {
                    success = true,
                    message = "Item moved to your shopping bag!",
                    cartCount = newCartCount,
                    wishlistCount = newWishlistCount
                });
            }

            TempData["Success"] = "Item moved to your shopping bag!";
            return RedirectToAction("Index");
        }

        #endregion

        #region Get Wishlist Count / Status (API)

        [HttpGet]
        public async Task<IActionResult> Count()
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                return Json(new { count = 0, productIds = new List<int>() });
            }

            int count = await _wishlistRepository.GetWishlistCount(customerId);
            var ids = await _wishlistRepository.GetWishlistProductIds(customerId);
            return Json(new { count = count, productIds = ids });
        }

        #endregion
    }
}
