using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class Payment
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Method { get; set; } = "VNPay";
        
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
        
        [MaxLength(100)]
        public string? TransactionId { get; set; }
        
        public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        public DateTime? CompletedAt { get; set; }
    }
}
