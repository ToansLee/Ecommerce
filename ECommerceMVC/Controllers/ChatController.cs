using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.Services;
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
        private readonly FoodChatbotService _chatbotService;

        public ChatController(FoodOrderingContext context, FoodChatbotService chatbotService)
        {
            db = context;
            _chatbotService = chatbotService;
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

        // Chatbot page
        public IActionResult Chatbot()
        {
            return View();
        }

        // Chat với AI Chatbot
        [HttpPost]
        public async Task<IActionResult> ChatWithBot([FromBody] ChatBotRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                {
                    return Json(new { success = false, message = "Tin nhắn không được để trống" });
                }

                // Kiểm tra xem có phải câu hỏi về món ăn không
                bool isMenuRelated = await _chatbotService.IsMenuRelatedQuestion(request.Message);

                string response;
                if (isMenuRelated || request.Message.ToLower().Contains("bot") || request.Message.ToLower().Contains("trợ lý"))
                {
                    // Gọi chatbot để trả lời
                    response = await _chatbotService.GetFoodRecommendationAsync(request.Message);
                }
                else
                {
                    // Nếu không liên quan đến món ăn, gợi ý chuyển sang chat với admin
                    response = "Câu hỏi của bạn không liên quan đến việc chọn món ăn. 🤔\n\n" +
                              "Nếu bạn cần hỗ trợ về đơn hàng, thanh toán, hay các vấn đề khác, " +
                              "hãy nhấn nút \"💬 Chat với nhân viên\" để được hỗ trợ trực tiếp nhé! 😊";
                }

                return Json(new { success = true, response = response });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Đã có lỗi xảy ra: " + ex.Message });
            }
        }

        // Lấy lời chào từ bot
        [HttpGet]
        public async Task<IActionResult> GetBotGreeting()
        {
            try
            {
                string greeting = await _chatbotService.GetGreetingMessage();
                return Json(new { success = true, greeting = greeting });
            }
            catch
            {
                return Json(new { success = false });
            }
        }
    }

    public class ChatBotRequest
    {
        public string Message { get; set; } = string.Empty;
    }
}
