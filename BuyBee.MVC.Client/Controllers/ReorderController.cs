using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    public class ReorderController : Controller
    {
        private readonly IReorderRepository _reorderRepository;
        private readonly ICartRepository _cartRepository;

        public ReorderController(
            IReorderRepository reorderRepository,
            ICartRepository cartRepository)
        {
            _reorderRepository = reorderRepository;
            _cartRepository = cartRepository;
        }

        private int? GetCustomerId()
        {
            var claim = User.FindFirst("CustomerId")?.Value;
            if (int.TryParse(claim, out int cid) && cid > 0)
            {
                return cid;
            }
            return HttpContext.Session.GetInt32("CustomerId");
        }

        #region Reorder Hub View

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? customerId = GetCustomerId();
            int cid = customerId ?? 1; // Default to test/demo customer if unauthenticated to show experience

            var hubData = await _reorderRepository.GetReorderHubData(cid);
            ViewBag.IsGuest = !customerId.HasValue || customerId.Value <= 0;
            return View(hubData);
        }

        #endregion

        #region Reorder Single Item

        [HttpPost]
        public async Task<IActionResult> ReorderSingle([FromBody] SingleReorderRequest req)
        {
            int? customerId = GetCustomerId();
            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return Json(new { success = false, message = "Please sign in to reorder items into your cart." });
            }

            int qty = req.Quantity <= 0 ? 1 : req.Quantity;
            bool added = await _cartRepository.AddToCart(customerId.Value, req.ProductId, qty);

            if (added)
            {
                int newCount = await _cartRepository.GetCartCount(customerId.Value);
                return Json(new
                {
                    success = true,
                    cartCount = newCount,
                    message = $"Added {qty} item(s) to your bag!"
                });
            }

            return Json(new { success = false, message = "Unable to add item to bag. Please try again." });
        }

        #endregion

        #region Reorder All Due Items

        [HttpPost]
        public async Task<IActionResult> ReorderAllDue()
        {
            int? customerId = GetCustomerId();
            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return Json(new { success = false, message = "Please sign in to reorder items." });
            }

            var hub = await _reorderRepository.GetReorderHubData(customerId.Value);
            var itemsToAdd = hub.DueItems.Where(x => x.IsInStock).ToList();

            if (itemsToAdd.Count == 0)
            {
                return Json(new { success = false, message = "No items currently due for replenishment." });
            }

            int addedCount = 0;
            foreach (var item in itemsToAdd)
            {
                bool ok = await _cartRepository.AddToCart(customerId.Value, item.ProductId, 1);
                if (ok) addedCount++;
            }

            int totalCartCount = await _cartRepository.GetCartCount(customerId.Value);

            return Json(new
            {
                success = true,
                itemsAdded = addedCount,
                cartCount = totalCartCount,
                message = $"Successfully reordered {addedCount} items to your shopping bag!"
            });
        }

        #endregion

        #region Top Suggestions API (For Home / AI)

        [HttpGet]
        public async Task<IActionResult> GetSuggestions(int take = 6)
        {
            int? customerId = GetCustomerId();
            int cid = customerId ?? 1;
            var suggestions = await _reorderRepository.GetTopReorderSuggestions(cid, take);
            return Json(new { success = true, items = suggestions });
        }

        #endregion

        public class SingleReorderRequest
        {
            public int ProductId { get; set; }
            public int Quantity { get; set; } = 1;
        }
    }
}
