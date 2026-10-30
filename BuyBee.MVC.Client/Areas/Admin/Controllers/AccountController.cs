using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
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
        public IActionResult Login()
        {
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Dashboard");
            }

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model)
        {
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
                ModelState.AddModelError("", "Your account is deactivated. Contact an administrator.");
                return View(model);
            }

            if (!string.Equals(user.RoleName, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "Access denied. Only administrators can access the admin portal.");
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
                // Support legacy plain-text password and auto-upgrade to secure hash
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
                new Claim(
                    ClaimTypes.NameIdentifier,
                    user.UserId.ToString()
                ),

                new Claim(
                    ClaimTypes.Name,
                    user.FullName!
                ),

                new Claim(
                    ClaimTypes.Email,
                    user.Email!
                ),

                new Claim(
                    ClaimTypes.Role,
                    user.RoleName!
                )
            };


            // ================================
            // CREATE IDENTITY
            // ================================

            var identity = new ClaimsIdentity(claims, "AdminCookie");


            // ================================
            // CREATE PRINCIPAL
            // ================================

            var principal = new ClaimsPrincipal(identity);


            // ================================
            // LOGIN
            // ================================

            await HttpContext.SignInAsync("AdminCookie", principal);


            // ================================
            // REDIRECT
            // ================================

            return RedirectToAction("Index", "Dashboard",
                new
                {
                    area = "Admin"
                }
            );
        }


        // ================================
        // LOGOUT
        // ================================

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(
    "AdminCookie");


            return RedirectToAction("Login", "Account",
                new
                {
                    area = "Admin"
                }
            );
        }
    }
}