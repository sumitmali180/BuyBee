using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EMart.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewRepository _reviewRepository;
        private readonly IWebHostEnvironment _env;

        public ReviewController(IReviewRepository reviewRepository, IWebHostEnvironment env)
        {
            _reviewRepository = reviewRepository;
            _env = env;
        }

        private int GetCustomerId()
        {
            var claim = User.FindFirst("CustomerId")?.Value;
            if (int.TryParse(claim, out int cid) && cid > 0)
            {
                return cid;
            }
            return HttpContext.Session.GetInt32("CustomerId") ?? 0;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ReviewSubmissionVM model)
        {
            int customerId = GetCustomerId();
            if (customerId <= 0)
            {
                TempData["ErrorMessage"] = "Please sign in to write a product review.";
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Products/Details/{model.ProductId}#customer-reviews" });
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide a valid rating, headline, and comment.";
                return RedirectToAction("Details", "Products", new { id = model.ProductId });
            }

            // Handle Photo Uploads
            var photoPaths = new List<string>();
            if (model.Photos != null && model.Photos.Any())
            {
                string uploadFolder = Path.Combine(_env.WebRootPath, "uploads", "reviews");
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                foreach (var photo in model.Photos.Take(3)) // Max 3 photos
                {
                    if (photo.Length > 0 && photo.Length <= 5 * 1024 * 1024) // Max 5MB
                    {
                        var ext = Path.GetExtension(photo.FileName).ToLowerInvariant();
                        if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".webp")
                        {
                            string fileName = $"{Guid.NewGuid()}{ext}";
                            string fullPath = Path.Combine(uploadFolder, fileName);
                            using (var stream = new FileStream(fullPath, FileMode.Create))
                            {
                                await photo.CopyToAsync(stream);
                            }
                            photoPaths.Add($"/uploads/reviews/{fileName}");
                        }
                    }
                }
            }

            bool isVerified = await _reviewRepository.HasCustomerPurchased(customerId, model.ProductId);

            var review = new ProductReview
            {
                ProductId = model.ProductId,
                CustomerId = customerId,
                Rating = model.Rating,
                Title = model.Title,
                Comment = model.Comment,
                ImagesJson = JsonSerializer.Serialize(photoPaths),
                IsVerifiedPurchase = isVerified,
                IsApproved = true
            };

            bool success = await _reviewRepository.AddReview(review);
            if (success)
            {
                TempData["SuccessMessage"] = "Thank you! Your review has been submitted and published.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to submit review. Please try again.";
            }

            return RedirectToAction("Details", "Products", new { id = model.ProductId });
        }
    }
}
