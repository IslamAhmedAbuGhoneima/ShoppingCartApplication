using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCart.Entities.Models
{
    public class Order
    {

        public int Id { get; set; }

        #region User Data

        public string UserName { get; set; }

        public string Email { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string PhoneNumber { get; set; }

        public decimal TotalPrice { get; set; }

        #endregion

        public DateOnly CreatedAt { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

        public DateOnly? ShippingDate { get; set; }

        public string OrderStatus { get; set; }

        public string PaymentStatus { get; set; }

        public string? TrackingNumber { get; set; }

        public string? Carrier { get; set; }

        public DateOnly? PaymentDate { get; set; }


        #region Stipe Attribute

        public string? SessionId { get; set; }

        public string? PaymentIntentId { get; set; }

        #endregion

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; }

        ApplicationUser? ApplicationUser { get; set; }

        public ICollection<OrderItem> Items { get; set; } = null!;

        [ForeignKey("Coupon")]
        public string? CouponCode { get; set; }

        public int? Discount { get; set; }

        public Coupon? Coupon { get; set; }
    }
}
