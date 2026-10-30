using EMart.Models;

namespace EMart.ViewModels
{
    public class AdminDashboardVM
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProductsCount { get; set; }
        public int TotalCustomers { get; set; }
        public List<Orders> RecentOrders { get; set; } = new();
        public List<Products> LowStockProducts { get; set; } = new();
    }
}
