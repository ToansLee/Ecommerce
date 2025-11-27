using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ECommerceMVC.Models
{
	public class CartItem
	{
		[Key]
		public int Id { get; set; }

		public int CartId { get; set; }

		public int MenuItemId { get; set; }

		public int Quantity { get; set; }

		public double Price { get; set; } // Store price at the time of adding to cart

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		[ForeignKey("CartId")]
		public Cart Cart { get; set; } = null!;

		[ForeignKey("MenuItemId")]
		public MenuItem MenuItem { get; set; } = null!;
	}
}
