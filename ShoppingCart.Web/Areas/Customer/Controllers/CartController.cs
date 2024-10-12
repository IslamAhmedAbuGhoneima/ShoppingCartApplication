using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.ModelVM;
using ShoppingCart.Entities.Repositories;
using System.Security.Claims;
using System.Text.Json;

namespace ShoppingCart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {

        private readonly IGenericRepository<Product> _productRepository;
        private readonly UserManager<ApplicationUser> _userManager;


        public CartController(IGenericRepository<Product> productRepository,
                UserManager<ApplicationUser> userManager)
        {
            _productRepository = productRepository;
            _userManager = userManager;
        }


        // Helper method to retrieve shopping cart from session
        private List<ShoppingCartVM> GetCartFromSession()
        {
            string sessionCart = HttpContext.Session.GetString("Cart");
            return string.IsNullOrEmpty(sessionCart)
                ? new List<ShoppingCartVM>()
                : JsonSerializer.Deserialize<List<ShoppingCartVM>>(sessionCart);
        }

        // Helper method to save shopping cart to session
        private void SaveCartToSession(List<ShoppingCartVM> shoppingCart)
        {
            HttpContext.Session.SetString("Cart", JsonSerializer.Serialize(shoppingCart));
        }



        public IActionResult Index()
        {
            var shoppingCart = GetCartFromSession();
            ViewData["totalPrice"] = shoppingCart.Sum(C => C.Price * C.Quantity);
            return View(shoppingCart);
        }

        
        public IActionResult AddToCart(int id,int quantity=1)
        {

            // Retrieve product from repository
            var product = _productRepository.Get(p => p.Id == id);
            if (product == null) return NotFound();


            var shoppingCart = GetCartFromSession();

            // Check if the product is already in the cart and update quantity
            var cartItem = shoppingCart.FirstOrDefault(item => item.ProductId == id);
            if (cartItem != null)
            {
                cartItem.Quantity += quantity;
            }
            else
            {
                shoppingCart.Add(new ShoppingCartVM { 
                    ProductId = id, 
                    Name = product.Name,
                    Description = product.Description,
                    ImageUrl = product.ImageUrl,
                    Price = product.Price, 
                    Quantity = quantity,
                });
            }

            SaveCartToSession(shoppingCart);

            int cartItemsCount = shoppingCart.Sum(item => item.Quantity);

            return Json(new { Success = true, CartCount = cartItemsCount });
        }


        public IActionResult Remove(int id)
        {


            var shoppingCart = GetCartFromSession();
            var item = shoppingCart.FirstOrDefault(S => S.ProductId == id);

            if (item == null) return NotFound();

            shoppingCart.Remove(item);

            SaveCartToSession(shoppingCart);

            return RedirectToAction("Index");


        }

        public IActionResult ChangeProductQuantity(int id,int quantity)
        {

            var shoppingCart = GetCartFromSession();
            var cartItem = shoppingCart.FirstOrDefault(S => S.ProductId == id);

            if(cartItem is not null)
            {
                cartItem.Quantity = quantity;

                SaveCartToSession(shoppingCart);

                // Calculate total price for the updated item
                var productTotalPrice = cartItem.Price * cartItem.Quantity;

                // Calculate the overall total price for the cart
                var totalCartPrice = shoppingCart.Sum(item => item.Price * item.Quantity);

                return Json(new { Success = true, TotalPrice = productTotalPrice, OverallTotalPrice = totalCartPrice });
            }
            
            return Json(new { Success = false });
        }


        public IActionResult GetCartCount()
        {
            var shoppingCart = GetCartFromSession();
            int  cartCount = shoppingCart.Sum(S => S.Quantity);
            return Json(new { Success = true, Count = cartCount });
        }

        
        public async Task<IActionResult> OrderSummary()
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

            order.TotalPrice = sessionOrder.Sum(S => S.Quantity * S.Price);

            return View(order);
        }

    }
}
