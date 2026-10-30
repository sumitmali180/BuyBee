using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EMart.ViewModels
{
    public class ReviewSubmissionVM
    {
        [Required]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Please select a star rating")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Please enter a review headline")]
        [StringLength(150, ErrorMessage = "Title cannot exceed 150 characters")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please write your review feedback")]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "Review must be at least 10 characters")]
        public string Comment { get; set; } = string.Empty;

        public List<IFormFile>? Photos { get; set; }
    }

    public class ProductRatingSummaryVM
    {
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int FiveStarCount { get; set; }
        public int FourStarCount { get; set; }
        public int ThreeStarCount { get; set; }
        public int TwoStarCount { get; set; }
        public int OneStarCount { get; set; }

        public int FiveStarPct => TotalReviews > 0 ? (int)Math.Round((double)FiveStarCount / TotalReviews * 100) : 0;
        public int FourStarPct => TotalReviews > 0 ? (int)Math.Round((double)FourStarCount / TotalReviews * 100) : 0;
        public int ThreeStarPct => TotalReviews > 0 ? (int)Math.Round((double)ThreeStarCount / TotalReviews * 100) : 0;
        public int TwoStarPct => TotalReviews > 0 ? (int)Math.Round((double)TwoStarCount / TotalReviews * 100) : 0;
        public int OneStarPct => TotalReviews > 0 ? (int)Math.Round((double)OneStarCount / TotalReviews * 100) : 0;
    }

    public class ProductReviewItemVM
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public string CustomerName { get; set; } = "Verified Customer";
        public int Rating { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public List<string> Photos { get; set; } = new();
        public bool IsVerifiedPurchase { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
