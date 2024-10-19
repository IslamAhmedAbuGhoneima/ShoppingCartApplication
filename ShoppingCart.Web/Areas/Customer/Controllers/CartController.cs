using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.ModelVM;
using ShoppingCart.Entities.Repositories;
using System.Text.Json;

namespace ShoppingCart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    [Authorize]
    public class CartController : Controller
    {

        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<Coupon> _couponRepo;

        public CartController(
            IGenericRepository<Product> productRepo,
            IGenericRepository<Coupon> couponRepo)
        {
            _productRepo = productRepo;
            _couponRepo = couponRepo;
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


        public IActionResult AddToCart(int id, int quantity = 1)
        {
            // Retrieve product from repository
            var product = _productRepo.Get(p => p.Id == id);
            try
            {
                if (product == null) return NotFound();

                var shoppingCart = GetCartFromSession();

                // Check if the product is already in the cart and update quantity
                var cartItem = shoppingCart.FirstOrDefault(item => item.ProductId == id);
                if (cartItem != null)
                {
                    cartItem.Quantity += quantity;
                    if (cartItem.Quantity > product.Stock)
                    {
                        cartItem.Quantity -= quantity;
                        return Json(new { Success = false, Available = product.Stock });
                    }
                }
                else
                {
                    if (quantity > product.Stock)
                    {
                        return Json(new { Success = false, Available = product.Stock });
                    }
                    shoppingCart.Add(new ShoppingCartVM
                    {
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
            catch
            {
                return Json(new { Success = false, Available = product.Stock });
            }
            
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

        public IActionResult ChangeProductQuantity(int id, int quantity)
        {
            var shoppingCart = GetCartFromSession();
            var cartItem = shoppingCart.FirstOrDefault(S => S.ProductId == id);
            var productStock = _productRepo.Get(P => P.Id == id).Stock;
            if(cartItem is not null)
            {
                if(quantity > productStock)
                {
                    return Json(new { Success = false, Available = productStock });
                }
                cartItem.Quantity = quantity;

                SaveCartToSession(shoppingCart);

                // Calculate total price for the updated item
                var productTotalPrice = cartItem.Price * cartItem.Quantity;

                // Calculate the overall total price for the cart
                var totalCartPrice = shoppingCart.Sum(SC => SC.Quantity * SC.Price);

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

        public IActionResult ApplyCoupone(string couponCode)
        {
            try
            {
                couponCode = couponCode.ToUpper();
                Coupon coupon = _couponRepo.Get(C => C.Code == couponCode);
                if (coupon is not null && coupon.Active)
                {
                    HttpContext.Session.SetString("coupon_id", coupon.Code);

                    decimal totalPrice = GetTotalPrice();

                    decimal discount = ((decimal)coupon.Discount / 100) * totalPrice;

                    decimal totalPriceAfterDiscount = totalPrice - discount;

                    HttpContext.Session.SetString("totalPriceAfterDiscount", totalPriceAfterDiscount.ToString());

                    return Json(new {
                        Success = true,
                        Discount = discount,
                        TotalPriceAfterDiscount = totalPriceAfterDiscount
                    });
                }
                else
                {
                    return Json(new { Success = false });
                }
            }
            catch
            {
                return Json(new { Success = false });
            }
        }

        public decimal GetTotalPrice()
        {
            List<ShoppingCartVM> shoppingCart = GetCartFromSession();
            return shoppingCart.Sum(SC => SC.Quantity * SC.Price);
        }

    }
}
