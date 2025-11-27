using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceMVC.ViewComponents
{
	public class MenuLoaiViewComponent : ViewComponent
	{
		private readonly FoodOrderingContext db;

		public MenuLoaiViewComponent(FoodOrderingContext context) => db = context;

		public IViewComponentResult Invoke()
		{
			var data = db.MenuCategories.Select(lo => new MenuLoaiVM
			{
				MaLoai = lo.Id,
				TenLoai = lo.Name,
				SoLuong = lo.MenuItems.Count
			}).OrderBy(p => p.TenLoai);

			return View(data); // Default.cshtml
			//return View("Default", data);
		}
	}
}
