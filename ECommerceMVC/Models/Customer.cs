using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = null!;
        
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = null!;
        
        public bool Gender { get; set; } = true; // true = Male, false = Female
        
        public DateTime? DateOfBirth { get; set; }
        
        [MaxLength(200)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        [Required]
        [MaxLength(255)]
        public string PasswordHash { get; set; } = null!;
        
        [Required]
        [MaxLength(20)]
        public string Role { get; set; } = "Customer";
        
        public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        public bool IsActive { get; set; } = true;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal WalletBalance { get; set; } = 0;
        
        // Customer Tier System
        [MaxLength(20)]
        public string CustomerTier { get; set; } = "Bạc"; // Bạc, Vàng, Kim cương
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal MonthlySpending { get; set; } = 0;
        
        public DateTime LastTierUpdate { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
        
        // Relationships
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
    }
}
