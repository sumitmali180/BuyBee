using EMart.Models;
using EMart.Repositories.Interfaces;
using EMart.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EMart.Controllers
{
    [Authorize(AuthenticationSchemes = "CustomerCookie")]
    public class OrderController : Controller
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IAddressRepository _addressRepository;

        public OrderController(
            IOrderRepository orderRepository,
            IAddressRepository addressRepository)
        {
            _orderRepository = orderRepository;
            _addressRepository = addressRepository;
        }


        #region Customer ID

        private int? GetCustomerId()
        {
            var claim = User.FindFirst("CustomerId")?.Value;
            if (int.TryParse(claim, out int cid) && cid > 0)
            {
                return cid;
            }

            return HttpContext.Session.GetInt32("CustomerId");
        }

        #endregion


        #region My Orders

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            int? customerId =
                GetCustomerId();

            if (!customerId.HasValue ||
                customerId.Value <= 0)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var orders =
                await _orderRepository.GetByCustomerId(
                    customerId.Value);


            return View(orders);
        }

        #endregion


        #region Confirmation

        [HttpGet]
        public async Task<IActionResult> Confirmation(
            int id)
        {
            int? customerId =
                GetCustomerId();

            if (!customerId.HasValue ||
                customerId.Value <= 0)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var order =
                await _orderRepository.GetById(
                    id,
                    customerId.Value);


            if (order == null)
            {
                return NotFound();
            }


            var address =
                await _addressRepository.GetById(
                    order.AddressId,
                    customerId.Value);


            var model =
                new OrderConfirmationVM
                {
                    Order = order,
                    Address = address
                };


            return View(model);
        }

        #endregion


        #region Details

        [HttpGet]
        public async Task<IActionResult> Details(
            int id)
        {
            int? customerId =
                GetCustomerId();

            if (!customerId.HasValue ||
                customerId.Value <= 0)
            {
                return RedirectToAction(
                    "Login",
                    "Account");
            }


            var order =
                await _orderRepository.GetById(
                    id,
                    customerId.Value);


            if (order == null)
            {
                return NotFound();
            }


            var details = await _orderRepository.GetOrderDetails(id, customerId.Value);


            var address =
                await _addressRepository.GetById(
                    order.AddressId,
                    customerId.Value);


            var model =
                new OrderVM
                {
                    Order = order,

                    OrderDetails = details,

                    Address = address
                };


            return View(model);
        }

        #endregion


        #region Invoice

        [HttpGet]
        public async Task<IActionResult> Invoice(int id)
        {
            int? customerId = GetCustomerId();

            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderRepository.GetById(id, customerId.Value);
            if (order == null)
            {
                return NotFound();
            }

            var details = await _orderRepository.GetOrderDetails(id, customerId.Value);
            var address = await _addressRepository.GetById(order.AddressId, customerId.Value);

            var model = new OrderVM
            {
                Order = order,
                OrderDetails = details,
                Address = address
            };

            return View(model);
        }

        #endregion


        #region Cancel Order

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            int? customerId = GetCustomerId();

            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return RedirectToAction("Login", "Account");
            }

            var order = await _orderRepository.GetById(id, customerId.Value);
            if (order == null)
            {
                return NotFound();
            }

            if (order.OrderStatus == "Pending" || order.OrderStatus == "Confirmed" || order.OrderStatus == "Processing")
            {
                await _orderRepository.UpdateOrderStatus(id, "Cancelled");
                TempData["SuccessMessage"] = $"Order #{order.OrderNumber ?? id.ToString()} has been cancelled successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = $"Order #{order.OrderNumber ?? id.ToString()} cannot be cancelled because it is already {order.OrderStatus}.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        #endregion

        #region Interactive Visual Order Tracker

        [HttpGet]
        public async Task<IActionResult> Track(int id)
        {
            int? customerId = GetCustomerId();
            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Order/Track/{id}" });
            }

            var order = await _orderRepository.GetById(id, customerId.Value);
            if (order == null)
            {
                return NotFound();
            }

            var details = await _orderRepository.GetOrderDetails(id, customerId.Value);
            var address = await _addressRepository.GetById(order.AddressId, customerId.Value);

            string st = order.OrderStatus ?? "Pending";
            bool isCancelled = st.Equals("Cancelled", StringComparison.OrdinalIgnoreCase);

            int step = 1;
            if (st.Equals("Confirmed", StringComparison.OrdinalIgnoreCase)) step = 2;
            else if (st.Equals("Processing", StringComparison.OrdinalIgnoreCase) || st.Equals("Packed", StringComparison.OrdinalIgnoreCase)) step = 3;
            else if (st.Equals("Shipped", StringComparison.OrdinalIgnoreCase) || st.Equals("Dispatched", StringComparison.OrdinalIgnoreCase) || st.Equals("Assigned", StringComparison.OrdinalIgnoreCase)) step = 4;
            else if (st.Equals("Out for Delivery", StringComparison.OrdinalIgnoreCase) || st.Equals("OutForDelivery", StringComparison.OrdinalIgnoreCase)) step = 5;
            else if (st.Equals("Delivered", StringComparison.OrdinalIgnoreCase)) step = 6;

            var baseDate = order.OrderDate;
            var checkpoints = new List<MilestoneCheckpoint>
            {
                new MilestoneCheckpoint
                {
                    Title = "Order Placed & Verified",
                    Location = "Buybee Online Order Desk",
                    Description = "Your order was successfully placed and verified.",
                    Timestamp = baseDate,
                    IsCompleted = step >= 1,
                    IsCurrent = step == 1,
                    Icon = "bi-receipt"
                },
                new MilestoneCheckpoint
                {
                    Title = "Order Confirmed & Payment Verified",
                    Location = "Mumbai Fulfillment Center",
                    Description = "Inventory allocated and order sent for picking.",
                    Timestamp = baseDate.AddMinutes(25),
                    IsCompleted = step >= 2,
                    IsCurrent = step == 2,
                    Icon = "bi-shield-check"
                },
                new MilestoneCheckpoint
                {
                    Title = "Packed & Quality Inspected",
                    Location = "Bhiwandi Central Warehouse",
                    Description = "Items securely packed in tamper-proof Buybee packaging.",
                    Timestamp = baseDate.AddHours(2),
                    IsCompleted = step >= 3,
                    IsCurrent = step == 3,
                    Icon = "bi-box-seam"
                },
                new MilestoneCheckpoint
                {
                    Title = "Dispatched with Courier Partner",
                    Location = "Western Express Logistics Hub",
                    Description = st.Equals("Assigned", StringComparison.OrdinalIgnoreCase) 
                        ? $"Delivery partner assigned ({order.CarrierName ?? "Buybee Express"}). Preparing for transit."
                        : $"Package handed over to {order.CarrierName ?? "Buybee Express"}. AWB assigned.",
                    Timestamp = baseDate.AddHours(6),
                    IsCompleted = step >= 4,
                    IsCurrent = step == 4,
                    Icon = "bi-truck"
                },
                new MilestoneCheckpoint
                {
                    Title = "Out for Delivery",
                    Location = $"{address?.City ?? "Local"} Hub Delivery Center",
                    Description = "Delivery executive is en route with your parcel. Keep OTP ready.",
                    Timestamp = baseDate.AddHours(20),
                    IsCompleted = step >= 5,
                    IsCurrent = step == 5,
                    Icon = "bi-bicycle"
                },
                new MilestoneCheckpoint
                {
                    Title = "Delivered to Customer",
                    Location = address != null ? $"{address.AddressLine1}, {address.City}" : "Customer Destination",
                    Description = "Package handed over safely. Delivered confirmed via OTP.",
                    Timestamp = baseDate.AddHours(24),
                    IsCompleted = step >= 6,
                    IsCurrent = step == 6,
                    Icon = "bi-house-check-fill"
                }
            };

            var trackingVM = new OrderTrackingVM
            {
                Order = order,
                OrderDetails = details,
                Address = address,
                CurrentMilestoneStep = step,
                StatusName = st,
                IsCancelled = isCancelled,
                DeliveryOtp = string.IsNullOrWhiteSpace(order.DeliveryOtp) ? "4829" : order.DeliveryOtp,
                CarrierName = string.IsNullOrWhiteSpace(order.CarrierName) ? "Buybee Express (Air Surface)" : order.CarrierName,
                TrackingNumber = string.IsNullOrWhiteSpace(order.TrackingNumber) ? $"BB-{order.OrderNumber}" : order.TrackingNumber,
                EstimatedDeliveryDate = order.OrderDate.AddDays(2),
                DistanceRemaining = step == 5 ? "1.4 km away • Arriving in ~15 mins" : (step == 6 ? "Delivered" : "In Transit to Local Hub"),
                Checkpoints = checkpoints,
                DeliveryAgent = new DeliveryAgentInfo
                {
                    Name = "Rajesh Kumar",
                    Phone = "+91 98201 44820",
                    Rating = 4.9m,
                    TotalDeliveries = "1,420+",
                    VehicleNumber = "MH-02-EA-9421",
                    VehicleType = "Buybee EV Cargo Van"
                }
            };

            return View(trackingVM);
        }

        [HttpPost]
        public async Task<IActionResult> SimulateTracking(int id, string nextStatus)
        {
            int? customerId = GetCustomerId();
            if (!customerId.HasValue || customerId.Value <= 0)
            {
                return Json(new { success = false, message = "Please log in." });
            }

            var order = await _orderRepository.GetById(id, customerId.Value);
            if (order == null)
            {
                return Json(new { success = false, message = "Order not found." });
            }

            bool ok = await _orderRepository.UpdateOrderStatus(id, nextStatus);
            return Json(new { success = ok, newStatus = nextStatus });
        }

        #endregion
    }
}