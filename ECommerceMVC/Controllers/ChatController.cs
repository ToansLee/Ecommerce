using ECommerceMVC.Data;
using ECommerceMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
    [Authorize]
    public class ChatController : Controller
    {
        private readonly FoodOrderingContext db;

        public ChatController(FoodOrderingContext context)
        {
            db = context;
        }

        // Customer chat with Seller
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var isSeller = User.IsInRole("Seller");

            if (isSeller)
            {
                // Seller sees list of customers who messaged them
                var customers = await db.ChatMessages
                    .Where(m => m.ReceiverId == userId || m.SenderId == userId)
                    .Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                    .Distinct()
                    .ToListAsync();

                var customerList = await db.Customers
                    .Where(c => customers.Contains(c.Id))
                    .ToListAsync();

                return View("SellerChat", customerList);
            }
            else
            {
                // Customer chats with first seller found
                var seller = await db.Customers
                    .FirstOrDefaultAsync(c => c.Role == "Seller");

                if (seller == null)
                {
                    TempData["Error"] = "Không tìm thấy người bán";
                    return RedirectToAction("Index", "Home");
                }

                ViewBag.SellerId = seller.Id;
                ViewBag.SellerName = seller.FullName;
                return View("CustomerChat");
            }
        }

        // Get messages between two users
        [HttpGet]
        public async Task<IActionResult> GetMessages(int otherUserId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var messages = await db.ChatMessages
                .Include(m => m.Sender)
                .Where(m => (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                           (m.SenderId == otherUserId && m.ReceiverId == userId))
                .OrderBy(m => m.SentAt)
                .Select(m => new
                {
                    id = m.Id,
                    senderId = m.SenderId,
                    senderName = m.Sender!.FullName,
                    message = m.Message,
                    sentAt = m.SentAt.ToString("HH:mm dd/MM/yyyy"),
                    isRead = m.IsRead
                })
                .ToListAsync();

            // Mark messages as read
            var unreadMessages = await db.ChatMessages
                .Where(m => m.SenderId == otherUserId && m.ReceiverId == userId && !m.IsRead)
                .ToListAsync();

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
            }

            if (unreadMessages.Any())
            {
                await db.SaveChangesAsync();
            }

            return Json(messages);
        }

        // Send a message
        [HttpPost]
        public async Task<IActionResult> SendMessage(int receiverId, string message)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            if (string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, message = "Tin nhắn không được để trống" });
            }

            var chatMessage = new ChatMessage
            {
                SenderId = userId,
                ReceiverId = receiverId,
                Message = message.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            db.ChatMessages.Add(chatMessage);
            await db.SaveChangesAsync();

            var sender = await db.Customers.FindAsync(userId);

            return Json(new
            {
                success = true,
                data = new
                {
                    id = chatMessage.Id,
                    senderId = chatMessage.SenderId,
                    senderName = sender?.FullName,
                    message = chatMessage.Message,
                    sentAt = chatMessage.SentAt.ToString("HH:mm dd/MM/yyyy")
                }
            });
        }

        // Get unread message count
        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

            var count = await db.ChatMessages
                .Where(m => m.ReceiverId == userId && !m.IsRead)
                .CountAsync();

            return Json(new { count });
        }
    }
}
