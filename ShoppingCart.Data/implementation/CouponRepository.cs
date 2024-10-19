using ShoppingCart.DataAccess.Data;
using ShoppingCart.Entities.Models;

namespace ShoppingCart.DataAccess.implementation
{
    public class CouponRepository : GenericRepository<Coupon>
    {
        public CouponRepository(AppDbContext _context)
            : base(_context)
        { }

        
    }
}
