using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace EMart.Controllers
{
    public class HomeController : Controller
    {
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IBrandsRepository _brandRepository;
        private readonly IProductsRepository _productsRepository;

        public HomeController(
            ICategoriesRepository categoriesRepository,
            IBrandsRepository brandsRepository, 
            IProductsRepository productsRepository)
        {
            _categoriesRepository = categoriesRepository;
            _brandRepository = brandsRepository;
            _productsRepository = productsRepository;
        }
        public async Task<IActionResult> Index(string searchText = "",
            int pageNumber = 1,
            int pageSize = 4)
        {
            HomeVM model = new HomeVM();
            model.Categories = _categoriesRepository.GetAll();
            model.Brands = _brandRepository.GetAll();
            model.LatestProducts = await _productsRepository.GetLeatestProducts(8);
            model.FeaturedProducts = await _productsRepository.GetFeaturedProducts(8);
            model.AllProducts = await _productsRepository.GetAll(searchText,
                pageNumber,
                pageSize);
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Title"] = "About Us - Buybee Commerce";
            return View();
        }

        [HttpGet]
        public IActionResult Contact()
        {
            ViewData["Title"] = "Contact & Support - Buybee Commerce";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Contact(string name, string email, string subject, string message)
        {
            TempData["Success"] = "Thank you for reaching out! Our support team has received your message and will respond within 24 hours.";
            return RedirectToAction("Contact");
        }

        public IActionResult Help()
        {
            ViewData["Title"] = "Help & FAQs - Buybee Commerce";
            return View();
        }

        public IActionResult NotFound()
        {
            return View("404");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
