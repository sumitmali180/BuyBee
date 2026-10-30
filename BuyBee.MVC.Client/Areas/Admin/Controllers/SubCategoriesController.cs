using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class SubCategoriesController : Controller
    {
        private readonly ISubCategoriesRepository _subCategoriesRepository;

        public SubCategoriesController(ISubCategoriesRepository subCategoriesRepository)
        {
            _subCategoriesRepository = subCategoriesRepository;
        }

        public IActionResult Index(string searchText = "", int pageNumber = 1, int pageSize = 20)
        {
            var model = _subCategoriesRepository.GetAll(
                searchText,
                pageNumber,
                pageSize);

            return View(model);
        }

        public IActionResult Create()
        {
            ViewBag.CategoryList = new SelectList(
                _subCategoriesRepository.GetCategories(),
                "CategoryId",
                "CategoryName");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(SubCategories subCategory)
        {
            if (ModelState.IsValid)
            {
                _subCategoriesRepository.Insert(subCategory);

                TempData["Success"] = "Sub Category created successfully.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new SelectList(
                _subCategoriesRepository.GetCategories(),
                "CategoryId",
                "CategoryName",
                subCategory.CategoryId);

            return View(subCategory);
        }

        public IActionResult Edit(int id)
        {
            var subCategory = _subCategoriesRepository.GetById(id);

            if (subCategory == null)
            {
                return NotFound();
            }

            ViewBag.CategoryList = new SelectList(
                _subCategoriesRepository.GetCategories(),
                "CategoryId",
                "CategoryName",
                subCategory.CategoryId);

            return View(subCategory);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(SubCategories subCategory)
        {
            if (ModelState.IsValid)
            {
                _subCategoriesRepository.Update(subCategory);

                TempData["Success"] = "Sub Category updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            ViewBag.CategoryList = new SelectList(
                _subCategoriesRepository.GetCategories(),
                "CategoryId",
                "CategoryName",
                subCategory.CategoryId);

            return View(subCategory);
        }

        public IActionResult Details(int id)
        {
            var subCategory = _subCategoriesRepository.GetById(id);

            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        public IActionResult Delete(int id)
        {
            var subCategory = _subCategoriesRepository.GetById(id);

            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _subCategoriesRepository.Delete(id);

            TempData["Success"] = "Sub Category deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}