using System.ComponentModel.DataAnnotations;

namespace ECommerceMVC.Models
{
    public class MenuCategory
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;
        
        public ICollection<MenuItem> MenuItems { get; set; } = new List<MenuItem>();
    }
}
