using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Entities.Models;
using ShoppingCart.Web.Helpers;
using System.Security.Claims;

namespace ShoppingCart.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class UsersController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        public async Task<IActionResult> Index()
        {
            var adminUsers = await _userManager.GetUsersInRoleAsync(Roles.Admin.ToString());

            var nonAdminUsers = _userManager.Users.Where(U => !adminUsers.Contains(U)).ToList();

            #region Retreive none admin user
            //List<ApplicationUser> allUsers = _userManager.Users.ToList();
            //List<ApplicationUser> nonAdminUsers = [];


            //foreach (ApplicationUser user in allUsers)
            //{
            //    if (!await _userManager.IsInRoleAsync(user, Roles.Admin.ToString()))
            //    {
            //        nonAdminUsers.Add(user);
            //    }
            //} 
            #endregion

            return View(nonAdminUsers);
        }

        public async Task<IActionResult> LockUnlock(string id)
        {
            ApplicationUser? user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if(user.LockoutEnd == null || user.LockoutEnd < DateTimeOffset.Now)
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now.AddYears(1));
            }
            else
            {
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now);
            }

            return RedirectToAction("Index", "Users", new { area = "Admin" });
        }
    }
}
