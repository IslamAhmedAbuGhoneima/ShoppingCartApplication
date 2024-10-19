using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Entities.Models
{
	public class Coupon
	{
		[Key]
		public string Code { get; set; }

		public int Discount { get; set; }

		public bool Active { get; set; }

		public ICollection<Order> Orders { get; set; } = null!;
	}
}
