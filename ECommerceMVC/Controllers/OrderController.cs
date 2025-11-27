using ECommerceMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly FoodOrderingContext db;

        public OrderController(FoodOrderingContext context)
        {
            db = context;
        }

        // Customer xem lịch sử đơn hàng
        public IActionResult Index()
        {
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var orders = db.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Payment)
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .ToList();

            return View(orders);
        }

        // Customer xem chi tiết đơn hàng
        public IActionResult Details(int id)
        {
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var order = db.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.Id == id && o.CustomerId == customerId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
