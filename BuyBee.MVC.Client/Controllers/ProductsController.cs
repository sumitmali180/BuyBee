using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.Services;
using EMart.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductsRepository _productsRepository;
        private readonly ICategoriesRepository _categoriesRepository;
        private readonly IBrandsRepository _brandsRepository;
        private readonly IPincodeService _pincodeService;
        private readonly IWishlistRepository _wishlistRepository;
        private readonly IReviewRepository _reviewRepository;

        public ProductsController(
            IProductsRepository productsRepository,
            ICategoriesRepository categoriesRepository,
            IBrandsRepository brandsRepository,
            IPincodeService pincodeService,
            IWishlistRepository wishlistRepository,
            IReviewRepository reviewRepository)
        {
            _productsRepository = productsRepository;
            _categoriesRepository = categoriesRepository;
            _brandsRepository = brandsRepository;
            _pincodeService = pincodeService;
            _wishlistRepository = wishlistRepository;
            _reviewRepository = reviewRepository;
        }

        private int GetCustomerId()
        {
            var customerIdClaim = User.FindFirst("CustomerId")?.Value;
            if (int.TryParse(customerIdClaim, out int cid) && cid > 0)
            {
                return cid;
            }

            int? sessionCid = HttpContext.Session.GetInt32("CustomerId");
            if (sessionCid.HasValue && sessionCid.Value > 0)
            {
                return sessionCid.Value;
            }

            return 0;
        }

        #region Product Catalog & Search

        [HttpGet]
        public async Task<IActionResult> Index(
            string? searchText,
            int? categoryId,
            int? subCategoryId,
            int? brandId,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy = "newest",
            int pageNumber = 1,
            int pageSize = 12)
        {
            var categories = _categoriesRepository.GetAll();
            var brands = _brandsRepository.GetAll();

            var productsResult = await _productsRepository.GetCatalogAsync(
                searchText,
                categoryId,
                subCategoryId,
                brandId,
                minPrice,
                maxPrice,
                sortBy,
                pageNumber,
                pageSize);

            string? selectedCatName = categories.FirstOrDefault(c => c.CategoryId == categoryId)?.CategoryName;
            string? selectedBrandName = brands.FirstOrDefault(b => b.BrandId == brandId)?.BrandName;

            var model = new ProductCatalogVM
            {
                Products = productsResult,
                Categories = categories,
                Brands = brands,
                SearchText = searchText,
                SelectedCategoryId = categoryId,
                SelectedCategoryName = selectedCatName,
                SelectedSubCategoryId = subCategoryId,
                SelectedBrandId = brandId,
                SelectedBrandName = selectedBrandName,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SortBy = sortBy ?? "newest"
            };

            return View("Index", model);
        }

        [HttpGet]
        public async Task<IActionResult> Category(int id, string? sortBy, int pageNumber = 1)
        {
            return await Index(null, id, null, null, null, null, sortBy, pageNumber);
        }

        [HttpGet]
        public async Task<IActionResult> Brand(int id, string? sortBy, int pageNumber = 1)
        {
            return await Index(null, null, null, id, null, null, sortBy, pageNumber);
        }

        #endregion

        #region Product Details

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var model = await _productsRepository.GetById(id);

            if (model == null || model.Product == null)
            {
                return NotFound();
            }

            var relatedList = new List<Products>();

            // 1. Same category products
            if (model.Product.CategoryId.HasValue && model.Product.CategoryId.Value > 0)
            {
                var catProducts = await _productsRepository.GetCatalogAsync(
                    searchText: null,
                    categoryId: model.Product.CategoryId,
                    subCategoryId: null,
                    brandId: null,
                    minPrice: null,
                    maxPrice: null,
                    sortBy: "newest",
                    pageNumber: 1,
                    pageSize: 8);

                if (catProducts?.Items != null)
                {
                    relatedList.AddRange(catProducts.Items.Where(p => p.ProductId != id && p.IsActive));
                }
            }

            // 2. Same brand products
            if (relatedList.Count < 4 && model.Product.BrandId.HasValue && model.Product.BrandId.Value > 0)
            {
                var brandProducts = await _productsRepository.GetCatalogAsync(
                    searchText: null,
                    categoryId: null,
                    subCategoryId: null,
                    brandId: model.Product.BrandId,
                    minPrice: null,
                    maxPrice: null,
                    sortBy: "newest",
                    pageNumber: 1,
                    pageSize: 8);

                if (brandProducts?.Items != null)
                {
                    foreach (var bp in brandProducts.Items)
                    {
                        if (bp.ProductId != id && bp.IsActive && !relatedList.Any(r => r.ProductId == bp.ProductId))
                        {
                            relatedList.Add(bp);
                            if (relatedList.Count >= 4) break;
                        }
                    }
                }
            }

            // 3. Fallback to featured products to ensure at least 4 items are displayed
            if (relatedList.Count < 4)
            {
                var featured = await _productsRepository.GetFeaturedProducts(8);
                if (featured != null)
                {
                    foreach (var fp in featured)
                    {
                        if (fp.ProductId != id && fp.IsActive && !relatedList.Any(r => r.ProductId == fp.ProductId))
                        {
                            relatedList.Add(fp);
                            if (relatedList.Count >= 4) break;
                        }
                    }
                }
            }

            model.RelatedProducts = relatedList.Take(4).ToList();

            int customerId = GetCustomerId();
            if (customerId > 0)
            {
                model.IsWishlisted = await _wishlistRepository.IsInWishlist(customerId, id);
            }

            model.RatingSummary = await _reviewRepository.GetRatingSummary(id);
            model.Reviews = await _reviewRepository.GetReviewsByProductId(id);
            model.NewReview = new ReviewSubmissionVM { ProductId = id };

            return View(model);
        }

        #endregion

        #region Pincode & Delivery SLA Checker

        [HttpGet]
        public IActionResult CheckDelivery(string? pincode)
        {
            var result = _pincodeService.CheckDelivery(pincode);
            return Json(result);
        }

        #endregion
    }
}