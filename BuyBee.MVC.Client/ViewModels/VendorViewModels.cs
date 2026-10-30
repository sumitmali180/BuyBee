using System.ComponentModel.DataAnnotations;
using EMart.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BuyBee.MVC.Client.ViewModels
{
    public class VendorLoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }

    public class VendorDashboardViewModel
    {
        public int VendorId { get; set; }
        public string StoreName { get; set; } = string.Empty;
        public decimal CommissionRate { get; set; }

        // Metrics
        public int TotalProducts { get; set; }
        public int ActiveProducts { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int PackedOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalUnitsSold { get; set; }
        public decimal PlatformCommission => Math.Round(TotalRevenue * (CommissionRate / 100m), 2);
        public decimal NetPayout => TotalRevenue - PlatformCommission;

        public List<VendorStockAlertItem> StockAlerts { get; set; } = new();
        public List<VendorRecentOrderSummary> RecentOrders { get; set; } = new();
    }

    public class VendorStockAlertItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public string? CategoryName { get; set; }
        public string? ImagePath { get; set; }
    }

    public class VendorRecentOrderSummary
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public int VendorItemCount { get; set; }
        public decimal VendorTotal { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string RecentItemStatus { get; set; } = string.Empty;
    }

    public class VendorProductListViewModel
    {
        public List<VendorProductItemViewModel> Products { get; set; } = new();
        public List<SelectListItem> Categories { get; set; } = new();
        public string? SearchTerm { get; set; }
        public int? SelectedCategoryId { get; set; }
        public string StockFilter { get; set; } = "ALL";
        public int TotalCount { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
    }

    public class VendorProductItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? SubCategoryId { get; set; }
        public int? BrandId { get; set; }
        public string? BrandName { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int LowStockThreshold { get; set; }
        public bool IsActive { get; set; }
        public string ApprovalStatus { get; set; } = "Approved";
        public string? RejectionReason { get; set; }
        public string? PrimaryImage { get; set; }
        public int ImageCount { get; set; }
        public DateTime CreatedOn { get; set; }
        public bool IsLowStock => StockQuantity <= LowStockThreshold && StockQuantity > 0;
        public bool IsOutOfStock => StockQuantity == 0;
    }

    public class VendorProductFormViewModel
    {
        public int? ProductId { get; set; }

        [Required(ErrorMessage = "Product Name is required")]
        [StringLength(200)]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Detailed Description")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Please select a Category")]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Display(Name = "Sub Category")]
        public int? SubCategoryId { get; set; }

        [Display(Name = "Brand")]
        public int? BrandId { get; set; }

        [Display(Name = "Unit")]
        public int? UnitId { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, 10000000, ErrorMessage = "Price must be greater than 0")]
        [Display(Name = "Selling Price (₹)")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Initial Stock is required")]
        [Range(0, 999999, ErrorMessage = "Stock must be 0 or more")]
        [Display(Name = "Stock Quantity")]
        public int? Quantity { get; set; }

        [Required(ErrorMessage = "Low stock alert threshold is required")]
        [Range(1, 1000, ErrorMessage = "Threshold must be between 1 and 1000")]
        [Display(Name = "Low Stock Alert Threshold")]
        public int LowStockThreshold { get; set; } = 5;

        [Display(Name = "Active / Available for Sale")]
        public bool IsActive { get; set; } = true;

        public string ApprovalStatus { get; set; } = "Approved";

        [Display(Name = "Product Images (Upload one or more)")]
        public List<IFormFile>? Images { get; set; }

        public List<ProductImages> ExistingImages { get; set; } = new();

        // Select lists
        public List<SelectListItem> Categories { get; set; } = new();
        public List<SelectListItem> SubCategories { get; set; } = new();
        public List<SelectListItem> Brands { get; set; } = new();
        public List<SelectListItem> Units { get; set; } = new();
    }

    public class VendorInventoryViewModel
    {
        public List<VendorProductItemViewModel> Products { get; set; } = new();
        public string StockFilter { get; set; } = "ALL";
        public string? SearchTerm { get; set; }
        public int LowStockCount { get; set; }
        public int OutOfStockCount { get; set; }
        public int TotalCount { get; set; }
    }

    public class VendorRestockRequest
    {
        public int ProductId { get; set; }
        public int AddQuantity { get; set; }
    }

    public class VendorOrderListViewModel
    {
        public List<VendorOrderSummaryViewModel> Orders { get; set; } = new();
        public string StatusFilter { get; set; } = "All";
        public int TotalCount { get; set; }
    }

    public class VendorOrderSummaryViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? State { get; set; }
        public int VendorItemCount { get; set; }
        public decimal VendorTotal { get; set; }
        public string ItemStatusSummary { get; set; } = string.Empty;
    }

    public class VendorOrderDetailsViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public string? CarrierName { get; set; }
        public string? TrackingNumber { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? AddressLine1 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }
        public string? CustomerPhoneMasked { get; set; }

        public List<VendorOrderItemViewModel> Items { get; set; } = new();

        public decimal CommissionRate { get; set; } = 10.00m;
        public decimal VendorSubtotal => Items.Sum(i => i.Total);
        public decimal PlatformCommission => Math.Round(VendorSubtotal * (CommissionRate / 100m), 2);
        public decimal NetPayout => VendorSubtotal - PlatformCommission;
    }

    public class VendorOrderItemViewModel
    {
        public int OrderDetailId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Total { get; set; }
        public string ItemStatus { get; set; } = "Pending";
        public string? ImagePath { get; set; }
    }

    public class VendorReportsViewModel
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal GrossSales { get; set; }
        public int OrderCount { get; set; }
        public int UnitsSold { get; set; }
        public decimal PlatformCommission { get; set; }
        public decimal NetPayout { get; set; }
        public decimal CommissionRate { get; set; }
        public List<VendorTopProductViewModel> TopProducts { get; set; } = new();
        public List<VendorDailySalesViewModel> DailySales { get; set; } = new();
    }

    public class VendorTopProductViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int UnitsSold { get; set; }
        public decimal TotalRevenue { get; set; }
        public string? ImagePath { get; set; }
    }

    public class VendorDailySalesViewModel
    {
        public DateTime SaleDate { get; set; }
        public int DailyOrders { get; set; }
        public int DailyUnits { get; set; }
        public decimal DailyRevenue { get; set; }
    }

    public class VendorProfileViewModel
    {
        public int VendorId { get; set; }

        [Required(ErrorMessage = "Store Name is required")]
        [StringLength(150)]
        [Display(Name = "Store Name")]
        public string StoreName { get; set; } = string.Empty;

        [Display(Name = "Store Slug")]
        public string StoreSlug { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Store Description")]
        public string? StoreDescription { get; set; }

        [Display(Name = "Store Logo URL / Path")]
        public string? StoreLogo { get; set; }

        [EmailAddress]
        [Display(Name = "Support / Notification Email")]
        public string? ContactEmail { get; set; }

        [Phone]
        [Display(Name = "Contact Phone")]
        public string? ContactPhone { get; set; }

        [Display(Name = "Business Address")]
        public string? BusinessAddress { get; set; }

        public string? City { get; set; }
        public string? State { get; set; }
        public string? Pincode { get; set; }

        [Display(Name = "GST Identification Number")]
        public string? GstNumber { get; set; }

        [Display(Name = "PAN Number")]
        public string? PanNumber { get; set; }

        [Display(Name = "Bank Account Number")]
        public string? BankAccountNumber { get; set; }

        [Display(Name = "Bank IFSC Code")]
        public string? BankIfsc { get; set; }

        [Display(Name = "Commission Rate (%)")]
        public decimal CommissionRate { get; set; }

        public bool IsApproved { get; set; }
    }
}
