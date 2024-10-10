using ShoppingCart.DataAccess.Data;
using ShoppingCart.Entities.Models;

namespace ShoppingCart.DataAccess.implementation
{
    public class OrderItmeRepository : GenericRepository<OrderItem>
    {
        public OrderItmeRepository(AppDbContext _context)
            : base(_context) { }
        
    }
}
