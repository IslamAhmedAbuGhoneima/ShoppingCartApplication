using ShoppingCart.Entities.Models;

namespace ShoppingCart.Entities.ModelVM
{
    public class CategoryFilterVM
    {
        public List<Product> Products { get; set; } = [];

        public List<Category> Categories { get; set; } = [];
    }
}
