using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(AuthenticationSchemes = "AdminCookie", Roles = "Admin")]
    public class ReportsController : Controller
    {
        private readonly IReportRepository _reportRepository;
        private readonly IOrderRepository _orderRepository;

        public ReportsController(
            IReportRepository reportRepository,
            IOrderRepository orderRepository)
        {
            _reportRepository = reportRepository;
            _orderRepository = orderRepository;
        }

        // Reports Home
        public IActionResult Index()
        {
            var reports = new List<ReportMenuItemViewModel>
            {
                new()
                {
                    Name = "Product List",
                    Description = "View all products with category, sub-category, brand, price and stock.",
                    Icon = "bi-box-seam",
                    Action = "ProductsReport"
                },

                new()
                {
                    Name = "Category List",
                    Description = "View all product categories.",
                    Icon = "bi-grid",
                    Action = "CategoriesReport"
                },

                new()
                {
                    Name = "Brand List",
                    Description = "View all product brands.",
                    Icon = "bi-bricks",
                    Action = "BrandsReport"
                },

                new()
                {
                    Name = "Sub Category List",
                    Description = "View all product sub-categories.",
                    Icon = "bi-diagram-3",
                    Action = "SubCategoriesReport"
                },

                new()
                {
                    Name = "Order List",
                    Description = "View customer orders and order information.",
                    Icon = "bi-cart-check",
                    Action = "OrdersReport"
                },

                new()
                {
                    Name = "Order Invoice",
                    Description = "Generate invoice for a particular order.",
                    Icon = "bi-receipt",
                    Action = "OrderInvoice",
                    RequiresParameter = true
                }
            };

            return View(reports);
        }

        public IActionResult ProductsReport()
        {
            ViewData["Title"] = "Product List Report";

            ViewBag.ReportUrl =
                "https://localhost/ReportServer?/Testing/ProductsList&rs:Command=Render&rs:Embed=true";

            return View("ReportViewer");
        }

        public IActionResult CategoriesReport()
        {
            ViewData["Title"] = "Category List Report";

            ViewBag.ReportUrl =
                "https://localhost/ReportServer?/Testing/CategoryList&rs:Command=Render&rs:Embed=true";

            return View("ReportViewer");
        }

        public IActionResult BrandsReport()
        {
            ViewData["Title"] = "Brand List Report";

            ViewBag.ReportUrl =
                "https://localhost/ReportServer?/Testing/BrandList&rs:Command=Render&rs:Embed=true";

            return View("ReportViewer");
        }

        public IActionResult SubCategoriesReport()
        {
            ViewData["Title"] = "Sub Category List Report";

            ViewBag.ReportUrl =
                "https://localhost/ReportServer?/Testing/SubCategoryList&rs:Command=Render&rs:Embed=true";

            return View("ReportViewer");
        }

        public IActionResult OrdersReport()
        {
            ViewData["Title"] = "Order List Report";

            ViewBag.ReportUrl =
                "https://localhost/ReportServer?/Testing/OrderList&rs:Command=Render&rs:Embed=true";

            return View("ReportViewer");
        }


        // =====================================================
        // ORDER INVOICE - SELECT ORDER
        // =====================================================

        public async Task<IActionResult> OrderInvoice()
        {
            var orders = await _orderRepository.GetAllOrders();

            ViewBag.OrderCount = orders?.Count ?? 0;

            var model = new OrderInvoiceViewModel
            {
                Orders = orders ?? new List<EMart.Models.Orders>()
            };

            ViewData["Title"] = "Order Invoice";

            return View(model);
        }


        // =====================================================
        // ORDER INVOICE REPORT
        // =====================================================

        public IActionResult OrderInvoiceReport(int orderId)
        {
            if (orderId <= 0)
            {
                return BadRequest("Invalid Order Id.");
            }

            ViewData["Title"] = "Order Invoice";

            ViewBag.ReportUrl =
                $"https://localhost/ReportServer?/EmartTesting/OrderInvoice" +
                $"&OrderId={orderId}" +
                $"&rs:Command=Render" +
                $"&rs:Embed=true";

            return View("ReportViewer");
        }


        public async Task<IActionResult> ProductsReportPdf()
        {
            var pdfBytes =
                await _reportRepository.GetProductsReportPdfAsync();

            return File(
                pdfBytes,
                "application/pdf",
                "ProductsReport.pdf");
        }
    }
}