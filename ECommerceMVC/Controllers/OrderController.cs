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

        // Xuất hóa đơn PDF
        public async Task<IActionResult> ExportInvoice(int id)
        {
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var order = await db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Payment)
                .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customerId);

            if (order == null)
            {
                return NotFound();
            }

            // Không cho phép xuất hóa đơn cho đơn hàng bị hủy
            if (order.Status == "Huỷ đơn")
            {
                TempData["ErrorMessage"] = "Không thể xuất hóa đơn cho đơn hàng đã bị hủy!";
                return RedirectToAction("Details", new { id });
            }

            try
            {
                byte[] pdfBytes = Services.InvoiceService.GenerateInvoice(order);
                string fileName = $"HoaDon_{order.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi xuất hóa đơn: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }
    }
}
