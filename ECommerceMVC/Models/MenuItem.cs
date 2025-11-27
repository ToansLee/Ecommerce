using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;
        
        public string? Description { get; set; }
        
        [Required]
        public double Price { get; set; }
        
        [ForeignKey(nameof(Category))]
        public int? CategoryId { get; set; }
        public MenuCategory? Category { get; set; }
        
        public string? Image { get; set; }
        public bool IsAvailable { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
