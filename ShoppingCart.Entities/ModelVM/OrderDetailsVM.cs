using ShoppingCart.Entities.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShoppingCart.Entities.ModelVM
{
    public class OrderDetailsVM
    {

        #region User Data
        public int Id { get; set; }

        public string UserName { get; set; }

        public string? Email { get; set; }

        public string Address { get; set; }

        public string City { get; set; }

        public string PhoneNumber { get; set; }


        #endregion


        public DateOnly? ShippingDate { get; set; }

        public DateOnly CreatedAt { get; set; }


        public string? OrderStatus { get; set; }

        public string PaymentStatus { get; set; }

        public string? TrackingNumber { get; set; }

        public string? Carrier { get; set; }

        public DateOnly? PaymentDate { get; set; }

        public decimal TotalPrice { get; set; }

        #region Stipe Attribute

        public string? SessionId { get; set; }

        public string? PaymentIntentId { get; set; }

        #endregion

        public List<OrderItem> OrderItems { get; set; } = [];

    }
}
