using ECommerceMVC.Data;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.ViewComponents
{
	public class CartViewComponent : ViewComponent
	{
		private readonly FoodOrderingContext db;

		public CartViewComponent(FoodOrderingContext context)
		{
			db = context;
		}

		public IViewComponentResult Invoke()
		{
			Models.Cart? cart;

			if (UserClaimsPrincipal?.Identity?.IsAuthenticated == true)
			{
				var customerId = int.Parse(UserClaimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
				cart = db.Carts
					.Include(c => c.CartItems)
					.FirstOrDefault(c => c.CustomerId == customerId);
			}
			else
			{
				var sessionId = HttpContext.Session.Id;
				cart = db.Carts
					.Include(c => c.CartItems)
					.FirstOrDefault(c => c.SessionId == sessionId);
			}

			var quantity = cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
			var total = cart?.CartItems.Sum(ci => ci.Quantity * ci.Price) ?? 0;

			return View("CartPanel", new CartModel
			{
				Quantity = quantity,
				Total = total
			});
		}
	}
}
