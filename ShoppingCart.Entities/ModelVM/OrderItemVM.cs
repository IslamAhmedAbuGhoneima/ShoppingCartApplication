namespace ShoppingCart.Entities.ModelVM
{
	public class OrderItemVM
	{
		public string ProductName { get; set; }

		public decimal Price { get; set; }

		public int Quantity { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }
        
    }
}
