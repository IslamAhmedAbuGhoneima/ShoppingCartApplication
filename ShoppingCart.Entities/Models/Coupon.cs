using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Entities.Models
{
	public class Coupon
	{
		public int Id { get; set; }

		public string Code { get; set; }

		public int Discount { get; set; }

		public bool Active { get; set; }

		public ICollection<Order>? Orders { get; set; } = null!;
	}
}
