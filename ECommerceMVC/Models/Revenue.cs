using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class Revenue
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [ForeignKey(nameof(Seller))]
        public int SellerId { get; set; }
        public Customer? Seller { get; set; }
        
        [Required]
        [ForeignKey(nameof(Order))]
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        
        [Required]
        public double OrderAmount { get; set; }
        
        [Required]
        public double AdminCommission { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Pending";
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedAt { get; set; }
    }
}
