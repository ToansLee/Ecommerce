using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace ECommerceMVC.Controllers
{
	public class HomeController : Controller
	{
		private readonly FoodOrderingContext _db;

		public HomeController(FoodOrderingContext db)
		{
			_db = db;
		}

		public IActionResult Index()
		{
		// Admin không được truy cập trang Home - chỉ dành cho Customer mua hàng
		if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
		{
			return RedirectToAction("Index", "Admin");
		}			// Lấy tất cả sản phẩm để hỗ trợ filter theo category
			var menuItems = _db.MenuItems
				.Include(m => m.Category)
				.OrderByDescending(m => m.CreatedAt)
				.Select(p => new HangHoaVM
				{
					MaHh = p.Id,
					TenHH = p.Name,
					DonGia = p.Price,
					Hinh = p.Image ?? "",
					MoTaNgan = p.Description ?? "",
					TenLoai = p.Category != null ? p.Category.Name : "",
					MaLoai = p.CategoryId
				})
				.ToList();

			// Lấy danh sách danh mục
			var categories = _db.MenuCategories.ToList();

			// Truyền dữ liệu qua ViewBag hoặc View Model
			ViewBag.Categories = categories;

			return View(menuItems);
		}

		[Route("/404")]
        public IActionResult PageNotFound()
        {
            return View();
        }

        public IActionResult Privacy()
		{
			return View();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
