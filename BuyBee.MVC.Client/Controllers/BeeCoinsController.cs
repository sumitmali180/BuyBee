using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    public class BeeCoinsController : Controller
    {
        private readonly IBeeCoinsRepository _beeCoinsRepository;
        private readonly IOrderRepository _orderRepository;

        public BeeCoinsController(
            IBeeCoinsRepository beeCoinsRepository,
            IOrderRepository orderRepository)
        {
            _beeCoinsRepository = beeCoinsRepository;
            _orderRepository = orderRepository;
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

        #region Index

        [HttpGet]
        [Authorize(AuthenticationSchemes = "CustomerCookie")]
        public async Task<IActionResult> Index()
        {
            int? customerId = GetCustomerId();
            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/BeeCoins" });
            }

            var wallet = await _beeCoinsRepository.GetWallet(customerId.Value);
            var transactions = await _beeCoinsRepository.GetTransactions(customerId.Value);
            var pastOrders = await _orderRepository.GetByCustomerId(customerId.Value);

            var model = new BeeCoinsDashboardVM
            {
                Wallet = wallet,
                Transactions = transactions,
                TotalOrdersWithCoins = pastOrders?.Count(o => o.CoinsRedeemed > 0 || o.CoinsEarned > 0) ?? 0
            };

            return View(model);
        }

        #endregion

        #region Get Balance API

        [HttpGet]
        public async Task<IActionResult> GetBalance()
        {
            int? customerId = GetCustomerId();
            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return Json(new { success = false, balance = 0, valueInRupees = 0 });
            }

            var wallet = await _beeCoinsRepository.GetWallet(customerId.Value);
            return Json(new
            {
                success = true,
                balance = wallet.Balance,
                valueInRupees = wallet.ValueInRupees,
                tier = wallet.TierName,
                tierBadgeClass = wallet.TierBadgeClass,
                coinsToNextTier = wallet.CoinsToNextTier,
                progress = wallet.TierProgressPercentage
            });
        }

        #endregion

        #region Apply BeeCoins for Checkout

        [HttpPost]
        [Authorize(AuthenticationSchemes = "CustomerCookie")]
        public async Task<IActionResult> Apply([FromBody] RedeemCoinsRequestVM request)
        {
            int? customerId = GetCustomerId();
            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return Json(new { success = false, message = "Please sign in to redeem BeeCoins." });
            }

            if (request.Coins <= 0)
            {
                return Json(new { success = false, message = "Please enter a valid amount of BeeCoins to redeem." });
            }

            var canRedeem = await _beeCoinsRepository.CanRedeem(customerId.Value, request.Coins);
            if (!canRedeem)
            {
                return Json(new { success = false, message = "Insufficient BeeCoins balance in your wallet." });
            }

            // Save in session
            HttpContext.Session.SetInt32("AppliedBeeCoins", request.Coins);

            return Json(new
            {
                success = true,
                coinsRedeemed = request.Coins,
                discountAmount = (decimal)request.Coins,
                message = $"🪙 {request.Coins} BeeCoins applied! You save ₹{request.Coins}.00."
            });
        }

        #endregion

        #region Remove BeeCoins from Checkout

        [HttpPost]
        [Authorize(AuthenticationSchemes = "CustomerCookie")]
        public IActionResult Remove()
        {
            HttpContext.Session.Remove("AppliedBeeCoins");
            return Json(new { success = true, message = "BeeCoins discount removed from checkout." });
        }

        #endregion
    }
}
