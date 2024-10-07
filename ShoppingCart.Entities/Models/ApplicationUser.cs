using Microsoft.AspNetCore.Identity;

namespace ShoppingCart.Entities.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Address { get; set; }

        public string City { get; set; }
    }
}
