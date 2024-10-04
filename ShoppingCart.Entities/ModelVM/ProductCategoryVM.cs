using Microsoft.AspNetCore.Http;
using ShoppingCart.Entities.Models;
using System.ComponentModel.DataAnnotations;

namespace ShoppingCart.Entities.ModelVM
{
    public class ProductCategoryVM
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Description { get; set; }
        
        [Required(ErrorMessage ="Product must have price")]
        [Range(500,9999999999999999.99,ErrorMessage = $"Value must be between 500 and 9999999999999999.99 ")]
        public decimal Price { get; set; }

        public string? ImageUrl { get; set; }

        [Display(Name = "Category")]
        public int CategoryId { get; set; }


        public List<Category>? Categories { get; set; } = [];
    }
}
