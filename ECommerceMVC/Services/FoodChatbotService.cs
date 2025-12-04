using ECommerceMVC.Data;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;

namespace ECommerceMVC.Services
{
    public class FoodChatbotService
    {
        private readonly FoodOrderingContext _db;
        private readonly string _apiKey;
        private readonly ILogger<FoodChatbotService> _logger;
        private readonly HttpClient _httpClient;

        public FoodChatbotService(FoodOrderingContext db, IConfiguration configuration, ILogger<FoodChatbotService> logger, IHttpClientFactory httpClientFactory)
        {
            _db = db;
            _apiKey = configuration["CohereSettings:ApiKey"] ?? throw new ArgumentNullException("Cohere API Key not found");
            _logger = logger;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GetFoodRecommendationAsync(string userMessage)
        {
            try
            {
                // Lấy danh sách món ăn từ database
                var availableMenuItems = await _db.MenuItems
                    .Include(m => m.Category)
                    .Where(m => m.IsAvailable)
                    .Select(m => new
                    {
                        m.Name,
                        m.Description,
                        Category = m.Category!.Name,
                        m.Price
                    })
                    .ToListAsync();

                // Tạo context cho Gemini
                string menuContext = "Danh sách món ăn hiện có:\n";
                foreach (var item in availableMenuItems)
                {
                    menuContext += $"- {item.Name} ({item.Category}): {item.Description} - Giá: {item.Price:N0}đ\n";
                }

                // Tạo prompt cho Gemini
                string systemPrompt = @"Bạn là trợ lý ảo thông minh của nhà hàng FoodHub, chuyên tư vấn món ăn cho khách hàng.

NHIỆM VỤ:
- Giúp khách hàng giải quyết câu hỏi ""Hôm nay ăn gì?""
- Gợi ý món ăn phù hợp dựa trên sở thích, tâm trạng, thời tiết, ngân sách
- Trả lời thân thiện, nhiệt tình bằng tiếng Việt
- Chỉ giới thiệu các món ăn có trong menu hiện tại

QUY TẮC:
1. CHỈ giới thiệu món ăn có trong danh sách menu được cung cấp
2. Nếu khách hàng hỏi về món không có trong menu, lịch sự thông báo và gợi ý món tương tự
3. Đề xuất 2-3 món phù hợp với câu hỏi
4. Nêu rõ tên món, giá, mô tả ngắn gọn
5. Giải thích tại sao món đó phù hợp với yêu cầu
6. Hỏi thêm nếu cần thông tin để tư vấn tốt hơn

PHONG CÁCH:
- Thân thiện, gần gũi
- Dùng emoji phù hợp (🍜, 🍕, 🍰, 😊, ✨)
- Câu văn ngắn gọn, dễ đọc
- Tạo cảm giác thoải mái, không gò bó

" + menuContext;

                // Gọi Cohere API bằng HttpClient
                _logger.LogInformation($"Calling Cohere API via HttpClient");
                _logger.LogInformation($"Menu items count: {availableMenuItems.Count}");
                
                var requestBody = new
                {
                    message = userMessage,
                    preamble = systemPrompt,
                    temperature = 0.7
                };
                
                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
                
                _logger.LogInformation($"Sending request to Cohere...");
                var response = await _httpClient.PostAsync("https://api.cohere.ai/v1/chat", content);
                
                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Cohere API error: {response.StatusCode} - {errorContent}");
                    return "Xin lỗi, hệ thống AI tạm thời không khả dụng. Vui lòng thử lại sau! 😊";
                }
                
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation("Cohere API response received");
                
                using var jsonDoc = JsonDocument.Parse(responseBody);
                var text = jsonDoc.RootElement.GetProperty("text").GetString();
                
                if (string.IsNullOrEmpty(text))
                {
                    _logger.LogWarning("Cohere API returned empty text");
                    return "Xin lỗi, tôi không thể đưa ra gợi ý lúc này. Bạn có thể thử lại không? 😊";
                }
                
                _logger.LogInformation($"Response text length: {text.Length}");

                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting food recommendation: {ex.Message}");
                
                // Xử lý lỗi quota
                if (ex.Message.Contains("429") || ex.Message.Contains("quota") || ex.Message.Contains("RESOURCE_EXHAUSTED"))
                {
                    return "Xin lỗi, hệ thống AI tạm thời quá tải. Vui lòng liên hệ nhân viên để được tư vấn trực tiếp nhé! 😊";
                }
                
                // Xử lý lỗi network
                if (ex.Message.Contains("failed") || ex.Message.Contains("timeout"))
                {
                    return "Xin lỗi, kết nối bị gián đoạn. Vui lòng thử lại sau giây lát! 😊";
                }
                
                return "Xin lỗi, đã có lỗi xảy ra. Bạn có thể liên hệ với nhân viên để được tư vấn trực tiếp nhé! 😊";
            }
        }

        public async Task<bool> IsMenuRelatedQuestion(string message)
        {
            // Các từ khóa liên quan đến việc chọn món ăn
            var keywords = new[] {
                "ăn gì", "ăn", "món", "đồ ăn", "thức ăn", "đói", "no",
                "ngon", "đặc sản", "gợi ý", "tư vấn", "giới thiệu",
                "menu", "thực đơn", "danh sách", "có gì", "bán gì",
                "giá", "rẻ", "đắt", "bao nhiêu", "mua",
                "đặt", "order", "giao", "ship"
            };

            string lowerMessage = message.ToLower();
            return keywords.Any(k => lowerMessage.Contains(k));
        }

        public async Task<string> GetGreetingMessage()
        {
            return "Xin chào! 👋 Tôi là trợ lý ảo của FoodHub.\n\n" +
                   "Hôm nay bạn muốn ăn gì? Hãy nói cho tôi biết:\n" +
                   "- Khẩu vị (cay, ngọt, mặn...)\n" +
                   "- Món Việt hay món Âu?\n" +
                   "- Ngân sách dự kiến?\n" +
                   "- Tâm trạng hiện tại?\n\n" +
                   "Tôi sẽ gợi ý món ngon nhất cho bạn! ✨";
        }
    }
}
