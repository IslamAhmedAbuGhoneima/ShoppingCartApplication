using ShoppingCart.Entities.Models;

namespace ShoppingCart.Entities.ModelVM
{
    public class CustomerOrderVM
    {
        public int Id { get; set; }

        public decimal TotalPrice { get; set; }

        public string OrderStatus { get; set; }

        public DateOnly CreatedAt { get; set; }

        public List<OrderItemVM> Items { get; set; }
    }
}
