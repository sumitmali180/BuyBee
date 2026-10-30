using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IUsersRepository _usersRepository;
        private readonly IRolesRepository _rolesRepository;

        public UsersController(IUsersRepository usersRepository, IRolesRepository rolesRepository)
        {
            _usersRepository = usersRepository;
            _rolesRepository = rolesRepository;
        }

        // GET: Admin/Users
        public IActionResult Index()
        {
            var users = _usersRepository.GetAll();
            return View(users);
        }

        // GET: Admin/Users/Details/5
        public IActionResult Details(int id)
        {
            var user = _usersRepository.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Admin/Users/Create
        public IActionResult Create()
        {
            UserVM model = new UserVM();

            LoadRoles(model);

            return View(model);
        }

        // POST: Admin/Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(UserVM user)
        {
            if (_usersRepository.EmailExists(user.Email))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (ModelState.IsValid)
            {
                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();
                user.Password = hasher.HashPassword(user, user.Password!);

                _usersRepository.Insert(user);

                TempData["Success"] = "User created successfully.";

                return RedirectToAction(nameof(Index));
            }

            LoadRoles(user);

            return View(user);
        }

        // GET: Admin/Users/Edit/5
        public IActionResult Edit(int id)
        {
            var user = _usersRepository.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            LoadRoles(user);

            return View(user);
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(UserVM user)
        {
            if (_usersRepository.EmailExists(user.Email, user.UserId))
            {
                ModelState.AddModelError("Email", "Email already exists.");
            }

            if (ModelState.IsValid)
            {
                _usersRepository.Update(user);

                if (!string.IsNullOrWhiteSpace(user.Password))
                {
                    var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<object>();
                    string hash = hasher.HashPassword(user, user.Password);
                    _usersRepository.UpdatePassword(user.UserId, hash);
                }

                TempData["Success"] = "User updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            LoadRoles(user);

            return View(user);
        }

        // GET: Admin/Users/Delete/5
        public IActionResult Delete(int id)
        {
            var user = _usersRepository.GetById(id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _usersRepository.Delete(id);

            TempData["Success"] = "User deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        private void LoadRoles(UserVM model)
        {
            model.Roles = _rolesRepository.GetAll()
                .Select(x => new SelectListItem
                {
                    Value = x.RoleId.ToString(),
                    Text = x.RoleName
                }).ToList();
        }
    }
}