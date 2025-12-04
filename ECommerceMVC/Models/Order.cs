using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class Order
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        [Required]
        public double TotalAmount { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
        
        public Payment? Payment { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
