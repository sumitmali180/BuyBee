using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EMart.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly ICartRepository _cartRepository;

        public CartViewComponent(ICartRepository cartRepository)
        {
            _cartRepository = cartRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            int customerId = 0;

            if (UserClaimsPrincipal?.Identity?.IsAuthenticated == true)
            {
                var claim = UserClaimsPrincipal.FindFirst("CustomerId")?.Value;
                if (int.TryParse(claim, out int parsedId) && parsedId > 0)
                {
                    customerId = parsedId;
                }
            }

            if (customerId <= 0)
            {
                int? sessionCid = HttpContext.Session.GetInt32("CustomerId");
                if (sessionCid.HasValue && sessionCid.Value > 0)
                {
                    customerId = sessionCid.Value;
                }
            }

            if (customerId <= 0)
            {
                return View(new MiniCartVM
                {
                    Items = new(),
                    CartCount = 0,
                    GrandTotal = 0
                });
            }

            var items = await _cartRepository.GetMiniCart(customerId) ?? new();

            MiniCartVM model = new MiniCartVM
            {
                Items = items,
                CartCount = items.Sum(x => x.Quantity),
                GrandTotal = items.Sum(x => x.Total)
            };

            return View(model);
        }
    }
}