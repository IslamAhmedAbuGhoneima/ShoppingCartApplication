using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.Repositories;

namespace ShoppingCart.Web.Areas.Admin.Controllers
{
    //[Route("Admin/{controller}/{action=Index}", Name = "Category")]
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class CategoryController : Controller
    {
        private readonly IGenericRepository<Category> categoryRepo;

        public CategoryController(IGenericRepository<Category> _categoryRepo)
        {
            categoryRepo = _categoryRepo;
        }

        public IActionResult Index()
        {
            List<Category> categories = categoryRepo.GetAll().ToList();
            return View(categories);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category formCategory)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    categoryRepo.Add(formCategory);
                    categoryRepo.Save();
                    TempData["Created"] = "Item Created Successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(formCategory);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Category category = categoryRepo.Get(C => C.Id == id);
            if (category == null)
            {
                NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, Category formCategory)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    categoryRepo.Update(formCategory);
                    categoryRepo.Save();
                    TempData["Updated"] = "Item Updated Successfully";

                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(formCategory);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {
            Category category = categoryRepo.Get(C => C.Id == id);
            if (category == null)
                return NotFound();

            return View(category);
        }

        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            Category category = categoryRepo.Get(C => C.Id == id);

            if (category is null)
            {
                return NotFound();
            }

            categoryRepo.Remove(category);
            categoryRepo.Save();

            TempData["Deleted"] = "item Deleted Successfully";

            return RedirectToAction("Index");
        }
    }
}
