using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class OrderItem
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        
        [Required]
        [ForeignKey(nameof(MenuItem))]
        public int MenuItemId { get; set; }
        public MenuItem? MenuItem { get; set; }
        
        [Required]
        public int Quantity { get; set; } = 1;
        
        [Required]
        public double UnitPrice { get; set; }
        
        [MaxLength(200)]
        public string? Notes { get; set; }
    }
}
