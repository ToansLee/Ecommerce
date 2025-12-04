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
        
        public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Status { get; set; } = "Chờ xác nhận";
        
        public Payment? Payment { get; set; }
        
        [MaxLength(500)]
        public string? Notes { get; set; }
        
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
