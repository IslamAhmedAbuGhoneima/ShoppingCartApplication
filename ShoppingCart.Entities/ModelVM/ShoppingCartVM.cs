namespace ShoppingCart.Entities.ModelVM
{
    public class ShoppingCartVM
    {
        public int ProductId { get; set; }


        public string Name { get; set; }

        public string Description { get; set; }

        public string ImageUrl { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

    }
}
