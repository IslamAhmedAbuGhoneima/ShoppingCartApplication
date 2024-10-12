using Microsoft.EntityFrameworkCore.Storage;
using ShoppingCart.DataAccess.Data;
using ShoppingCart.Entities.Models;

namespace ShoppingCart.DataAccess.implementation
{
    public class OrderRepository : GenericRepository<Order>
    {
        public OrderRepository(AppDbContext context)
            : base(context) { }

        
    }
}
