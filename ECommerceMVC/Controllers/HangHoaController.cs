using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Controllers
{
    public class HangHoaController : Controller
    {
        private readonly FoodOrderingContext db;

        public HangHoaController(FoodOrderingContext context)
        {
            db = context;
        }

        public IActionResult Index(int? loai, int page = 1, double? minPrice = null, double? maxPrice = null, string? sortBy = null)
        {
            // Seller kh�ng du?c truy c?p trang mua h�ng
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Seller");
            }
            int pageSize = 9;
            var menuItems = db.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsAvailable)
                .AsQueryable();

            if (loai.HasValue)
            {
                menuItems = menuItems.Where(p => p.CategoryId == loai.Value);
            }

            if (minPrice.HasValue)
            {
                menuItems = menuItems.Where(p => p.Price >= (decimal)minPrice.Value);
            }
            if (maxPrice.HasValue)
            {
                menuItems = menuItems.Where(p => p.Price <= (decimal)maxPrice.Value);
            }


            // Sorting
            menuItems = sortBy switch
            {
                "price_asc" => menuItems.OrderBy(p => p.Price),
                "price_desc" => menuItems.OrderByDescending(p => p.Price),
                _ => menuItems.OrderByDescending(p => p.CreatedAt)
            };

            int totalItems = menuItems.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);

            var result = menuItems.Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new HangHoaVM
                {
                    MaHh = p.Id,
                    TenHH = p.Name,
                    DonGia = (double)p.Price,
                    Hinh = p.Image ?? "",
                    MoTaNgan = p.Description ?? "",
                    TenLoai = p.Category != null ? p.Category.Name : ""
                })
                .ToList(); var paginatedData = new PaginatedVM<HangHoaVM>
                {
                    Items = result,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    CategoryId = loai,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    SortBy = sortBy
                };

            return View(paginatedData);
        }

        public IActionResult Search(string? query)
        {
            // Seller kh�ng du?c truy c?p trang mua h�ng
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Seller");
            }

            var menuItems = db.MenuItems
                .Include(m => m.Category)
                .Where(m => m.IsAvailable)
                .AsQueryable();
            if (query != null)
            {
                menuItems = menuItems.Where(p => p.Name.Contains(query));
            }

            var result = menuItems.Select(p => new HangHoaVM
            {
                MaHh = p.Id,
                TenHH = p.Name,
                DonGia = (double)p.Price,
                Hinh = p.Image ?? "",
                MoTaNgan = p.Description ?? "",
                TenLoai = p.Category != null ? p.Category.Name : ""
            });
            return View(result);
        }


        public IActionResult Detail(int id)
        {
            // Seller kh�ng du?c truy c?p trang mua h�ng
            if (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Seller");
            }

            var data = db.MenuItems
                .Include(p => p.Category)
                .SingleOrDefault(p => p.Id == id);
            if (data == null || !data.IsAvailable)
            {
                TempData["Message"] = $"Sản phẩm không khả dụng hoặc đã hết hàng";
                return Redirect("/404");
            }

            // Lấy 4 sản phẩm ngẫu nhiên, không bao gồm sản phẩm hiện tại
            var relatedProducts = db.MenuItems
                .Include(p => p.Category)
                .Where(p => p.Id != id && p.IsAvailable)
                .OrderBy(x => Guid.NewGuid())
                .Take(4)
                .Select(p => new HangHoaVM
                {
                    MaHh = p.Id,
                    TenHH = p.Name,
                    DonGia = (double)p.Price,
                    Hinh = p.Image ?? "",
                    MoTaNgan = p.Description ?? "",
                    TenLoai = p.Category != null ? p.Category.Name : ""
                })
                .ToList();

            var result = new ChiTietHangHoaVM
            {
                MaHh = data.Id,
                TenHH = data.Name,
                DonGia = (double)data.Price,
                ChiTiet = data.Description ?? string.Empty,
                Hinh = data.Image ?? string.Empty,
                MoTaNgan = data.Description ?? string.Empty,
                TenLoai = data.Category != null ? data.Category.Name : "",
                SoLuongTon = 10,//t�nh sau
                DiemDanhGia = 5,//check sau
                RelatedProducts = relatedProducts
            };
            return View(result);
        }
    }
}
