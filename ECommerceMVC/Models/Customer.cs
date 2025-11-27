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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // For sellers - relationship with Restaurant (1-to-1)
        public Restaurant? Restaurant { get; set; }
        
        // For customers - orders and addresses
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<Address> Addresses { get; set; } = new List<Address>();
        public ICollection<Revenue> Revenues { get; set; } = new List<Revenue>();
    }
}
