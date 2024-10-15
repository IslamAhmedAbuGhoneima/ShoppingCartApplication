using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.Repositories;
using ShoppingCart.Web.Helpers;

namespace ShoppingCart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class DashboardController : Controller
    {
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly UserManager<ApplicationUser> _userManager;

        public DashboardController(
            IGenericRepository<Order> orderRepo,
            IGenericRepository<Product> productRepo,
            UserManager<ApplicationUser> userManager
            )
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            ViewBag.Orders = _orderRepo.GetAll().Count();
            ViewBag.ApprovedOrders = _orderRepo.GetAll(O => O.OrderStatus == OrderStatus.Approved.ToString()).Count();
            ViewBag.Users = _userManager.Users.Count();
            ViewBag.Products = _productRepo.GetAll().Count();
                
            return View();
        }
    }
}
