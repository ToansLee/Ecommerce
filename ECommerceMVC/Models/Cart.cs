using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
	public class Cart
	{
		[Key]
		public int Id { get; set; }

		public int? CustomerId { get; set; }

		[MaxLength(100)]
		public string? SessionId { get; set; } // For guests (not logged in)

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

		[ForeignKey("CustomerId")]
		public Customer? Customer { get; set; }

		public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
	}
}
