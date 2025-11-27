using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
    public class Address
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [ForeignKey(nameof(Customer))]
        public int CustomerId { get; set; }
        
        [Required]
        [MaxLength(200)]
        public string Line { get; set; } = null!;
        
        [MaxLength(50)]
        public string? City { get; set; }
        
        [MaxLength(50)]
        public string? District { get; set; }
        
        [MaxLength(20)]
        public string? PostalCode { get; set; }
        
        [MaxLength(20)]
        public string? Phone { get; set; }
        
        public bool IsDefault { get; set; } = false;
        
        public Customer? Customer { get; set; }
    }
}
