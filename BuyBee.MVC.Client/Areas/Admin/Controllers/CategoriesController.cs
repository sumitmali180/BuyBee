using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoriesRepository _repository;

        public CategoriesController(ICategoriesRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            var categories = _repository.GetAll();

            return View(categories);
        }

        public IActionResult Details(int id)
        {
            var category = _repository.GetById(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Categories model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (ModelState.IsValid)
            {
                _repository.Insert(model);

                TempData["Success"] = "Category Added Successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Edit(int id)
        {
            var category = _repository.GetById(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Categories model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            if (ModelState.IsValid)
            {
                _repository.Update(model);

                TempData["Success"] = "Category Updated Successfully.";

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public IActionResult Delete(int id)
        {
            var category = _repository.GetById(id);

            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _repository.Delete(id);

            TempData["Success"] = "Category Deleted Successfully.";

            return RedirectToAction(nameof(Index));
        }


    }
}