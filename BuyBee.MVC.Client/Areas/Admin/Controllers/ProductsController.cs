using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductsRepository _productsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ISubCategoriesRepository _subCategoriesRepository;
        private readonly IBrandsRepository _brandsRepository;
        private readonly IUnitsRepository _unitsRepository;

        public ProductsController(
            IProductsRepository productsRepository,
            ICategoriesRepository categoriesRepository,
            ISubCategoriesRepository subCategoriesRepository,
            IBrandsRepository brandsRepository,
            IUnitsRepository unitsRepository)
        {
            _productsRepository = productsRepository;
            _categoriesRepository = categoriesRepository;
            _subCategoriesRepository = subCategoriesRepository;
            _brandsRepository = brandsRepository;
            _unitsRepository = unitsRepository;
        }

        #region Load Dropdowns

        private async Task LoadDropdowns(ProductVM model)
        {
            // Categories
            var categories = _categoriesRepository.GetAll();

            model.Categories = categories.Select(x => new SelectListItem
            {
                Value = x.CategoryId.ToString(),
                Text = x.CategoryName
            });

            // SubCategories
            var subCategories = _subCategoriesRepository.GetAll("", 1, 1000);

            model.SubCategories = subCategories.Items.Select(x => new SelectListItem
            {
                Value = x.SubCategoryId.ToString(),
                Text = x.SubCategoryName
            });

            // Brands
            var brands = _brandsRepository.GetAll();

            model.Brands = brands.Select(x => new SelectListItem
            {
                Value = x.BrandId.ToString(),
                Text = x.BrandName
            });

            // Units
            var units = _unitsRepository.GetAll();

            model.Units = units.Select(x => new SelectListItem
            {
                Value = x.UnitId.ToString(),
                Text = x.UnitName
            });
        }

        #endregion

        #region Index

        public async Task<IActionResult> Index(
            string searchText = "",
            int pageNumber = 1,
            int pageSize = 10)
        {
            var result = await _productsRepository.GetAll(
                searchText,
                pageNumber,
                pageSize);

            ViewBag.SearchText = searchText;

            return View(result);
        }

        #endregion

        #region Create GET

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            ProductVM model = new ProductVM();

            await LoadDropdowns(model);

            model.Product.IsActive = true;

            return View(model);
        }

        #endregion

        #region Create POST

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);
                return View(model);
            }

            bool result = await _productsRepository.Insert(model);

            if (result)
            {
                TempData["Success"] = "Product created successfully.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Unable to save product.";

            await LoadDropdowns(model);

            return View(model);
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            ProductVM model = await _productsRepository.GetById(id);

            if (model == null || model.Product.ProductId == null)
            {
                return NotFound();
            }

            await LoadDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductVM model)
        {
            if (!ModelState.IsValid)
            {
                await LoadDropdowns(model);
                model.ProductImages = await _productsRepository.GetImages(model.Product.ProductId ?? 0);

                return View(model);
            }

            bool result = await _productsRepository.Update(model);

            if (result)
            {
                TempData["Success"] = "Product updated successfully.";

                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "Unable to update product.";

            await LoadDropdowns(model);
            model.ProductImages = await _productsRepository.GetImages(model.Product.ProductId ?? 0);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            ProductVM model = await _productsRepository.GetById(id);

            if (model == null || model.Product.ProductId == null)
            {
                return NotFound();
            }

            //await LoadDropdowns(model);

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            ProductVM model = await _productsRepository.GetById(id);

            if (model == null || model.Product.ProductId == null)
            {
                return NotFound();
            }

            await LoadDropdowns(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool result = await _productsRepository.Delete(id);

            if (result)
            {
                TempData["Success"] = "Product deleted successfully.";
            }
            else
            {
                TempData["Error"] = "Unable to delete product.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            await _productsRepository.DeleteImage(imageId);

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        [HttpGet]
        public IActionResult GetSubCategoriesByCategory(int categoryId)
        {
            var subCategories = _subCategoriesRepository.GetAll("", 1, 1000);
            var filtered = subCategories.Items
                .Where(x => x.CategoryId == categoryId)
                .Select(x => new { value = x.SubCategoryId, text = x.SubCategoryName })
                .ToList();

            return Json(filtered);
        }

    }
}