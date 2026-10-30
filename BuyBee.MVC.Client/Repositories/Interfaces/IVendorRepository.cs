using BuyBee.MVC.Client.ViewModels;
using EMart.Models;

namespace EMart.Repositories.Interfaces
{
    public interface IVendorRepository
    {
        // Vendor Account & Profile
        Task<Vendors?> GetVendorByUserIdAsync(int userId);
        Task<Vendors?> GetVendorByIdAsync(int vendorId);
        Task<bool> UpdateVendorProfileAsync(Vendors vendor);

        // Dashboard
        Task<VendorDashboardViewModel> GetDashboardMetricsAsync(int vendorId);

        // Products Catalog & Inventory
        Task<List<VendorProductItemViewModel>> GetVendorProductsAsync(int vendorId, int? categoryId = null, string? searchTerm = null, string stockFilter = "ALL");
        Task<VendorProductFormViewModel?> GetProductForEditAsync(int productId, int vendorId);
        Task<int> CreateProductAsync(VendorProductFormViewModel model, int vendorId);
        Task<bool> UpdateProductAsync(VendorProductFormViewModel model, int vendorId);
        Task<bool> ToggleProductStatusAsync(int productId, int vendorId);
        Task<int> RestockProductAsync(int productId, int vendorId, int quantity);
        Task<bool> SaveProductImagesAsync(int productId, List<string> imagePaths);
        Task<bool> DeleteProductImageAsync(int imageId, int productId, int vendorId);

        // Orders & Line Items
        Task<List<VendorOrderSummaryViewModel>> GetVendorOrdersAsync(int vendorId, string? statusFilter = null);
        Task<VendorOrderDetailsViewModel?> GetVendorOrderDetailsAsync(int orderId, int vendorId);
        Task<bool> UpdateItemStatusAsync(int orderDetailId, int vendorId, string newStatus);

        // Sales Reports & Analytics
        Task<VendorReportsViewModel> GetVendorReportsAsync(int vendorId, DateTime? startDate = null, DateTime? endDate = null);
    }
}
