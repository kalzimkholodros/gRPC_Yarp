using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BasketService.Models
{
    public class Basket
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string? UserId { get; set; }
        public List<BasketItem> Items { get; set; } = new List<BasketItem>();
    }
} 