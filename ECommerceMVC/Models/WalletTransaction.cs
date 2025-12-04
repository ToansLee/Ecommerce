using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class WalletTransaction
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public int CustomerId { get; set; }
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Type { get; set; } = null!; // "Nạp tiền", "Hoàn tiền", "Thanh toán"
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public int? OrderId { get; set; }
        
        public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        
        // Relationships
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; } = null!;
        
        [ForeignKey("OrderId")]
        public Order? Order { get; set; }
    }
}
