using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using OrderModel = ShoppingCart.Entities.Models.Order;
using ShoppingCart.Entities.ModelVM;
using ShoppingCart.Entities.Repositories;
using ShoppingCart.Web.Helpers;
using Stripe.Climate;
using System.Security.Claims;
using System.Text.Json;

namespace ShoppingCart.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	[Authorize]
	public class OrderController : Controller
	{
		private readonly IGenericRepository<OrderItem> _orderItemRepo;
		private readonly IGenericRepository<OrderModel> _orderRepo;
        private readonly IGenericRepository<Coupon> _couponRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IGenericRepository<OrderItem> orderItemRepo,
				IGenericRepository<OrderModel> orderRepo,
				IGenericRepository<Coupon> couponRepo,
				UserManager<ApplicationUser> userManager)
		{
			_orderItemRepo = orderItemRepo;
			_orderRepo = orderRepo;
            _couponRepo = couponRepo;
            _userManager = userManager;
        }

		private List<ShoppingCartVM> GetCartFromSession()
		{
			string sessionCart = HttpContext.Session.GetString("Cart");
			return string.IsNullOrEmpty(sessionCart)
				? new List<ShoppingCartVM>()
				: JsonSerializer.Deserialize<List<ShoppingCartVM>>(sessionCart);
		}

		private decimal GetTotalPriceAfterDiscount()
		{
            string totalPriceAfterDiscount = HttpContext.Session.GetString("totalPriceAfterDiscount");
            if (decimal.TryParse(totalPriceAfterDiscount, out decimal finalPriceAfterDiscount))
            {
                return finalPriceAfterDiscount;
            }
			return 0;
        }

		[HttpGet]
        public async Task<IActionResult> CreateOrder()
		{
            var sessionOrder = GetCartFromSession();

            string userId = User.Claims.FirstOrDefault(C => C.Type == ClaimTypes.NameIdentifier).Value;

            ApplicationUser user = await _userManager.FindByIdAsync(userId);

            OrderSummaryVM order = new OrderSummaryVM();

            order.shoppingCart = sessionOrder;

            order.Name = user.UserName;
            order.Email = user.Email;
            order.Address = user.Address;
            order.City = user.City;

            string? couponId = HttpContext.Session.GetString("coupon_id");

			order.TotalPriceBeforeDiscount = sessionOrder.Sum(S => S.Quantity * S.Price);

            if (couponId is not null)
            {
                Coupon coupon = _couponRepo.Get(C => C.Code == couponId);
				order.DiscountAmount = ((decimal)coupon.Discount / 100) * order.TotalPriceBeforeDiscount;
			}
			
            order.TotalPrice = GetTotalPriceAfterDiscount() != 0 ?
				GetTotalPriceAfterDiscount() :
				order.TotalPrice = order.TotalPriceBeforeDiscount;
			
            return View(order);
        }


        [HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult CreateOrder(OrderSummaryVM orderSummaryForm)
		{
			var sessionCart = GetCartFromSession();

			orderSummaryForm.TotalPrice = GetTotalPriceAfterDiscount() != 0 ?
				GetTotalPriceAfterDiscount()
				: sessionCart.Sum(SC => SC.Quantity * SC.Price);

            if (ModelState.IsValid)
			{
				try
				{
					string userId = User.Claims.FirstOrDefault(C => C.Type == ClaimTypes.NameIdentifier).Value;

                    OrderModel order = new()
					{
						UserName = orderSummaryForm.Name,
						Email = orderSummaryForm.Email,
						Address = orderSummaryForm.Address,
						City = orderSummaryForm.City,
						PhoneNumber = orderSummaryForm.PhoneNumber,
						OrderStatus = OrderStatus.Pending.ToString(),
						PaymentStatus = OrderStatus.Pending.ToString(),
						TotalPrice = orderSummaryForm.TotalPrice,
						UserId = userId,
					};

					string? couponId = HttpContext.Session.GetString("coupon_id");

					if(couponId is not null)
					{
                        Coupon coupon = _couponRepo.Get(C => C.Code == couponId);
						order.Discount = coupon.Discount;
						order.CouponCode = coupon.Code;
                        
                    }

					_orderRepo.Add(order);
					_orderRepo.Save();

					foreach (var item in sessionCart)
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
					}
					HttpContext.Session.SetInt32("order_id", order.Id);
					return RedirectToAction("Process", "Payment", new { area = "Customer" });
				}
				catch (Exception ex)
				{
					ModelState.AddModelError("", ex.Message);
				}

			}
			orderSummaryForm.shoppingCart = sessionCart;
			orderSummaryForm.TotalPrice = orderSummaryForm.TotalPrice;
			return View(orderSummaryForm);
		}
	}
}
