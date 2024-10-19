using ShoppingCart.Entities.Models;

namespace ShoppingCart.Entities.ModelVM
{
    public class OrderItemDetailsVM
    {
        public List<OrderItemVM> OrderItems { get; set; }

        public Order Order { get; set; }

    }
}
