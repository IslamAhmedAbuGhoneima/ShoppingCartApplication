using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.ModelVM;
using ShoppingCart.Entities.Repositories;
using ShoppingCart.Web.Helpers;

namespace ShoppingCart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductController : Controller
    {
        private readonly IGenericRepository<Product> productRepo;

        private readonly IGenericRepository<Category> categoryRepo;

        private readonly IWebHostEnvironment webHost;

        public ProductController(
            IGenericRepository<Product> _productRepo,
            IGenericRepository<Category> _categoryRepo,
            IWebHostEnvironment _webHost)
        {
            productRepo = _productRepo;
            categoryRepo = _categoryRepo;
            webHost = _webHost;
        }

        public IActionResult Index()
        {
            return View();
        }


        public IActionResult GetProducts()
        {
            List<Product> products = productRepo.GetAll(includeWord: "Category").ToList();
            return Json(new { data = products });
        }

        [HttpGet]
        public IActionResult Create()
        {
            ProductCategoryVM productCategoryVM = new ProductCategoryVM();
            productCategoryVM.Categories = categoryRepo.GetAll().ToList(); 

            return View(productCategoryVM);
        }


        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductCategoryVM formProduct,IFormFile uploadFile)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    string ImageUrl = Application.ImageUpload(webHost, uploadFile);
                    Product product = new()
                    {
                        Name = formProduct.Name,
                        Description = formProduct.Description,
                        Price = formProduct.Price,
                        ImageUrl = ImageUrl,
                        CategoryId = formProduct.CategoryId,
                    };
                    productRepo.Add(product);
                    productRepo.Save();
                    TempData["Created"] = "Item Created Successfully";
                    return RedirectToAction("Index");
                    
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            formProduct.Categories = categoryRepo.GetAll().ToList();
            return View(formProduct);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Product product = productRepo.Get(C => C.Id == id);
            if (product == null)
            {
                NotFound();
            }

            ProductCategoryVM productCategoryVM = new()
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                Categories = categoryRepo.GetAll().ToList(),
            };

            ViewData["ImageUrl"] = product.ImageUrl;

            return View(productCategoryVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, ProductCategoryVM formProduct, IFormFile? uploadFile)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    Product product = new()
                    {
                        Id = id,
                        Name = formProduct.Name,
                        Description = formProduct.Description,
                        Price = formProduct.Price,
                        CategoryId = formProduct.CategoryId,
                    };

                    if (uploadFile != null)
                    {
                        if (formProduct.ImageUrl != null)
                        {
                            var oldImage = Path.Combine(webHost.WebRootPath, formProduct.ImageUrl.TrimStart('\\'));
                            if (System.IO.File.Exists(oldImage))
                            {
                                System.IO.File.Delete(oldImage);
                            }
                        }
                        string imageUrl = Application.ImageUpload(webHost, uploadFile);
                        product.ImageUrl = imageUrl;
                    }
                    else
                    {
                        product.ImageUrl = formProduct.ImageUrl;
                    }

                    productRepo.Update(product);
                    productRepo.Save();
                    TempData["Updated"] = "Item Updated Successfully";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            formProduct.Categories = categoryRepo.GetAll().ToList();   
            return View(formProduct);
        }

        [HttpDelete]
        public IActionResult DeleteProduct(int id)
        {
            try
            {
                Product product = productRepo.Get(C => C.Id == id);

                productRepo.Remove(product);

                productRepo.Save();

                var oldImage = Path.Combine(webHost.WebRootPath, product.ImageUrl.TrimStart('\\'));
                if (System.IO.File.Exists(oldImage))
                {
                    System.IO.File.Delete(oldImage);
                }
                return Json(new { success = true, message = "Product has been deleted" });

            }
            catch
            {
                return Json(new { success = false, message = "Error while Deleteing product" });
            }
        }
    }
}
