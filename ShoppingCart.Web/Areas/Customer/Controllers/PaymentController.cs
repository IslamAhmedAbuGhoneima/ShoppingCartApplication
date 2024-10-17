using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.ModelVM;
using ShoppingCart.Entities.Repositories;
using ShoppingCart.Web.Helpers;
using Stripe.Checkout;
using OrderServices = Stripe.Climate;
using System.Security.Claims;
using System.Text.Json;

namespace ShoppingCart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class PaymentController : Controller
    {
        private readonly IGenericRepository<OrderItem> _orderItemRepo;
        private readonly IGenericRepository<Order> _orderRepo;

        public PaymentController(IGenericRepository<OrderItem> orderItemRepo,
                IGenericRepository<Order> orderRepo)
        {
            _orderItemRepo = orderItemRepo;
            _orderRepo = orderRepo;
        }


        private List<ShoppingCartVM> GetCartFromSession()
        {
            string sessionCart = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(sessionCart)
                ? new List<ShoppingCartVM>()
                : JsonSerializer.Deserialize<List<ShoppingCartVM>>(sessionCart);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCheckoutSession(OrderSummaryVM orderSummaryVM)
        {
            var sessionOrder = GetCartFromSession();
            if (ModelState.IsValid)
            {
                try
                {
                    string userId = User.Claims.FirstOrDefault(C => C.Type == ClaimTypes.NameIdentifier).Value;

                    Order order = new()
                    {
                        UserName = orderSummaryVM.Name,
                        Email = orderSummaryVM.Email,
                        Address = orderSummaryVM.Address,
                        City = orderSummaryVM.City,
                        PhoneNumber = orderSummaryVM.PhoneNumber,
                        OrderStatus = OrderStatus.Pending.ToString(),
                        PaymentStatus = OrderStatus.Pending.ToString(),
                        TotalPrice = sessionOrder.Sum(O => O.Quantity * O.Price),
                        UserId = userId,
                    };

                    _orderRepo.Add(order);
                    _orderRepo.Save();


                    var sessionLineItemOptions = new List<SessionLineItemOptions>();

                    foreach (var item in sessionOrder)
                    {
                        OrderItem orderItem = new()
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price,
                        };

                        _orderItemRepo.Add(orderItem);
                        _orderItemRepo.Save();

                        sessionLineItemOptions.Add(
                            new()
                            {
                                PriceData = new SessionLineItemPriceDataOptions()
                                {
                                    UnitAmount = (long)(item.Price*100),
                                    Currency = "usd",
                                    ProductData = new SessionLineItemPriceDataProductDataOptions()
                                    {
                                        Name = item.Name,
                                        Description = item.Description,
                                    },
                                },
                                Quantity = item.Quantity
                            });
                    }

                    var domain = "https://localhost:7009/";
                    var options = new SessionCreateOptions
                    {
                        LineItems = sessionLineItemOptions,
                        Mode = "payment",
                        SuccessUrl = domain + $"customer/payment/success?id={order.Id}",
                        CancelUrl = domain + $"customer/payment/cancel?id={order.Id}",
                    };

                    var service = new SessionService();
                    //Session session = service.Create(options);
                    Session session =  await service.CreateAsync(options);
                    order.SessionId = session.Id;

                    _orderRepo.Save();

                    Response.Headers.Add("Location", session.Url);

                    return new StatusCodeResult(303);

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }

            }
            orderSummaryVM.shoppingCart = sessionOrder;
            orderSummaryVM.TotalPrice = sessionOrder.Sum(S => S.Quantity * S.Price);
            return View("/Areas/Customer/Views/Cart/OrderSummary.cshtml",  orderSummaryVM);

        }

        public IActionResult Success(int id)
        {
            Order order = _orderRepo.Get(O => O.Id == id);
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
            Order order = _orderRepo.Get(O => O.Id == id);
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
