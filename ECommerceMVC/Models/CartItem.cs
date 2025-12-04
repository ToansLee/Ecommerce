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

	[Column(TypeName = "decimal(18,2)")]
	public decimal Price { get; set; } // Store price at the time of adding to cart		public DateTime CreatedAt { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));

		[ForeignKey("CartId")]
		public Cart Cart { get; set; } = null!;

		[ForeignKey("MenuItemId")]
		public MenuItem MenuItem { get; set; } = null!;
	}
}
