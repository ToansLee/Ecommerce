using ECommerceMVC.Data;
using ECommerceMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
	[Authorize(Roles = "Seller")]
	public class SellerController : Controller
	{
		private readonly FoodOrderingContext db;

		public SellerController(FoodOrderingContext context)
		{
			db = context;
		}

		// Dashboard
		public async Task<IActionResult> Index()
		{
			var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var restaurant = await db.Restaurants
				.Include(r => r.MenuItems)
				.Include(r => r.Orders)
				.FirstOrDefaultAsync(r => r.SellerId == sellerId);

			if (restaurant == null)
			{
				return RedirectToAction("CreateRestaurant");
			}

			ViewBag.TotalMenuItems = restaurant.MenuItems.Count;
			ViewBag.TotalOrders = restaurant.Orders.Count;
			ViewBag.TotalRevenue = restaurant.TotalRevenue;
			ViewBag.RestaurantName = restaurant.Name;

			return View(restaurant);
		}

		// Tạo nhà hàng cho seller mới
		[HttpGet]
		public IActionResult CreateRestaurant()
		{
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreateRestaurant(Restaurant model)
		{
			if (ModelState.IsValid)
			{
				var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
				
				// Kiểm tra seller đã có restaurant chưa
				if (await db.Restaurants.AnyAsync(r => r.SellerId == sellerId))
				{
					ModelState.AddModelError("", "Bạn đã có nhà hàng rồi");
					return View(model);
				}

				model.SellerId = sellerId;
				model.CreatedAt = DateTime.Now;
				model.IsActive = true;

				db.Restaurants.Add(model);
				await db.SaveChangesAsync();
				
				return RedirectToAction("Index");
			}
			return View(model);
		}

		// Quản lý món ăn
		public async Task<IActionResult> MenuItems()
		{
			var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var restaurant = await db.Restaurants
				.Include(r => r.MenuItems)
				.ThenInclude(m => m.Category)
				.FirstOrDefaultAsync(r => r.SellerId == sellerId);

			if (restaurant == null)
			{
				return RedirectToAction("CreateRestaurant");
			}

			return View(restaurant.MenuItems.OrderByDescending(m => m.CreatedAt).ToList());
		}

		// Thêm món ăn
		[HttpGet]
		public async Task<IActionResult> CreateMenuItem()
		{
			var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var restaurant = await db.Restaurants.FirstOrDefaultAsync(r => r.SellerId == sellerId);

			if (restaurant == null)
			{
				return RedirectToAction("CreateRestaurant");
			}

			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			ViewBag.RestaurantId = restaurant.Id;
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreateMenuItem(MenuItem model, IFormFile? imageFile)
		{
			var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var restaurant = await db.Restaurants.FirstOrDefaultAsync(r => r.SellerId == sellerId);

			if (restaurant == null)
			{
				return RedirectToAction("CreateRestaurant");
			}

		if (ModelState.IsValid)
		{
			model.CreatedAt = DateTime.Now;				// Xử lý upload ảnh
				if (imageFile != null)
				{
					var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(imageFile.FileName);
					var categoryFolder = model.CategoryId switch
					{
						1 => "Khai_vi",
						2 => "Mon_chinh",
						3 => "Nuoc_uong",
						4 => "Trang_mieng",
						_ => "Others"
					};
					var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", categoryFolder, fileName);
					
					Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
					
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await imageFile.CopyToAsync(stream);
					}
					
					model.Image = $"{categoryFolder}/{fileName}";
				}

				db.MenuItems.Add(model);
				await db.SaveChangesAsync();
				
				return RedirectToAction("MenuItems");
			}

			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			ViewBag.RestaurantId = restaurant.Id;
			return View(model);
		}

		// Sửa món ăn
		[HttpGet]
		public async Task<IActionResult> EditMenuItem(int id)
		{
			var menuItem = await db.MenuItems
				.FirstOrDefaultAsync(m => m.Id == id);

			if (menuItem == null)
			{
				return NotFound();
			}

			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			return View(menuItem);
		}

		[HttpPost]
		public async Task<IActionResult> EditMenuItem(MenuItem model, IFormFile? imageFile)
		{
			var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var restaurant = await db.Restaurants.FirstOrDefaultAsync(r => r.SellerId == sellerId);

			if (restaurant == null)
			{
				return RedirectToAction("CreateRestaurant");
			}

			var menuItem = await db.MenuItems
				.FirstOrDefaultAsync(m => m.Id == model.Id);

			if (menuItem == null)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				menuItem.Name = model.Name;
				menuItem.Description = model.Description;
				menuItem.Price = model.Price;
				menuItem.CategoryId = model.CategoryId;
				menuItem.IsAvailable = model.IsAvailable;

				// Xử lý upload ảnh mới
				if (imageFile != null)
				{
					// Xóa ảnh cũ
					if (!string.IsNullOrEmpty(menuItem.Image))
					{
						var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", menuItem.Image);
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(imageFile.FileName);
					var categoryFolder = model.CategoryId switch
					{
						1 => "Khai_vi",
						2 => "Mon_chinh",
						3 => "Nuoc_uong",
						4 => "Trang_mieng",
						_ => "Others"
					};
					var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", categoryFolder, fileName);
					
					Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
					
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await imageFile.CopyToAsync(stream);
					}
					
					menuItem.Image = $"{categoryFolder}/{fileName}";
				}

				await db.SaveChangesAsync();
				return RedirectToAction("MenuItems");
			}

			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			return View(model);
		}

		// Xóa món ăn
		[HttpPost]
		public async Task<IActionResult> DeleteMenuItem(int id)
		{
			var menuItem = await db.MenuItems
				.FirstOrDefaultAsync(m => m.Id == id);

			if (menuItem == null)
			{
				return Json(new { success = false, message = "Không tìm thấy món ăn" });
			}

			// Xóa ảnh
			if (!string.IsNullOrEmpty(menuItem.Image))
			{
				var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", menuItem.Image);
				if (System.IO.File.Exists(imagePath))
				{
					System.IO.File.Delete(imagePath);
				}
			}

			db.MenuItems.Remove(menuItem);
			await db.SaveChangesAsync();

			return Json(new { success = true, message = "Đã xóa món ăn thành công" });
		}

		// Doanh thu
		public async Task<IActionResult> Revenue()
		{
			var sellerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
			var restaurant = await db.Restaurants
				.Include(r => r.Orders)
				.FirstOrDefaultAsync(r => r.SellerId == sellerId);

			if (restaurant == null)
			{
				return RedirectToAction("CreateRestaurant");
			}

			var revenues = await db.Revenues
				.Include(r => r.Order)
				.Where(r => r.SellerId == sellerId)
				.OrderByDescending(r => r.CreatedAt)
				.ToListAsync();

			ViewBag.RestaurantName = restaurant.Name;
			ViewBag.TotalRevenue = restaurant.TotalRevenue;
			return View(revenues);
		}

		// Quản lý đơn hàng
		public async Task<IActionResult> Orders()
		{
			var orders = await db.Orders
				.Include(o => o.Customer)
				.Include(o => o.Items)
				.ThenInclude(oi => oi.MenuItem)
				.Include(o => o.Payment)
				.OrderByDescending(o => o.CreatedAt)
				.ToListAsync();

			return View(orders);
		}

		// Xem chi tiết đơn hàng
		public async Task<IActionResult> OrderDetails(int id)
		{
			var order = await db.Orders
				.Include(o => o.Customer)
				.Include(o => o.Items)
				.ThenInclude(oi => oi.MenuItem)
				.Include(o => o.Payment)
				.FirstOrDefaultAsync(o => o.Id == id);

			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}

		// Cập nhật trạng thái đơn hàng
		[HttpPost]
		public async Task<IActionResult> UpdateOrderStatus(int id, string status)
		{
			var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);

			if (order == null)
			{
				return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
			}

			order.Status = status;
			await db.SaveChangesAsync();

			return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
		}
	}
}
