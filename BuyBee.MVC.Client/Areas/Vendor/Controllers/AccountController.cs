using BuyBee.MVC.Client.ViewModels;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMart.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(AuthenticationSchemes = "VendorCookie", Roles = "Vendor")]
    public class AccountController : Controller
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IVendorRepository _vendorRepository;
        private readonly PasswordHasher<object> _passwordHasher;

        public AccountController(IUsersRepository usersRepository, IVendorRepository vendorRepository)
        {
            _usersRepository = usersRepository;
            _vendorRepository = vendorRepository;
            _passwordHasher = new PasswordHasher<object>();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Vendor"))
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Dashboard", new { area = "Vendor" });
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new VendorLoginViewModel { ReturnUrl = returnUrl });
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(VendorLoginViewModel model, string? returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (string.IsNullOrWhiteSpace(model.Email) || string.IsNullOrWhiteSpace(model.Password))
            {
                ModelState.AddModelError("", "Email and password are required.");
                return View(model);
            }

            var user = _usersRepository.GetByEmail(model.Email.Trim());
            if (user == null)
            {
                ModelState.AddModelError("", "Invalid vendor email or password.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your vendor account has been deactivated. Please contact platform support.");
                return View(model);
            }

            if (!string.Equals(user.RoleName, "Vendor", StringComparison.OrdinalIgnoreCase) && user.RoleId != 3)
            {
                ModelState.AddModelError("", "Access denied. Only registered merchant vendors can access this portal.");
                return View(model);
            }

            bool isPasswordValid = false;
            if (!string.IsNullOrEmpty(user.Password) && user.Password.StartsWith("AQAAAA"))
            {
                var verifyResult = _passwordHasher.VerifyHashedPassword(user, user.Password, model.Password);
                isPasswordValid = (verifyResult != PasswordVerificationResult.Failed);
            }
            else if (!string.IsNullOrEmpty(user.Password))
            {
                if (user.Password == model.Password)
                {
                    isPasswordValid = true;
                    try
                    {
                        string newHash = _passwordHasher.HashPassword(user, model.Password);
                        _usersRepository.UpdatePassword(user.UserId, newHash);
                    }
                    catch
                    {
                        // Ignore auto-upgrade failure during login
                    }
                }
            }

            if (!isPasswordValid)
            {
                ModelState.AddModelError("", "Invalid vendor email or password.");
                return View(model);
            }

            // Retrieve Vendor store record
            var vendor = await _vendorRepository.GetVendorByUserIdAsync(user.UserId);
            if (vendor == null)
            {
                ModelState.AddModelError("", "No merchant store profile is associated with this vendor account.");
                return View(model);
            }

            if (!vendor.IsApproved)
            {
                ModelState.AddModelError("", "Your merchant store profile is pending platform approval. You will receive an email once approved.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? vendor.StoreName),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, "Vendor"),
                new Claim("VendorId", vendor.VendorId.ToString()),
                new Claim("StoreName", vendor.StoreName),
                new Claim("StoreSlug", vendor.StoreSlug)
            };

            var identity = new ClaimsIdentity(claims, "VendorCookie");
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync("VendorCookie", principal, authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Vendor" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("VendorCookie");
            return RedirectToAction("Login", "Account", new { area = "Vendor" });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
