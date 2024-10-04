using Microsoft.EntityFrameworkCore;
using ShoppingCart.Entities.Models;

namespace ShoppingCart.DataAccess.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) :
            base(options)
        { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }

    }
}
