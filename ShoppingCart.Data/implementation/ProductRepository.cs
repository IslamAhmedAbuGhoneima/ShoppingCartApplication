using ShoppingCart.DataAccess.Data;
using ShoppingCart.Entities.Models;

namespace ShoppingCart.DataAccess.implementation
{
    public class ProductRepository : GenericRepository<Product>
    {
        public ProductRepository(AppDbContext _context) 
            : base(_context) { }
        
    }
}
