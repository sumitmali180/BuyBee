using EMart.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel;

namespace EMart.ViewModels
{
    public class ProductVM
    {
        // Product Information
        public Products Product { get; set; } = new Products();

        // Dropdown Lists
        public IEnumerable<SelectListItem> Categories { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> SubCategories { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> Brands { get; set; }
            = new List<SelectListItem>();

        public IEnumerable<SelectListItem> Units { get; set; }
            = new List<SelectListItem>();

        // Existing Images (Edit Screen)
        public List<ProductImages> ProductImages { get; set; }
            = new List<ProductImages>();

        // Upload Images
        [DisplayName("Product Images")]
        public List<IFormFile>? Images { get; set; }

        public List<Products> RelatedProducts { get; set; }
            = new List<Products>();

        public bool IsWishlisted { get; set; } = false;

        public ProductRatingSummaryVM RatingSummary { get; set; } = new ProductRatingSummaryVM();

        public List<ProductReviewItemVM> Reviews { get; set; } = new List<ProductReviewItemVM>();

        public ReviewSubmissionVM NewReview { get; set; } = new ReviewSubmissionVM();
    }
}