using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace EMart.Models
{
    public class Brands
    {
        public int BrandId { get; set; }

        [Required(ErrorMessage = "Brand Name is required.")]
        [DisplayName("Brand Name")]
        [StringLength(200, ErrorMessage = "Brand Name cannot exceed 200 characters.")]
        public string? BrandName { get; set; }

        [DisplayName("Brand Logo")]
        [StringLength(500, ErrorMessage = "Brand Logo path cannot exceed 500 characters.")]
        public string? BrandLogo { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; } = true;

        [DisplayName("Created On")]
        public DateTime? CreatedOn { get; set; }

        [DisplayName("Updated On")]
        public DateTime? UpdatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        /// <summary>
        /// Backward-compatibility property linking to BrandLogo with smart fallback.
        /// </summary>
        public string? LogoPath
        {
            get => DisplayLogo;
            set => BrandLogo = value;
        }

        /// <summary>
        /// Uploaded image file from Admin create/edit forms.
        /// </summary>
        [DisplayName("Brand Logo File")]
        public IFormFile? LogoFile { get; set; }

        /// <summary>
        /// Resolved logo URL: returns BrandLogo if specified; otherwise attempts local vector asset mapping.
        /// </summary>
        public string DisplayLogo
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(BrandLogo))
                {
                    return BrandLogo;
                }

                if (!string.IsNullOrWhiteSpace(BrandName))
                {
                    string slug = ToBrandSlug(BrandName);
                    return $"/images/brands/{slug}.svg";
                }

                return string.Empty;
            }
        }

        private static string ToBrandSlug(string name)
        {
            string clean = name.Trim().ToLowerInvariant();
            clean = clean.Replace("é", "e").Replace("è", "e");
            clean = Regex.Replace(clean, @"[^a-z0-9]+", "-").Trim('-');
            return clean;
        }
    }
}
