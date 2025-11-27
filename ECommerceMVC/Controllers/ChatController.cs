using ECommerceMVC.Data;
using ECommerceMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
    [Authorize(Roles = "Customer")]
    public class ChatController : Controller
    {
        private readonly FoodOrderingContext db;

        public ChatController(FoodOrderingContext context)
        {
            db = context;
        }

        // Customer chat with Admin (chỉ dành cho Customer)
        public async Task<IActionResult> Index()
        {
            // Customer chats with Admin
            var admin = await db.Customers
                .FirstOrDefaultAsync(c => c.Role == "Admin");

            if (admin == null)
            {
                TempData["Error"] = "Không tìm thấy quản trị viên. Vui lòng thử lại sau.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.AdminId = admin.Id;
            ViewBag.AdminName = admin.FullName;
            return View("CustomerChat");
        }
    }
}
