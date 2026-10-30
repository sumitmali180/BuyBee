using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMart.Areas.Delivery.Controllers
{
    [Area("Delivery")]
    [Authorize(AuthenticationSchemes = "DeliveryCookie", Roles = "Delivery")]
    public class AccountController : Controller
    {
        private readonly IUsersRepository _usersRepository;
        private readonly PasswordHasher<object> _passwordHasher;

        public AccountController(IUsersRepository usersRepository)
        {
            _usersRepository = usersRepository;
            _passwordHasher = new PasswordHasher<object>();
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Delivery"))
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);

                return RedirectToAction("Index", "Dashboard", new { area = "Delivery" });
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginVM());
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
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
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError("", "Your delivery partner account is deactivated. Contact logistics dispatch.");
                return View(model);
            }

            if (!string.Equals(user.RoleName, "Delivery", StringComparison.OrdinalIgnoreCase) && user.RoleId != 4)
            {
                ModelState.AddModelError("", "Access denied. Only authorized delivery partners can access this portal.");
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
                ModelState.AddModelError("", "Invalid email or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? "Delivery Partner"),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(ClaimTypes.Role, "Delivery")
            };

            var identity = new ClaimsIdentity(claims, "DeliveryCookie");
            var principal = new ClaimsPrincipal(identity);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync("DeliveryCookie", principal, authProperties);

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Dashboard", new { area = "Delivery" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("DeliveryCookie");
            return RedirectToAction("Login", "Account", new { area = "Delivery" });
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
