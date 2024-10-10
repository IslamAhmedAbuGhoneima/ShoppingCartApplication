using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Entities.ModelVM
{
    public class OrderSummaryVM
    {
        public List<ShoppingCartVM>? shoppingCart {  get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        [Display(Name="Shipping Address")]
        public string Address { get; set; }

        public string City { get; set; }

        [RegularExpression(@"01[012]\d-\d{4}-\d{3}",
            ErrorMessage ="Invalid Phone number should be xxxx-xxxx-xxx")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public decimal TotalPrice { get; set; }

    }
}
