using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
	public class AdminController : Controller
	{
		private readonly FoodOrderingContext db;

		public AdminController(FoodOrderingContext context)
		{
			db = context;
		}

		// Action để tạo tài khoản admin mặc định
		[HttpGet]
		public async Task<IActionResult> CreateDefaultAdmin()
		{
			try
			{
				// Kiểm tra xem đã có admin chưa
				var existingAdmin = await db.Customers.FirstOrDefaultAsync(c => c.Role == "Admin");
				
				if (existingAdmin != null)
				{
					return Content($"Admin đã tồn tại: Username = {existingAdmin.UserName}, Email = {existingAdmin.Email}");
				}

				// Tạo admin mới
				var admin = new Customer
				{
					UserName = "admin",
					Email = "admin@foodhub.com",
					FullName = "Administrator",
					PasswordHash = "admin123".ToMd5Hash(null!),
					Role = "Admin",
					Gender = true,
					Phone = "0123456789",
					Address = "218 Lĩnh Nam, Hoàng Mai, Hà Nội",
					CreatedAt = DateTime.Now,
					IsActive = true
				};

				db.Customers.Add(admin);
				await db.SaveChangesAsync();

				return Content("Tạo tài khoản Admin thành công!\n\nUsername: admin\nPassword: admin123\n\nVui lòng đăng nhập tại: /KhachHang/DangNhap");
			}
			catch (Exception ex)
			{
				return Content($"Lỗi: {ex.Message}");
			}
		}
	}
}
