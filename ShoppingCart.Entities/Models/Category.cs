using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Entities.Models
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage ="Category Name required")]
        public string Name { get; set; }

        public string Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;


        ICollection<Product> products { get; set; } = [];
    }
}
