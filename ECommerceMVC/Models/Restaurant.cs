using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class Restaurant
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = null!;
        
        public string? Description { get; set; }
        
        [MaxLength(200)]
        public string? Address { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        // FK to Seller (Customer with Role=Seller)
        [Required]
        [ForeignKey(nameof(Seller))]
        public int SellerId { get; set; }
        public Customer? Seller { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        
        // Revenue info
        public double TotalRevenue { get; set; } = 0;
        public double AdminCommission { get; set; } = 0;
        
        // Collections
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
