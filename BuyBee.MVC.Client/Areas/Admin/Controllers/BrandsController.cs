using EMart.Models;
using EMart.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class BrandsController : Controller
    {
        private readonly IBrandsRepository _brandRepository;
        private readonly IWebHostEnvironment _environment;

        public BrandsController(IBrandsRepository brandsRepository, IWebHostEnvironment environment)
        {
            _brandRepository = brandsRepository;
            _environment = environment;
        }

        public IActionResult Index()
        {
            var brands = _brandRepository.GetAll();
            return View(brands);
        }

        public IActionResult Details(int id)
        {
            var brand = _brandRepository.GetById(id);
            if (brand == null)
                return NotFound();
            return View(brand);
        }

        public IActionResult Create()
        {
            return View(new Brands { IsActive = true });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Brands brands)
        {
            if (!ModelState.IsValid)
            {
                return View(brands);
            }

            // Handle logo file upload if provided
            if (brands.LogoFile != null && brands.LogoFile.Length > 0)
            {
                brands.BrandLogo = await SaveBrandLogoFile(brands.LogoFile);
            }

            _brandRepository.Insert(brands);
            TempData["Success"] = "Brand added successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Edit(int id)
        {
            var brand = _brandRepository.GetById(id);
            if (brand == null)
                return NotFound();
            return View(brand);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Brands brands)
        {
            if (!ModelState.IsValid)
            {
                return View(brands);
            }

            // Handle new logo file upload if provided
            if (brands.LogoFile != null && brands.LogoFile.Length > 0)
            {
                brands.BrandLogo = await SaveBrandLogoFile(brands.LogoFile);
            }

            _brandRepository.Update(brands);
            TempData["Success"] = "Brand updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        public IActionResult Delete(int id)
        {
            var brand = _brandRepository.GetById(id);
            if (brand == null)
                return NotFound();
            return View(brand);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            _brandRepository.Delete(id);
            TempData["Success"] = "Brand deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        #region File Upload Helper

        private async Task<string> SaveBrandLogoFile(IFormFile file)
        {
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "brands");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            string uniqueFileName = $"{Guid.NewGuid():N}_{Path.GetFileName(file.FileName)}";
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"/uploads/brands/{uniqueFileName}";
        }

        #endregion
    }
}
