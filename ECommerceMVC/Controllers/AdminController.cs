using ECommerceMVC.Data;
using ECommerceMVC.Helpers;
using ECommerceMVC.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
	[Authorize(Roles = "Admin")]
	public class AdminController : Controller
	{
		private readonly FoodOrderingContext db;
		private readonly CustomerTierService _tierService;

		public AdminController(FoodOrderingContext context, CustomerTierService tierService)
		{
			db = context;
			_tierService = tierService;
		}

	// Dashboard
	public async Task<IActionResult> Index()
	{
		// Admin có thể quản lý tất cả sản phẩm và đơn hàng
		var totalMenuItems = await db.MenuItems.CountAsync();
		var totalOrders = await db.Orders.CountAsync();
		var totalRevenue = await db.Orders
			.Where(o => o.Status == "Hoàn thành")
			.SumAsync(o => (double?)o.TotalAmount) ?? 0;
		var totalCustomers = await db.Customers.CountAsync(c => c.Role == "Customer");

		ViewBag.TotalMenuItems = totalMenuItems;
		ViewBag.TotalOrders = totalOrders;
		ViewBag.TotalRevenue = totalRevenue;
		ViewBag.TotalCustomers = totalCustomers;
		ViewBag.RestaurantName = "Admin Dashboard";

		return View();
	}		// Quản lý món ăn
		public async Task<IActionResult> MenuItems(string? search, int? categoryId, bool? isAvailable, string? sortBy, int page = 1)
		{
			int pageSize = 10;
			
			// Base query
			var query = db.MenuItems.Include(m => m.Category).AsQueryable();

			// Search
			if (!string.IsNullOrEmpty(search))
			{
				query = query.Where(m => m.Name.Contains(search) || (m.Description != null && m.Description.Contains(search)));
			}

			// Filter by category
			if (categoryId.HasValue && categoryId.Value > 0)
			{
				query = query.Where(m => m.CategoryId == categoryId.Value);
			}

			// Filter by availability
			if (isAvailable.HasValue)
			{
				query = query.Where(m => m.IsAvailable == isAvailable.Value);
			}

			// Sort
			query = sortBy switch
			{
				"name" => query.OrderBy(m => m.Name),
				"name_desc" => query.OrderByDescending(m => m.Name),
				"price" => query.OrderBy(m => m.Price),
				"price_desc" => query.OrderByDescending(m => m.Price),
				"category" => query.OrderBy(m => m.Category!.Name),
				"date" => query.OrderBy(m => m.CreatedAt),
				_ => query.OrderByDescending(m => m.CreatedAt)
			};

			// Pagination
			var totalItems = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
			
			var menuItems = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Statistics
			ViewBag.TotalItems = totalItems;
			ViewBag.TotalAvailable = await db.MenuItems.CountAsync(m => m.IsAvailable);
			ViewBag.TotalUnavailable = await db.MenuItems.CountAsync(m => !m.IsAvailable);
			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			ViewBag.CategoryStats = await db.MenuItems
				.Where(m => m.Category != null)
				.GroupBy(m => m.Category!.Name)
				.Select(g => new { Category = g.Key, Count = g.Count() })
				.ToListAsync();

			// Pagination data
			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.Search = search;
			ViewBag.CategoryId = categoryId;
			ViewBag.IsAvailable = isAvailable;
			ViewBag.SortBy = sortBy;

			return View(menuItems);
		}

		// Toggle availability
		[HttpPost]
		public async Task<IActionResult> ToggleAvailability(int id)
		{
			var menuItem = await db.MenuItems.FindAsync(id);
			if (menuItem == null)
			{
				return Json(new { success = false, message = "Không tìm thấy món ăn" });
			}

			menuItem.IsAvailable = !menuItem.IsAvailable;
			await db.SaveChangesAsync();

			return Json(new { 
				success = true, 
				message = menuItem.IsAvailable ? "Đã bật món ăn" : "Đã tắt món ăn",
				isAvailable = menuItem.IsAvailable 
			});
		}

		// Thêm món ăn
		[HttpGet]
		public async Task<IActionResult> CreateMenuItem()
		{
			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> CreateMenuItem(MenuItem model, IFormFile? imageFile)
		{
			if (ModelState.IsValid)
			{
				model.CreatedAt = DateTime.Now;
				// Xử lý upload ảnh
				if (imageFile != null)
				{
					var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(imageFile.FileName);
					var categoryFolder = model.CategoryId switch
					{
						1 => "Khai_vi",
						2 => "Mon_chinh",
						3 => "Nuoc_uong",
						4 => "Trang_mieng",
						_ => "Others"
					};
					var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", categoryFolder, fileName);
					
					Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
					
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await imageFile.CopyToAsync(stream);
					}
					
					model.Image = $"{categoryFolder}/{fileName}";
				}

				db.MenuItems.Add(model);
				await db.SaveChangesAsync();
				
				return RedirectToAction("MenuItems");
			}

			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			return View(model);
		}

		// Sửa món ăn
		[HttpGet]
		public async Task<IActionResult> EditMenuItem(int id)
		{
			var menuItem = await db.MenuItems
				.FirstOrDefaultAsync(m => m.Id == id);

			if (menuItem == null)
			{
				return NotFound();
			}

			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			return View(menuItem);
		}

		[HttpPost]
		public async Task<IActionResult> EditMenuItem(MenuItem model, IFormFile? imageFile)
		{
			var menuItem = await db.MenuItems
				.FirstOrDefaultAsync(m => m.Id == model.Id);

			if (menuItem == null)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				menuItem.Name = model.Name;
				menuItem.Description = model.Description;
				menuItem.Price = model.Price;
				menuItem.CategoryId = model.CategoryId;
				menuItem.IsAvailable = model.IsAvailable;

				// Xử lý upload ảnh mới
				if (imageFile != null)
				{
					// Xóa ảnh cũ
					if (!string.IsNullOrEmpty(menuItem.Image))
					{
						var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", menuItem.Image);
						if (System.IO.File.Exists(oldImagePath))
						{
							System.IO.File.Delete(oldImagePath);
						}
					}

					var fileName = DateTime.Now.Ticks.ToString() + Path.GetExtension(imageFile.FileName);
					var categoryFolder = model.CategoryId switch
					{
						1 => "Khai_vi",
						2 => "Mon_chinh",
						3 => "Nuoc_uong",
						4 => "Trang_mieng",
						_ => "Others"
					};
					var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", categoryFolder, fileName);
					
					Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);
					
					using (var stream = new FileStream(filePath, FileMode.Create))
					{
						await imageFile.CopyToAsync(stream);
					}
					
					menuItem.Image = $"{categoryFolder}/{fileName}";
				}

				await db.SaveChangesAsync();
				return RedirectToAction("MenuItems");
			}

			ViewBag.Categories = await db.MenuCategories.ToListAsync();
			return View(model);
		}

		// Xóa món ăn
		[HttpPost]
		public async Task<IActionResult> DeleteMenuItem(int id)
		{
			var menuItem = await db.MenuItems
				.FirstOrDefaultAsync(m => m.Id == id);

			if (menuItem == null)
			{
				return Json(new { success = false, message = "Không tìm thấy món ăn" });
			}

			// Xóa ảnh
			if (!string.IsNullOrEmpty(menuItem.Image))
			{
				var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "Hinh", "HangHoa", menuItem.Image);
				if (System.IO.File.Exists(imagePath))
				{
					System.IO.File.Delete(imagePath);
				}
			}

			db.MenuItems.Remove(menuItem);
			await db.SaveChangesAsync();

			return Json(new { success = true, message = "Đã xóa món ăn thành công" });
		}

		// Quản lý đơn hàng
		public async Task<IActionResult> Orders(string? search, string? status, DateTime? fromDate, DateTime? toDate, string? sortBy, int page = 1)
		{
			int pageSize = 10;

			// Base query
			var query = db.Orders
				.Include(o => o.Customer)
				.Include(o => o.Items)
				.ThenInclude(oi => oi.MenuItem)
				.Include(o => o.Payment)
				.AsQueryable();

			// Search by order ID or customer name
			if (!string.IsNullOrEmpty(search))
			{
				if (int.TryParse(search, out int orderId))
				{
					query = query.Where(o => o.Id == orderId);
				}
				else
				{
					query = query.Where(o => o.Customer != null && o.Customer.FullName.Contains(search));
				}
			}

			// Filter by status
			if (!string.IsNullOrEmpty(status) && status != "All")
			{
				query = query.Where(o => o.Status == status);
			}

			// Filter by date range
			if (fromDate.HasValue)
			{
				query = query.Where(o => o.CreatedAt.Date >= fromDate.Value.Date);
			}
			if (toDate.HasValue)
			{
				query = query.Where(o => o.CreatedAt.Date <= toDate.Value.Date);
			}

			// Sort
			query = sortBy switch
			{
				"id" => query.OrderBy(o => o.Id),
				"id_desc" => query.OrderByDescending(o => o.Id),
				"customer" => query.OrderBy(o => o.Customer!.FullName),
				"customer_desc" => query.OrderByDescending(o => o.Customer!.FullName),
				"total" => query.OrderBy(o => o.TotalAmount),
				"total_desc" => query.OrderByDescending(o => o.TotalAmount),
				"date" => query.OrderBy(o => o.CreatedAt),
				_ => query.OrderByDescending(o => o.CreatedAt)
			};

			// Pagination
			var totalOrders = await query.CountAsync();
			var totalPages = (int)Math.Ceiling(totalOrders / (double)pageSize);

			var orders = await query
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			// Statistics
			var allOrders = db.Orders.AsQueryable();
			
			// Apply same filters for statistics
			if (!string.IsNullOrEmpty(search))
			{
				if (int.TryParse(search, out int orderId))
				{
					allOrders = allOrders.Where(o => o.Id == orderId);
				}
				else
				{
					allOrders = allOrders.Where(o => o.Customer != null && o.Customer.FullName.Contains(search));
				}
			}
			if (fromDate.HasValue)
			{
				allOrders = allOrders.Where(o => o.CreatedAt.Date >= fromDate.Value.Date);
			}
			if (toDate.HasValue)
			{
				allOrders = allOrders.Where(o => o.CreatedAt.Date <= toDate.Value.Date);
			}

		ViewBag.TotalOrders = totalOrders;
		ViewBag.PendingOrders = await allOrders.CountAsync(o => o.Status == "Chờ xác nhận");
		ViewBag.PreparingOrders = await allOrders.CountAsync(o => o.Status == "Chuẩn bị món");
		ViewBag.DeliveringOrders = await allOrders.CountAsync(o => o.Status == "Đang giao");
		ViewBag.DeliveredOrders = await allOrders.CountAsync(o => o.Status == "Hoàn thành");
		ViewBag.CancelledOrders = await allOrders.CountAsync(o => o.Status == "Huỷ đơn");
		ViewBag.TotalRevenue = await allOrders
			.Where(o => o.Status == "Hoàn thành")
			.SumAsync(o => (double?)o.TotalAmount) ?? 0;			// Pagination data
			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.Search = search;
			ViewBag.Status = status;
			ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
			ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
			ViewBag.SortBy = sortBy;

			return View(orders);
		}

		// Xem chi tiết đơn hàng
		public async Task<IActionResult> OrderDetails(int id)
		{
			var order = await db.Orders
				.Include(o => o.Customer)
				.Include(o => o.Items)
				.ThenInclude(oi => oi.MenuItem)
				.Include(o => o.Payment)
				.FirstOrDefaultAsync(o => o.Id == id);

			if (order == null)
			{
				return NotFound();
			}

			return View(order);
		}

	// Cập nhật trạng thái đơn hàng
	[HttpPost]
	public async Task<IActionResult> UpdateOrderStatus(int id, string status)
	{
		var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == id);

		if (order == null)
		{
			return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
		}

		order.Status = status;
		await db.SaveChangesAsync();

		// Cập nhật hạng thành viên khi đơn hàng hoàn thành
		if (status == "Hoàn thành")
		{
			await _tierService.UpdateCustomerTier(order.CustomerId);
		}

		return Json(new { success = true, message = "Cập nhật trạng thái thành công" });
	}		// Xuất hóa đơn PDF
		public async Task<IActionResult> ExportInvoice(int id)
		{
			var order = await db.Orders
				.Include(o => o.Customer)
				.Include(o => o.Items)
					.ThenInclude(oi => oi.MenuItem)
				.Include(o => o.Payment)
				.FirstOrDefaultAsync(o => o.Id == id);

			if (order == null)
			{
				return NotFound();
			}

			// Không cho phép xuất hóa đơn cho đơn hàng bị hủy
			if (order.Status == "Huỷ đơn")
			{
				TempData["ErrorMessage"] = "Không thể xuất hóa đơn cho đơn hàng đã bị hủy!";
				return RedirectToAction("OrderDetails", new { id });
			}

			try
			{
				byte[] pdfBytes = Services.InvoiceService.GenerateInvoice(order);
				string fileName = $"Invoice_{order.Id}_{DateTime.Now:yyyyMMddHHmmss}.pdf";
				
				return File(pdfBytes, "application/pdf", fileName);
			}
			catch (Exception ex)
			{
				TempData["ErrorMessage"] = $"Lỗi khi xuất hóa đơn: {ex.Message}";
				return RedirectToAction("OrderDetails", new { id });
			}
		}

		// Hủy đơn hàng
		[HttpPost]
		public async Task<IActionResult> CancelOrder(int id, string? reason)
		{
			var order = await db.Orders
				.Include(o => o.Payment)
				.Include(o => o.Customer)
				.FirstOrDefaultAsync(o => o.Id == id);

			if (order == null)
			{
				return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
			}

			if (order.Status == "Hoàn thành")
			{
				return Json(new { success = false, message = "Không thể hủy đơn hàng đã hoàn thành" });
			}

			if (order.Status == "Huỷ đơn")
			{
				return Json(new { success = false, message = "Đơn hàng đã bị hủy trước đó" });
			}

			// Hoàn tiền vào ví nếu đã thanh toán VNPay
			if (order.Payment != null && order.Payment.Method == "VNPay" && order.Payment.Status == "Completed" && order.Customer != null)
			{
				order.Customer.WalletBalance += order.Payment.Amount;
				
				var refundTransaction = new WalletTransaction
				{
					CustomerId = order.CustomerId,
					Amount = order.Payment.Amount,
					Type = "Hoàn tiền",
					Description = $"Hoàn tiền đơn hàng #{order.Id} bị hủy",
					OrderId = order.Id
				};
				db.WalletTransactions.Add(refundTransaction);
			}

			// Hoàn tiền từ ví đã sử dụng
			var walletPayment = await db.WalletTransactions
				.FirstOrDefaultAsync(w => w.OrderId == id && w.Type == "Thanh toán");
			if (walletPayment != null && order.Customer != null)
			{
				order.Customer.WalletBalance += Math.Abs(walletPayment.Amount);
				
				var refundTransaction = new WalletTransaction
				{
					CustomerId = order.CustomerId,
					Amount = Math.Abs(walletPayment.Amount),
					Type = "Hoàn tiền",
					Description = $"Hoàn tiền ví từ đơn hàng #{order.Id} bị hủy",
					OrderId = order.Id
				};
				db.WalletTransactions.Add(refundTransaction);
			}

			order.Status = "Huỷ đơn";
			if (!string.IsNullOrEmpty(reason))
			{
				order.Notes = (order.Notes ?? "") + "\n[Lý do hủy: " + reason + "]";
			}
			await db.SaveChangesAsync();

			return Json(new { success = true, message = "Đã hủy đơn hàng và hoàn tiền thành công" });
		}

		// Xóa đơn hàng (chỉ cho admin)
		[HttpPost]
		public async Task<IActionResult> DeleteOrder(int id)
		{
			var order = await db.Orders
				.Include(o => o.Items)
				.Include(o => o.Payment)
				.FirstOrDefaultAsync(o => o.Id == id);

			if (order == null)
			{
				return Json(new { success = false, message = "Không tìm thấy đơn hàng" });
			}

		// Chỉ cho phép xóa đơn hàng đã hủy hoặc đã hoàn thành lâu
		if (order.Status != "Huỷ đơn" && (order.Status != "Hoàn thành" || order.CreatedAt > DateTime.UtcNow.AddDays(-30)))
		{
			return Json(new { success = false, message = "Chỉ có thể xóa đơn hàng đã hủy hoặc đơn hàng đã hoàn thành hơn 30 ngày" });
		}			db.Orders.Remove(order);
			await db.SaveChangesAsync();

			return Json(new { success = true, message = "Đã xóa đơn hàng thành công" });
		}

		// Chat management - Admin chat with customers
		public async Task<IActionResult> AdminChat()
		{
			var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

			// Admin sees list of customers who messaged them
			var customers = await db.ChatMessages
				.Where(m => m.ReceiverId == userId || m.SenderId == userId)
				.Select(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
				.Distinct()
				.ToListAsync();

			var customerList = await db.Customers
				.Where(c => customers.Contains(c.Id) && c.Role == "Customer")
				.OrderByDescending(c => db.ChatMessages
					.Where(msg => (msg.SenderId == c.Id && msg.ReceiverId == userId) || 
								 (msg.SenderId == userId && msg.ReceiverId == c.Id))
					.Max(msg => msg.SentAt))
				.ToListAsync();

			return View(customerList);
		}

		// Get messages between two users
		[AllowAnonymous]
		[Authorize] // Cả Customer và Admin đều có thể gọi
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
		[AllowAnonymous]
		[Authorize] // Cả Customer và Admin đều có thể gọi
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

		// Action để tạo tài khoản admin mặc định
		[AllowAnonymous]
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

		// ==================== QUẢN LÝ KHÁCH HÀNG ====================

		// Danh sách khách hàng
		public async Task<IActionResult> Customers(string searchString, string status, int page = 1)
		{
			const int pageSize = 10;

			var customersQuery = db.Customers
				.Where(c => c.Role == "Customer")
				.AsQueryable();

			// Tìm kiếm
			if (!string.IsNullOrEmpty(searchString))
			{
				customersQuery = customersQuery.Where(c =>
					c.FullName.Contains(searchString) ||
					c.Email.Contains(searchString) ||
					c.UserName.Contains(searchString) ||
					(c.Phone != null && c.Phone.Contains(searchString))
				);
				ViewBag.SearchString = searchString;
			}

			// Lọc theo trạng thái
			if (!string.IsNullOrEmpty(status))
			{
				bool isActive = status == "active";
				customersQuery = customersQuery.Where(c => c.IsActive == isActive);
				ViewBag.Status = status;
			}

			var totalCustomers = await customersQuery.CountAsync();
			var totalPages = (int)Math.Ceiling(totalCustomers / (double)pageSize);

			var customers = await customersQuery
				.Include(c => c.Orders)
				.OrderByDescending(c => c.CreatedAt)
				.Skip((page - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			ViewBag.CurrentPage = page;
			ViewBag.TotalPages = totalPages;
			ViewBag.TotalCustomers = totalCustomers;

			return View(customers);
		}

		// Chi tiết khách hàng
		public async Task<IActionResult> CustomerDetails(int id)
		{
			var customer = await db.Customers
				.Include(c => c.Orders.OrderByDescending(o => o.CreatedAt).Take(10))
				.FirstOrDefaultAsync(c => c.Id == id && c.Role == "Customer");

			if (customer == null)
			{
				return NotFound();
			}

			// Thống kê đơn hàng
			var totalOrders = await db.Orders.CountAsync(o => o.CustomerId == id);
			var completedOrders = await db.Orders.CountAsync(o => o.CustomerId == id && o.Status == "Hoàn thành");
			var cancelledOrders = await db.Orders.CountAsync(o => o.CustomerId == id && o.Status == "Cancelled");
			var totalSpent = await db.Orders
				.Where(o => o.CustomerId == id && o.Status == "Hoàn thành")
				.SumAsync(o => (double?)o.TotalAmount) ?? 0;

			ViewBag.TotalOrders = totalOrders;
			ViewBag.CompletedOrders = completedOrders;
			ViewBag.CancelledOrders = cancelledOrders;
			ViewBag.TotalSpent = totalSpent;

			return View(customer);
		}

		// Khóa/Mở khóa tài khoản
		[HttpPost]
		public async Task<IActionResult> ToggleCustomerStatus(int id)
		{
			var customer = await db.Customers
				.FirstOrDefaultAsync(c => c.Id == id && c.Role == "Customer");

			if (customer == null)
			{
				return Json(new { success = false, message = "Không tìm thấy khách hàng" });
			}

			customer.IsActive = !customer.IsActive;
			await db.SaveChangesAsync();

			return Json(new
			{
				success = true,
				message = customer.IsActive ? "Đã mở khóa tài khoản" : "Đã khóa tài khoản",
				isActive = customer.IsActive
			});
		}

		// Xóa khách hàng (soft delete)
		[HttpPost]
		public async Task<IActionResult> DeleteCustomer(int id)
		{
			var customer = await db.Customers
				.Include(c => c.Orders)
				.FirstOrDefaultAsync(c => c.Id == id && c.Role == "Customer");

			if (customer == null)
			{
				return Json(new { success = false, message = "Không tìm thấy khách hàng" });
			}

			// Kiểm tra có đơn hàng đang xử lý không
			var hasActiveOrders = customer.Orders.Any(o =>
				o.Status == "Pending" ||
				o.Status == "Preparing" ||
				o.Status == "Delivering"
			);

			if (hasActiveOrders)
			{
				return Json(new
				{
					success = false,
					message = "Không thể xóa khách hàng có đơn hàng đang xử lý"
				});
			}

			// Soft delete: chỉ khóa tài khoản
			customer.IsActive = false;
			await db.SaveChangesAsync();

			return Json(new
			{
				success = true,
				message = "Đã khóa tài khoản khách hàng"
			});
		}

		// ==================== QUẢN LÝ DOANH THU ====================

		// Trang thống kê doanh thu
		public async Task<IActionResult> Revenue(string period = "month", int year = 0, int month = 0)
		{
			if (year == 0) year = DateTime.Now.Year;
			if (month == 0) month = DateTime.Now.Month;

			ViewBag.SelectedPeriod = period;
			ViewBag.SelectedYear = year;
			ViewBag.SelectedMonth = month;

		// Thống kê tổng quan - Giờ Việt Nam (UTC+7)
		var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
		var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);
		var today = vietnamNow.Date;

		var totalRevenue = await db.Orders
			.Where(o => o.Status == "Hoàn thành")
			.SumAsync(o => (double?)o.TotalAmount) ?? 0;

		// Lấy đơn hàng và chuyển sang giờ VN để so sánh
		var allDeliveredOrders = await db.Orders
			.Where(o => o.Status == "Hoàn thành")
			.Select(o => new { o.TotalAmount, o.CreatedAt })
			.ToListAsync();

		var todayRevenue = allDeliveredOrders
			.Where(o => TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, vietnamTimeZone).Date == today)
			.Sum(o => o.TotalAmount);

		var monthRevenue = allDeliveredOrders
			.Where(o => {
				var vnDate = TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, vietnamTimeZone);
				return vnDate.Year == vietnamNow.Year && vnDate.Month == vietnamNow.Month;
			})
			.Sum(o => o.TotalAmount);

		var yearRevenue = allDeliveredOrders
			.Where(o => TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, vietnamTimeZone).Year == vietnamNow.Year)
			.Sum(o => o.TotalAmount);

		ViewBag.TotalRevenue = totalRevenue;
		ViewBag.TodayRevenue = todayRevenue;
		ViewBag.MonthRevenue = monthRevenue;
		ViewBag.YearRevenue = yearRevenue;

		// Số đơn hàng
		var totalOrders = allDeliveredOrders.Count;
		var todayOrders = allDeliveredOrders
			.Count(o => TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, vietnamTimeZone).Date == today);

		ViewBag.TotalOrders = totalOrders;
		ViewBag.TodayOrders = todayOrders;

		return View();
	}

	// API lấy dữ liệu biểu đồ doanh thu theo tháng
	[HttpGet]
	public async Task<IActionResult> GetRevenueByMonth(int year, int month)
	{
		var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

		// Lấy tất cả đơn hàng đã giao trong tháng
		var orders = await db.Orders
			.Where(o => o.Status == "Hoàn thành")
			.Select(o => new { o.TotalAmount, o.CreatedAt })
			.ToListAsync();

		// Chuyển sang giờ VN và lọc theo tháng
		var dailyRevenue = orders
			.Select(o => new {
				o.TotalAmount,
				VnDate = TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, vietnamTimeZone)
			})
			.Where(o => o.VnDate.Year == year && o.VnDate.Month == month)
			.GroupBy(o => o.VnDate.Day)
			.Select(g => new
			{
				Day = g.Key,
				Revenue = g.Sum(o => o.TotalAmount),
				OrderCount = g.Count()
			})
			.OrderBy(x => x.Day)
			.ToList();

		// Tạo dữ liệu đầy đủ cho tất cả các ngày trong tháng
		var daysInMonth = DateTime.DaysInMonth(year, month);
		var result = Enumerable.Range(1, daysInMonth)
			.Select(day => {
				var data = dailyRevenue.FirstOrDefault(d => d.Day == day);
				return new
				{
					Day = day,
					Revenue = data?.Revenue ?? 0,
					OrderCount = data?.OrderCount ?? 0
				};
			})
			.ToList();

		return Json(result);
	}	// API lấy dữ liệu biểu đồ doanh thu theo năm
	[HttpGet]
	public async Task<IActionResult> GetRevenueByYear(int year)
	{
		var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

		// Lấy tất cả đơn hàng đã giao
		var orders = await db.Orders
			.Where(o => o.Status == "Hoàn thành")
			.Select(o => new { o.TotalAmount, o.CreatedAt })
			.ToListAsync();

		// Chuyển sang giờ VN và lọc theo năm
		var monthlyRevenue = orders
			.Select(o => new {
				o.TotalAmount,
				VnDate = TimeZoneInfo.ConvertTimeFromUtc(o.CreatedAt, vietnamTimeZone)
			})
			.Where(o => o.VnDate.Year == year)
			.GroupBy(o => o.VnDate.Month)
			.Select(g => new
			{
				Month = g.Key,
				Revenue = g.Sum(o => o.TotalAmount),
				OrderCount = g.Count()
			})
			.OrderBy(x => x.Month)
			.ToList();

		// Tạo dữ liệu đầy đủ cho 12 tháng
		var result = Enumerable.Range(1, 12)
			.Select(month => {
				var data = monthlyRevenue.FirstOrDefault(m => m.Month == month);
				return new
				{
					Month = month,
					Revenue = data?.Revenue ?? 0,
					OrderCount = data?.OrderCount ?? 0
				};
			})
			.ToList();

		return Json(result);
	}		// API thống kê sản phẩm bán chạy
		[HttpGet]
		public async Task<IActionResult> GetTopSellingProducts(int limit = 10)
		{
			var topProducts = await db.OrderItems
				.Include(oi => oi.MenuItem)
				.Include(oi => oi.Order)
				.Where(oi => oi.Order != null && oi.Order.Status == "Hoàn thành" && oi.MenuItem != null)
				.GroupBy(oi => new { oi.MenuItemId, oi.MenuItem!.Name })
				.Select(g => new
				{
					ProductName = g.Key.Name,
					TotalQuantity = g.Sum(oi => oi.Quantity),
					TotalRevenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
				})
				.OrderByDescending(x => x.TotalRevenue)
				.Take(limit)
				.ToListAsync();

			return Json(topProducts);
		}

		// API thống kê theo danh mục
		[HttpGet]
		public async Task<IActionResult> GetRevenueByCategory()
		{
			var categoryRevenue = await db.OrderItems
				.Include(oi => oi.MenuItem)
				.ThenInclude(mi => mi!.Category)
				.Include(oi => oi.Order)
				.Where(oi => oi.Order != null && oi.Order.Status == "Hoàn thành" && oi.MenuItem != null && oi.MenuItem.Category != null)
				.GroupBy(oi => oi.MenuItem!.Category!.Name)
				.Select(g => new
				{
					Category = g.Key,
					Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice),
					OrderCount = g.Select(oi => oi.OrderId).Distinct().Count()
				})
				.OrderByDescending(x => x.Revenue)
				.ToListAsync();

			return Json(categoryRevenue);
		}

		// API thống kê khách hàng top
		[HttpGet]
		public async Task<IActionResult> GetTopCustomers(int limit = 10)
		{
			var topCustomers = await db.Orders
				.Include(o => o.Customer)
				.Where(o => o.Status == "Hoàn thành" && o.Customer != null)
				.GroupBy(o => new { o.CustomerId, o.Customer!.FullName, o.Customer.Email })
				.Select(g => new
				{
					CustomerName = g.Key.FullName,
					Email = g.Key.Email,
					TotalOrders = g.Count(),
					TotalSpent = g.Sum(o => o.TotalAmount)
				})
				.OrderByDescending(x => x.TotalSpent)
				.Take(limit)
				.ToListAsync();

			return Json(topCustomers);
		}
	}
}
