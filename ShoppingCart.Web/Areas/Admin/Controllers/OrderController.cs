using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Repositories;
using ShoppingCart.Entities.Models;
using Microsoft.AspNetCore.Authorization;
using ShoppingCart.Entities.ModelVM;
using ShoppingCart.Web.Helpers;
using Stripe;

namespace ShoppingCart.Web.Areas.Admin.Controllers
{
    [Area(areaName: "Admin")]
    [Authorize(Roles = "Admin")]
    public class OrderController : Controller
    {
        private readonly IGenericRepository<Order> _orderRepo;

        private readonly IGenericRepository<OrderItem> _orderItemRepo;

        public OrderController(IGenericRepository<Order> orderRepo,
            IGenericRepository<OrderItem> orderItemRepo)
        {
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetData()
        {
            var orders = _orderRepo.GetAll()
                .Select(O => new
                {
                    O.Id,
                    O.UserName,
                    O.Email,
                    O.OrderStatus,
                    O.TotalPrice,
                    O.PhoneNumber,
                }).ToList();

            return Json(new { data = orders });
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            Order order = _orderRepo.Get(O => O.Id == id);

            List<OrderItem> orderItems =
                _orderItemRepo
                .GetAll(includeWord: "Product")
                .Where(O => O.OrderId == id).ToList();

            OrderDetailsVM orderDetails = new()
            {
                Id = order.Id,
                UserName = order.UserName,
                Email = order.Email,
                Address = order.Address,
                City = order.City,
                PhoneNumber = order.PhoneNumber,
                TotalPrice = order.TotalPrice,
                Carrier = order.Carrier,
                PaymentDate = order.PaymentDate,
                OrderStatus = order.OrderStatus,
                TrackingNumber = order.TrackingNumber,
                PaymentIntentId = order.PaymentIntentId,
                CreatedAt = order.CreatedAt,
                PaymentStatus = order.PaymentStatus,
                SessionId = order.SessionId,
                ShippingDate = order.ShippingDate,
                OrderItems = orderItems
            };

            return View(orderDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateOrderDetails(int id, OrderDetailsVM orderDetails)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Order order = _orderRepo.Get(O => O.Id == orderDetails.Id);

                    order.UserName = orderDetails.UserName;
                    order.Address = orderDetails.Address;
                    order.City = orderDetails.City;
                    order.PhoneNumber = orderDetails.PhoneNumber;
                    

                    if (orderDetails.Carrier is not null)
                        order.Carrier = orderDetails.Carrier;

                    if (orderDetails.TrackingNumber is not null)
                        order.TrackingNumber = orderDetails.TrackingNumber;

                    _orderRepo.Update(order);
                    _orderRepo.Save();
                    TempData["Updated"] = "Item has Updated Successfully";
                    return RedirectToAction("Details", "Order", new { Id = order.Id });
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
                
            }
            return RedirectToAction("Details", "Order", new { Id = orderDetails.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartProccess(int id)
        {
            try
            {
                Order order = _orderRepo.Get(O => O.Id == id);
                order.OrderStatus = OrderStatus.Proccessing.ToString();
                _orderRepo.Save();
                //TempData["Updated"] = "Order Started Proccessing";
                return Json(new { Success = true ,orderStatus = order.OrderStatus});
            }
            catch
            {
                return Json(new { Success = false });
            }
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StartShipping(int id, [FromBody] ShippingDetailsRequestBody details)
        {
            try
            {
                Order order = _orderRepo.Get(O => O.Id == id);
                order.OrderStatus = OrderStatus.Shipped.ToString();
                order.Carrier = details.OrderCarrier;
                order.TrackingNumber = details.OrderTrackingNumber;
                order.ShippingDate = DateOnly.FromDateTime(DateTime.UtcNow);
                _orderRepo.Save();
                //TempData["Updated"] = "Order Started Shipping";
                return Json(new { 
                    Success = true ,
                    orderStatus = order.OrderStatus, 
                    carrier = order.Carrier,
                    trackingNumber = order.TrackingNumber,
                    shippingDate = order.ShippingDate
                });
            }
            catch
            {
                return Json(new { Success = false });
            }

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                Order order = _orderRepo.Get(O => O.Id == id);
                var orderItems = _orderItemRepo.GetAll(OI => OI.OrderId == id, "Product");
                if(order.PaymentStatus == OrderStatus.Approved.ToString())
                {
                    var options = new RefundCreateOptions()
                    {
                        Reason = RefundReasons.RequestedByCustomer,
                        PaymentIntent = order.PaymentIntentId,

                    };
                    var service = new RefundService();
                    Refund refund = await service.CreateAsync(options);

                    order.OrderStatus = OrderStatus.Cancelled.ToString();
                    order.PaymentStatus = OrderStatus.Refund.ToString();
                    foreach (var item in orderItems)
                        item.Product.Stock += item.Quantity;
                }
                else
                {
                    order.OrderStatus = OrderStatus.Cancelled.ToString();
                    order.PaymentStatus = OrderStatus.Cancelled.ToString();
                }

                _orderRepo.Save();
                _orderItemRepo.Save();
                TempData["Updated"] = "Order has been Canceled";
                return Json(new { Success = true });
            }
            catch
            {
                return Json(new { Success = false });
            }

        }
    }
}
