using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using OrderModel = ShoppingCart.Entities.Models.Order;
using ShoppingCart.Entities.Repositories;
using ShoppingCart.Web.Helpers;
using Stripe.Checkout;
using Stripe;
using StripeCoupon = Stripe.Coupon;
using ShoppingCart.Entities.ModelVM;
using Microsoft.AspNetCore.Authorization;

namespace ShoppingCart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IGenericRepository<OrderItem> _orderItemRepo;
        private readonly IGenericRepository<OrderModel> _orderRepo;

        public PaymentController(IGenericRepository<OrderItem> orderItemRepo,
                IGenericRepository<OrderModel> orderRepo)
        {
            _orderItemRepo = orderItemRepo;
            _orderRepo = orderRepo;
        }


        [HttpGet]
        public IActionResult Process()
        {
            int? orderId = HttpContext.Session.GetInt32("order_id");

            OrderModel order = _orderRepo.Get(O => O.Id == orderId);

            var orderItems = _orderItemRepo.GetAll(OI => OI.OrderId == orderId, "Product")
                        .Select(OI => new OrderItemVM
                        {
                            ProductName = OI.Product.Name,
                            Description = OI.Product.Description,
                            ImageUrl = OI.Product.ImageUrl,
                            Price = OI.Price,
                            Quantity = OI.Quantity,
                        }).ToList();

            OrderItemDetailsVM orderItemDetails = new()
            {
                OrderItems = orderItems,
                Order = order,
            };
            return View("Process", orderItemDetails);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession()
        {
            int? orderId = HttpContext.Session.GetInt32("order_id");

            OrderModel order = _orderRepo.Get(O => O.Id == orderId);

            var orderItems = _orderItemRepo.GetAll(OI => OI.OrderId == orderId, "Product")
                        .Select(OI => new OrderItemVM
                        {
                            ProductName = OI.Product.Name,
                            Description = OI.Product.Description,
                            ImageUrl = OI.Product.ImageUrl,
                            Price = OI.Price,
                            Quantity = OI.Quantity,
                        }).ToList();

            var sessionLineItemOptions = new List<SessionLineItemOptions>();

            foreach (var item in orderItems)
            {
                sessionLineItemOptions.Add(
                    new()
                    {
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = item.ProductName,
                                Description = item.Description
                            },
                        },
                        Quantity = item.Quantity
                    }
                );
            }

            

            var domain = "https://localhost:7009/";
            var options = new SessionCreateOptions
            {
                LineItems = sessionLineItemOptions,
                Mode = "payment",
                SuccessUrl = domain + $"customer/payment/success?id={orderId}",
                CancelUrl = domain + $"customer/payment/cancel?id={orderId}",
            };

            if (order.CouponCode != null)
            {
                var couponOptions = new CouponCreateOptions()
                {
                    Name = order.CouponCode,
                    PercentOff = order.Discount,
                    Duration = "once"
                };
                var couponService = new CouponService();
                StripeCoupon stripeCoupon = await couponService.CreateAsync(couponOptions);

                options.Discounts = [
                    new() { Coupon = stripeCoupon.Id }
                ];

            }

            var service = new SessionService();
            //Session session = service.Create(options);
            Session session = await service.CreateAsync(options);
            order.SessionId = session.Id;

            _orderRepo.Save();

            Response.Headers.Add("Location", session.Url);

            return new StatusCodeResult(303);
        }


        public IActionResult Success(int id)
        {
            OrderModel order = _orderRepo.Get(O => O.Id == id);
            var orderItems = _orderItemRepo.GetAll(OI => OI.OrderId == id, "Product").ToList();

            var services = new SessionService();

            Session session = services.Get(order.SessionId);

            if(session.PaymentStatus.ToLower() == "paid")
            {
                order.PaymentStatus = OrderStatus.Approved.ToString();
                order.OrderStatus = OrderStatus.Approved.ToString();
                order.PaymentDate = DateOnly.FromDateTime(DateTime.UtcNow);
                order.PaymentIntentId = session.PaymentIntentId;

                foreach (var item in orderItems)
                    item.Product.Stock -= item.Quantity;
                
                _orderRepo.Save();
                _orderItemRepo.Save();

                HttpContext.Session.Remove("Cart");
            }

            return View(order);
        }

        public IActionResult Cancel(int id)
        {
            OrderModel order = _orderRepo.Get(O => O.Id == id);
            if(order is not null)
            {
                _orderRepo.Remove(order);
                _orderRepo.Save();
                HttpContext.Session.Remove("Cart");
            }
            return View();
        }

	}
}
