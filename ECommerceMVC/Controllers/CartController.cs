using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
	public class CartController : Controller
	{
		private readonly FoodOrderingContext db;

		public CartController(FoodOrderingContext context)
		{
			db = context;
		}

		private string GetCartIdentifier()
		{
			if (User.Identity?.IsAuthenticated == true)
			{
				return User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";
			}
			return HttpContext.Session.Id;
		}

		private Models.Cart GetOrCreateCart()
		{
			var identifier = GetCartIdentifier();
			Models.Cart? cart;

			if (User.Identity?.IsAuthenticated == true)
			{
				var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
				cart = db.Carts
					.Include(c => c.CartItems)
					.ThenInclude(ci => ci.MenuItem)
					.FirstOrDefault(c => c.CustomerId == customerId);

				if (cart == null)
				{
					cart = new Models.Cart { CustomerId = customerId };
					db.Carts.Add(cart);
					db.SaveChanges();
				}
			}
			else
			{
				cart = db.Carts
					.Include(c => c.CartItems)
					.ThenInclude(ci => ci.MenuItem)
					.FirstOrDefault(c => c.SessionId == identifier);

				if (cart == null)
				{
					cart = new Models.Cart { SessionId = identifier };
					db.Carts.Add(cart);
					db.SaveChanges();
				}
			}

			return cart;
		}

		public IActionResult Index()
		{
			// Seller kh�ng du?c truy c?p gi? h�ng
			if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
			{
				return RedirectToAction("Index", "Seller");
			}

			var cart = GetOrCreateCart();
			var cartItems = cart.CartItems.Select(ci => new ViewModels.CartItem
			{
				MaHh = ci.MenuItemId,
				TenHH = ci.MenuItem.Name,
				DonGia = ci.Price,
				Hinh = ci.MenuItem.Image ?? "",
				SoLuong = ci.Quantity
			}).ToList();

			return View(cartItems);
		}

		public IActionResult AddToCart(int id, int quantity = 1)
		{
			// Seller kh�ng du?c mua h�ng
			if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
			{
				if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				{
					return Json(new { success = false, message = "Seller không được mua hàng" });
				}
				return RedirectToAction("Index", "Seller");
			}

			var menuItem = db.MenuItems.SingleOrDefault(p => p.Id == id);
			if (menuItem == null)
			{
				if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				{
					return Json(new { success = false, message = $"Không tìm thấy sản phẩm có mã {id}" });
				}
				TempData["Message"] = $"Không tìm thấy menu item có mã {id}";
				return Redirect("/404");
			}

			var cart = GetOrCreateCart();
			var cartItem = cart.CartItems.FirstOrDefault(ci => ci.MenuItemId == id);

			if (cartItem == null)
			{
				cartItem = new Models.CartItem
				{
					CartId = cart.Id,
					MenuItemId = menuItem.Id,
					Quantity = quantity,
					Price = menuItem.Price
				};
				db.CartItems.Add(cartItem);
			}
			else
			{
				cartItem.Quantity += quantity;
				db.CartItems.Update(cartItem);
			}

			cart.UpdatedAt = DateTime.UtcNow;
			db.SaveChanges();

			// Check if this is an AJAX request
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				var totalItems = cart.CartItems.Sum(ci => ci.Quantity);
				return Json(new 
				{ 
					success = true, 
					message = "Đã thêm sản phẩm vào giỏ hàng!",
					cartCount = totalItems
				});
			}

			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult RemoveCart(int id)
		{
			// Seller kh�ng du?c truy c?p gi? h�ng
			if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
			{
				if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				{
					return Json(new { success = false, message = "Seller kh�ng du?c mua h�ng" });
				}
				return RedirectToAction("Index", "Seller");
			}

			var cart = GetOrCreateCart();
			var cartItem = cart.CartItems.FirstOrDefault(ci => ci.MenuItemId == id);
			
			if (cartItem == null)
			{
				if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
				{
					return Json(new { success = false, message = "Kh�ng t�m th?y s?n ph?m trong gi? h�ng" });
				}
				return RedirectToAction("Index");
			}

			db.CartItems.Remove(cartItem);
			cart.UpdatedAt = DateTime.UtcNow;
			db.SaveChanges();

			// Recalculate totals
			var cartTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.Price);
			var cartCount = cart.CartItems.Sum(ci => ci.Quantity);
			var shippingFee = cartTotal > 0 ? (cartTotal >= 500000 ? 0 : 35000) : 0;

			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return Json(new
				{
					success = true,
					message = "�� x�a s?n ph?m kh?i gi? h�ng",
					cartTotal = cartTotal,
					cartCount = cartCount,
					shippingFee = shippingFee
				});
			}
			
			return RedirectToAction("Index");
		}

		[HttpPost]
		public IActionResult UpdateQuantity(int id, int quantity)
		{
			// Seller kh�ng du?c mua h�ng
			if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
			{
				return Json(new { success = false, message = "Seller kh�ng du?c mua h�ng" });
			}

			if (quantity < 1)
			{
				return Json(new { success = false, message = "S? lu?ng ph?i l?n hon 0" });
			}

			var cart = GetOrCreateCart();
			var cartItem = cart.CartItems.FirstOrDefault(ci => ci.MenuItemId == id);

			if (cartItem == null)
			{
				return Json(new { success = false, message = "Kh�ng t�m th?y s?n ph?m trong gi? h�ng" });
			}

			cartItem.Quantity = quantity;
			cart.UpdatedAt = DateTime.UtcNow;
			db.SaveChanges();

			// Recalculate totals
			var cartTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.Price);
			var cartCount = cart.CartItems.Sum(ci => ci.Quantity);
			var shippingFee = cartTotal > 0 ? (cartTotal >= 500000 ? 0 : 35000) : 0;

			return Json(new
			{
				success = true,
				message = "�� c?p nh?t s? lu?ng",
				cartTotal = cartTotal,
				cartCount = cartCount,
				shippingFee = shippingFee
			});
		}
	}
}
