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
        private readonly IGenericRepository<Category> _categoryRepo;
        private readonly IGenericRepository<Product> _productRepo;
        private readonly IGenericRepository<Order> _orderRepo;

        public HomeController(
            IGenericRepository<Product> productRepo,
            IGenericRepository<Order> orderRepo,
            IGenericRepository<Category> categoryRepo)
        {
            _productRepo = productRepo;
            _orderRepo = orderRepo;
            _categoryRepo = categoryRepo;
        }

        public IActionResult Index(int category, int min, int max, int page = 1)
        {
            int productsPerPage = 8;

            IQueryable<Product> productsQuery = _productRepo.GetAll();
                
			if (category != 0)
            {
				productsQuery = productsQuery.Where(C => C.CategoryId == category);
			}

            if (max != 0 && min != 0)
            {
				productsQuery = productsQuery.Where(C => C.Price >= min && C.Price <= max);
			}

            int totalProducts = productsQuery.Count();
            int totalPages =
                (int)Math.Ceiling((decimal)totalProducts / productsPerPage);

            List<Product> products = productsQuery
                .Skip((page - 1) * productsPerPage)
                .Take(productsPerPage)
                .ToList();

			List<Category> categories = _categoryRepo.GetAll().ToList();

			ViewBag.totalPages = totalPages;
			ViewBag.CurrentPage = page;

			CategoryFilterVM categoryFilter = new CategoryFilterVM()
            {
                Products = products,
                Categories = categories
            };

            return View(categoryFilter);
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
                    Items = O.Items.Select(item => new OrderItemVM
                    {
                        ProductName = item.Product.Name,
                        Price = item.Price,
                        Quantity = item.Quantity
                    }).ToList(),
                }).ToList();

            return View(CustomerOrderVM);
        }
    }
}
