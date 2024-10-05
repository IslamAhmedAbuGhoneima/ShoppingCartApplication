using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.Repositories;

namespace ShoppingCart.Web.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly IGenericRepository<Product> productRepo;

        public HomeController(IGenericRepository<Product> productRepo)
        {
            this.productRepo = productRepo;
        }

        public IActionResult Index()
        {
            List<Product> products = productRepo.GetAll().ToList();
            return View(products);
        }

        public IActionResult Details(int id)
        {
            Product product = productRepo.Get(P => P.Id == id,"Category");

            if(product is null)
            {

            }


            return View(product);
        }
    }
}
