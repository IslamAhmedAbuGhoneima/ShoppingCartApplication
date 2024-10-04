using ShoppingCart.DataAccess.Data;
using ShoppingCart.Entities.Models;

namespace ShoppingCart.DataAccess.implementation
{
    public class CategoryRepository : GenericRepository<Category>
    {
        public CategoryRepository(AppDbContext _context)
            : base(_context)
        { }

        
    }
}
