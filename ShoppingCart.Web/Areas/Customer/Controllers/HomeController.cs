using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.ModelVM;
using ShoppingCart.Entities.Repositories;
using System.Security.Claims;

namespace ShoppingCart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<Order> _orderRepo;
        private readonly IGenericRepository<OrderItem> _orderItemRepo;


        public HomeController(
            IGenericRepository<Product> productRepo,
            IGenericRepository<Order> orderRepo,
            IGenericRepository<OrderItem> orderItemRepo)
        {
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _orderItemRepo = orderItemRepo;
        }

        public IActionResult Index(int page = 1)
        {
            int productsPerPage = 8;
            int totalPages = 
                (int)Math.Ceiling((decimal)_productRepo.GetAll().Count() / productsPerPage);

            ViewBag.totalPages = totalPages;
            ViewBag.CurrentPage = page;
            List<Product> products =
                _productRepo.GetAll()
                .Skip((page - 1) * productsPerPage)
                .Take(productsPerPage)
                .ToList();
            return View(products);
        }

        public IActionResult Details(int id)
        {
            Product product = _productRepo.Get(P => P.Id == id,"Category");

            if (product is null)
                return NotFound();


            return View(product);
        }

        public IActionResult CustomerOrders()
        {
            var userId = User.Claims
                .FirstOrDefault(C => C.Type == ClaimTypes.NameIdentifier)?.Value;


            List<CustomerOrderVM> CustomerOrderVM = _orderRepo
                .GetAll(O => O.UserId == userId, includeWord: "Items")
                .Select(O => new CustomerOrderVM
                {
                    Id = O.Id,
                    TotalPrice = O.TotalPrice,
                    OrderStatus = O.OrderStatus,
                    CreatedAt = O.CreatedAt,
                    Items = _orderItemRepo.GetAll(OI => OI.OrderId == O.Id,"Product").ToList(),
                }).ToList();

            return View(CustomerOrderVM);
        }
    }
}
