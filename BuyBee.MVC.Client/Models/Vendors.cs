using System.ComponentModel.DataAnnotations;

namespace EMart.Models
{
    public class Vendors
    {
        public int VendorId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "Store Name is required")]
        [StringLength(150)]
        [Display(Name = "Store Name")]
        public string StoreName { get; set; } = string.Empty;

        [Required]
        [StringLength(150)]
        [Display(Name = "Store Slug")]
        public string StoreSlug { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Store Description")]
        public string? StoreDescription { get; set; }

        [StringLength(300)]
        [Display(Name = "Store Logo")]
        public string? StoreLogo { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Support / Contact Email")]
        public string? ContactEmail { get; set; }

        [Phone]
        [StringLength(20)]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [StringLength(300)]
        [Display(Name = "Business Address")]
        public string? BusinessAddress { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? State { get; set; }

        [StringLength(20)]
        public string? Pincode { get; set; }

        [StringLength(50)]
        [Display(Name = "GST Number")]
        public string? GstNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "PAN Number")]
        public string? PanNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "Bank Account Number")]
        public string? BankAccountNumber { get; set; }

        [StringLength(20)]
        [Display(Name = "Bank IFSC Code")]
        public string? BankIfsc { get; set; }

        [Display(Name = "Commission Rate (%)")]
        public decimal CommissionRate { get; set; } = 10.00m;

        public bool IsApproved { get; set; } = true;

        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedOn { get; set; }
    }
}
