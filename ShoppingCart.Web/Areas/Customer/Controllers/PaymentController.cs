using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.ModelVM;
using ShoppingCart.Entities.Repositories;
using ShoppingCart.Web.Helpers;
using System.Security.Claims;
using System.Text.Json;

namespace ShoppingCart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class PaymentController : Controller
    {
        private readonly IGenericRepository<OrderItem> _orderItemRepository;
        private readonly IGenericRepository<Order> _orderRepository;


        public PaymentController(IGenericRepository<OrderItem> orderItemRepository,
                IGenericRepository<Order> orderRepository)
        {
            _orderItemRepository = orderItemRepository;
            _orderRepository = orderRepository;
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
        public IActionResult CreateCheckoutSession(OrderSummaryVM orderSummaryVM)
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
                        Address = orderSummaryVM.Address,
                        City = orderSummaryVM.City,
                        PhoneNumber = orderSummaryVM.PhoneNumber,
                        OrderStatus = OrderStatus.Pending.ToString(),
                        PaymentStatus = OrderStatus.Pending.ToString(),
                        UserId = userId,
                    };

                    _orderRepository.Add(order);
                    _orderRepository.Save();


                    foreach (var item in sessionOrder)
                    {
                        OrderItem orderItem = new()
                        {
                            OrderId = order.Id,
                            ProductId = item.ProductId,
                            Quantity = item.Quantity,
                            Price = item.Price,
                        };

                        _orderItemRepository.Add(orderItem);
                        _orderItemRepository.Save();
                    }

                    return RedirectToAction("Index", "Home");
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
    }
}
