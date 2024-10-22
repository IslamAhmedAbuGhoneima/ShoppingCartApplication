using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Entities.Repositories;
using ShoppingCart.Web.Helpers;

namespace ShoppingCart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class CouponController : Controller
    {
        private readonly IGenericRepository<Coupon> _couponRepo;

        public CouponController(IGenericRepository<Coupon> couponRepo)
        {
            _couponRepo = couponRepo;
        }

        public IActionResult Index()
        {
            List<Coupon> coupons = _couponRepo.GetAll().ToList();
            return View(coupons);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    coupon.Code = coupon.Code.ToUpper();
                    _couponRepo.Add(coupon);
                    _couponRepo.Save();
                    TempData["Created"] = "Coupon Created Successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(coupon);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            Coupon coupon = _couponRepo.Get(C => C.Id == id);

            if (coupon == null)
                return NotFound();

            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id,Coupon couponFromForm)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Coupon coupon = _couponRepo.Get(C => C.Id == id);

                    coupon.Code = couponFromForm.Code;
                    coupon.Discount = couponFromForm.Discount;
                    coupon.Active = couponFromForm.Active;
                    _couponRepo.Update(coupon);
                    _couponRepo.Save();
                    TempData["Updated"] = "Coupon Updated Successfully";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }
            return View(couponFromForm);
        }

        [HttpGet]
        public IActionResult Delete(int id)
        {

            Coupon coupon = _couponRepo.Get(C => C.Id == id);

            if (coupon is null)
            {
                return NotFound();
            }
            return View(coupon);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCoupon(int id)
        {
            
            Coupon coupon = _couponRepo.Get(C => C.Id == id);

            if (coupon is null)
            {
                return NotFound();
            }

            _couponRepo.Remove(coupon);
            _couponRepo.Save();
            TempData["Deleted"] = "item Deleted Successfully";
            return RedirectToAction("Index");
            
        }
    }
}
