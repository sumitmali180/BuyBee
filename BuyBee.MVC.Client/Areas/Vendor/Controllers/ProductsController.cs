using BuyBee.MVC.Client.ViewModels;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace EMart.Areas.Vendor.Controllers
{
    [Area("Vendor")]
    [Authorize(AuthenticationSchemes = "VendorCookie", Roles = "Vendor")]
    public class ProductsController : Controller
    {
        private readonly IVendorRepository _vendorRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly ISubCategoriesRepository _subCategoriesRepository;
        private readonly IBrandsRepository _brandsRepository;
        private readonly IUnitsRepository _unitsRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IVendorRepository vendorRepository,
            ICategoriesRepository categoriesRepository,
            ISubCategoriesRepository subCategoriesRepository,
            IBrandsRepository brandsRepository,
            IUnitsRepository unitsRepository,
            IWebHostEnvironment environment,
            ILogger<ProductsController> logger)
        {
            _vendorRepository = vendorRepository;
            _categoriesRepository = categoriesRepository;
            _subCategoriesRepository = subCategoriesRepository;
            _brandsRepository = brandsRepository;
            _unitsRepository = unitsRepository;
            _environment = environment;
            _logger = logger;
        }

        private int GetCurrentVendorId()
        {
            var claim = User.FindFirst("VendorId")?.Value;
            if (int.TryParse(claim, out int vendorId))
            {
                return vendorId;
            }
            throw new UnauthorizedAccessException("Vendor identity claim not found in authentication cookie.");
        }

        private void LoadDropdowns(VendorProductFormViewModel model)
        {
            var categories = _categoriesRepository.GetAll();
            model.Categories = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            }).ToList();

            var subCategories = _subCategoriesRepository.GetAll("", 1, 1000);
            model.SubCategories = subCategories.Items.Select(s => new SelectListItem
            {
                Value = s.SubCategoryId.ToString(),
                Text = s.SubCategoryName
            }).ToList();

            var brands = _brandsRepository.GetAll();
            model.Brands = brands.Select(b => new SelectListItem
            {
                Value = b.BrandId.ToString(),
                Text = b.BrandName
            }).ToList();

            var units = _unitsRepository.GetAll();
            model.Units = units.Select(u => new SelectListItem
            {
                Value = u.UnitId.ToString(),
                Text = u.UnitName
            }).ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? categoryId = null, string? searchTerm = null, string stockFilter = "ALL")
        {
            int vendorId = GetCurrentVendorId();

            var allProducts = await _vendorRepository.GetVendorProductsAsync(vendorId, stockFilter: "ALL");
            var filteredProducts = await _vendorRepository.GetVendorProductsAsync(vendorId, categoryId, searchTerm, stockFilter);

            var categories = _categoriesRepository.GetAll();

            var vm = new VendorProductListViewModel
            {
                Products = filteredProducts,
                Categories = categories.Select(c => new SelectListItem
                {
                    Value = c.CategoryId.ToString(),
                    Text = c.CategoryName,
                    Selected = (c.CategoryId == categoryId)
                }).ToList(),
                SearchTerm = searchTerm,
                SelectedCategoryId = categoryId,
                StockFilter = stockFilter,
                TotalCount = allProducts.Count,
                LowStockCount = allProducts.Count(p => p.IsLowStock),
                OutOfStockCount = allProducts.Count(p => p.IsOutOfStock)
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new VendorProductFormViewModel
            {
                IsActive = true,
                LowStockThreshold = 5,
                Quantity = 10,
                ApprovalStatus = "Approved"
            };

            LoadDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VendorProductFormViewModel model)
        {
            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            int vendorId = GetCurrentVendorId();

            int productId = await _vendorRepository.CreateProductAsync(model, vendorId);
            if (productId <= 0)
            {
                ModelState.AddModelError("", "Failed to save product to database.");
                LoadDropdowns(model);
                return View(model);
            }

            // Upload product images if provided
            if (model.Images != null && model.Images.Count > 0)
            {
                var uploadedPaths = await UploadImagesAsync(model.Images);
                if (uploadedPaths.Count > 0)
                {
                    await _vendorRepository.SaveProductImagesAsync(productId, uploadedPaths);
                }
            }

            TempData["SuccessMessage"] = $"Product '{model.ProductName}' published successfully with instant approval!";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            int vendorId = GetCurrentVendorId();
            var model = await _vendorRepository.GetProductForEditAsync(id, vendorId);

            if (model == null)
            {
                return NotFound();
            }

            LoadDropdowns(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, VendorProductFormViewModel model)
        {
            if (id != model.ProductId)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                LoadDropdowns(model);
                return View(model);
            }

            int vendorId = GetCurrentVendorId();

            bool success = await _vendorRepository.UpdateProductAsync(model, vendorId);
            if (!success)
            {
                ModelState.AddModelError("", "Failed to update product or access denied.");
                LoadDropdowns(model);
                return View(model);
            }

            // Upload additional images if provided
            if (model.Images != null && model.Images.Count > 0)
            {
                var uploadedPaths = await UploadImagesAsync(model.Images);
                if (uploadedPaths.Count > 0)
                {
                    await _vendorRepository.SaveProductImagesAsync(id, uploadedPaths);
                }
            }

            TempData["SuccessMessage"] = $"Product '{model.ProductName}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            int vendorId = GetCurrentVendorId();
            bool success = await _vendorRepository.ToggleProductStatusAsync(id, vendorId);

            if (success)
            {
                TempData["SuccessMessage"] = "Product status updated.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not update status for this product.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            int vendorId = GetCurrentVendorId();
            bool success = await _vendorRepository.DeleteProductImageAsync(imageId, productId, vendorId);

            if (success)
            {
                TempData["SuccessMessage"] = "Product image removed.";
            }
            else
            {
                TempData["ErrorMessage"] = "Could not remove image.";
            }

            return RedirectToAction(nameof(Edit), new { id = productId });
        }

        private async Task<List<string>> UploadImagesAsync(List<IFormFile> files)
        {
            var paths = new List<string>();
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "products");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    string uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
                    string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    paths.Add($"/uploads/products/{uniqueFileName}");
                }
            }

            return paths;
        }
    }
}
