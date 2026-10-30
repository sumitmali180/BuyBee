using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EMart.Models
{
    public class Products
    {
        public int? ProductId { get; set; }

        //[Required(ErrorMessage = "Enter SKU.")]
        //[DisplayName("SKU")]
        //[StringLength(50)]
        public string? SKU { get; set; }

        [Required(ErrorMessage = "Enter Product Name.")]
        [DisplayName("Product Name")]
        [StringLength(300)]
        public string? ProductName { get; set; }

        [Required(ErrorMessage = "Select Unit.")]
        [DisplayName("Unit")]
        public int? UnitId { get; set; }

        [Required(ErrorMessage = "Select Category.")]
        [DisplayName("Category")]
        public int? CategoryId { get; set; }

        [Required(ErrorMessage = "Select Sub Category.")]
        [DisplayName("Sub Category")]
        public int? SubCategoryId { get; set; }

        [Required(ErrorMessage = "Select Brand.")]
        [DisplayName("Brand")]
        public int? BrandId { get; set; }

        [Required(ErrorMessage = "Enter Price.")]
        [Range(0.01, 99999999)]
        [DisplayName("Price")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Enter Quantity.")]
        [Range(0, 999999)]
        [DisplayName("Quantity")]
        public int? Quantity { get; set; }

        [DisplayName("Description")]
        public string? Description { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; } = true;

        public DateTime? CreatedDate { get; set; }

        public DateTime? CreatedOn { get; set; }

        public DateTime? UpdatedOn { get; set; }

        public int? CreatedBy { get; set; }

        public int? UpdatedBy { get; set; }

        // Display Fields (used in Grid/Details)
        public string? CategoryName { get; set; }

        public string? SubCategoryName { get; set; }

        public string? BrandName { get; set; }

        public string? UnitName { get; set; }

        public string? ThumbnailImage { get; set; }

        // Vendor Fields
        public int? VendorId { get; set; }

        public string ApprovalStatus { get; set; } = "Approved";

        public string? RejectionReason { get; set; }

        public int LowStockThreshold { get; set; } = 5;

        public string? VendorStoreName { get; set; }
    }
}