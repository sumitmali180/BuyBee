using EMart.Models;
using EMart.Repositories.Implementation;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly IRolesRepository _rolesRepository;
        public RolesController(IRolesRepository rolesRepository)
        {
            _rolesRepository = rolesRepository;
        }
        public IActionResult Index()
        {
            var roles = _rolesRepository.GetAll();
            return View(roles);
        }
        public IActionResult Details(int id)
        {
            var roles = (_rolesRepository.GetById(id));
            if (roles == null)
                return NotFound();
            return View(roles);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Roles roles)
        {
            if (!ModelState.IsValid)
            {
                return View(roles);
            }
            if (ModelState.IsValid)
            {
                _rolesRepository.Insert(roles);
                TempData["Success"] = "Role Add Successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(roles);
        }

        public IActionResult Edit(int id)
        {
            var roles = _rolesRepository.GetById(id);
            if (roles == null)
                return NotFound();
            return View(roles);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Roles roles)
        {
            if (!ModelState.IsValid)
            {
                return View(roles);
            }
            if (ModelState.IsValid)
            {
                _rolesRepository.Update(roles);
                TempData["Success"] = "Role Update Successfully.";
                return RedirectToAction(nameof(Index));
            }
            return View(roles);
        }

        public IActionResult Delete(int id)
        {
            var roles = _rolesRepository.GetById(id);
            if (roles == null)
                return NotFound();
            return View(roles);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _rolesRepository.Delete(id);

            TempData["Success"] = "Role Deleted Successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
