using ECommerceMVC.Data;
using ECommerceMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
	[Authorize(Roles = "Admin")]
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
			// Admin có thể quản lý tất cả sản phẩm và đơn hàng
			var totalMenuItems = await db.MenuItems.CountAsync();
			var totalOrders = await db.Orders.CountAsync();
			var totalRevenue = await db.Orders
				.Where(o => o.Status == "Delivered")
				.SumAsync(o => (double?)o.TotalAmount) ?? 0;

			ViewBag.TotalMenuItems = totalMenuItems;
			ViewBag.TotalOrders = totalOrders;
			ViewBag.TotalRevenue = totalRevenue;
			ViewBag.RestaurantName = "Admin Dashboard";

			return View();
		}

		// Tạo nhà hàng cho seller mới
		[HttpGet]
		// Quản lý món ăn
		public async Task<IActionResult> MenuItems()
		{
			// Admin xem tất cả món ăn
			var menuItems = await db.MenuItems
				.Include(m => m.Category)
				.OrderByDescending(m => m.CreatedAt)
				.ToListAsync();

			return View(menuItems);
		}

		// Thêm món ăn
		[HttpGet]
		public async Task<IActionResult> CreateMenuItem()
		{
			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreateMenuItem(MenuItem model, IFormFile? imageFile)
		{
			if (ModelState.IsValid)
			{
				model.CreatedAt = DateTime.Now;
				// Xử lý upload ảnh
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
