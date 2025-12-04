using ECommerceMVC.Data;
using ECommerceMVC.Models;
using ECommerceMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceMVC.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly FoodOrderingContext db;
        private readonly VNPayService _vnPayService;
        private readonly CustomerTierService _tierService;

        public CheckoutController(FoodOrderingContext context, VNPayService vnPayService, CustomerTierService tierService)
        {
            db = context;
            _vnPayService = vnPayService;
            _tierService = tierService;
        }

        private Models.Cart GetCart()
        {
            Models.Cart? cart;
            var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            
            cart = db.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.MenuItem)
                .FirstOrDefault(c => c.CustomerId == customerId);

            if (cart == null || !cart.CartItems.Any())
            {
                throw new Exception("Giỏ hàng trống");
            }

            return cart;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var cart = GetCart();
                var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var customer = db.Customers.Find(customerId);
                
                // Cập nhật hạng thành viên
                await _tierService.UpdateCustomerTier(customerId);
                customer = db.Customers.Find(customerId);
                
                ViewBag.WalletBalance = customer?.WalletBalance ?? 0;
                ViewBag.CustomerTier = customer?.CustomerTier ?? "Bạc";
                ViewBag.DiscountPercentage = (int)(_tierService.GetDiscountPercentage(customer?.CustomerTier ?? "Bạc") * 100);
                
                return View(cart);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        public IActionResult ProcessPayment(string? deliveryAddress, string? notes, string paymentMethod = "COD")
        {
            try
            {
                if (string.IsNullOrEmpty(deliveryAddress))
                {
                    TempData["Error"] = "Vui lòng nhập địa chỉ giao hàng";
                    return RedirectToAction("Index");
                }

                var cart = GetCart();
                var customerId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
                var customer = db.Customers.Find(customerId);

                // Calculate totals
                var orderTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.Price);
                var shippingFee = orderTotal >= 500000 ? 0 : 35000;
                var subtotal = orderTotal + shippingFee;
                
                // Áp dụng giảm giá theo hạng thành viên
                var tierDiscount = _tierService.CalculateDiscount(subtotal, customer?.CustomerTier ?? "Bạc");
                var totalAmount = subtotal - tierDiscount;

                // Ưu tiên sử dụng số dư ví
                var walletUsed = Math.Min(customer?.WalletBalance ?? 0, totalAmount);
                var remainingAmount = totalAmount - walletUsed;

                // Create single order for all items
                var order = new Order
                {
                    CustomerId = customerId,
                    TotalAmount = totalAmount,
                    Status = "Chờ xác nhận",
                    Notes = notes,
                    CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
                };

                db.Orders.Add(order);
                db.SaveChanges();

                // Create order items
                foreach (var cartItem in cart.CartItems)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        MenuItemId = cartItem.MenuItemId,
                        Quantity = cartItem.Quantity,
                        UnitPrice = cartItem.Price
                    };
                    db.OrderItems.Add(orderItem);
                }

                // Trừ tiền từ ví nếu có
                if (walletUsed > 0 && customer != null)
                {
                    customer.WalletBalance -= walletUsed;
                    var walletTransaction = new WalletTransaction
                    {
                        CustomerId = customerId,
                        Amount = -walletUsed,
                        Type = "Thanh toán",
                        Description = $"Thanh toán đơn hàng #{order.Id}",
                        OrderId = order.Id
                    };
                    db.WalletTransactions.Add(walletTransaction);
                }

                if (paymentMethod == "VNPay" && remainingAmount > 0)
                {
                    // Thanh toán VNPay cho số tiền còn lại
                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        Method = "VNPay",
                        Amount = remainingAmount,
                        Status = "Pending",
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
                    };
                    db.Payments.Add(payment);
                    db.SaveChanges();

                    // Clear cart
                    db.CartItems.RemoveRange(cart.CartItems);
                    db.SaveChanges();

                    // Tạo URL thanh toán VNPay
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
                    var paymentUrl = _vnPayService.CreatePaymentUrl(
                        order.Id,
                        remainingAmount,
                        $"Thanh toán đơn hàng #{order.Id}",
                        ipAddress
                    );

                    return Redirect(paymentUrl);
                }
                else
                {
                    // Thanh toán COD hoặc đã đủ tiền trong ví
                    var payment = new Payment
                    {
                        OrderId = order.Id,
                        Method = remainingAmount == 0 ? "Wallet" : "COD",
                        Amount = remainingAmount,
                        Status = remainingAmount == 0 ? "Completed" : "Pending",
                        CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"))
                    };
                    db.Payments.Add(payment);
                    db.SaveChanges();

                    // Clear cart
                    db.CartItems.RemoveRange(cart.CartItems);
                    db.SaveChanges();

                    TempData["Success"] = remainingAmount == 0 
                        ? "Đặt hàng thành công! Đã thanh toán bằng ví." 
                        : "Đặt hàng thành công! Bạn sẽ thanh toán khi nhận hàng.";
                    return RedirectToAction("OrderSuccess", new { orderId = order.Id });
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public IActionResult VNPayReturn()
        {
            try
            {
                var response = _vnPayService.ProcessPaymentResponse(Request.Query);

                if (response.Success)
                {
                    var payment = db.Payments
                        .Include(p => p.Order)
                        .FirstOrDefault(p => p.OrderId == response.OrderId);

                    if (payment != null)
                    {
                        payment.Status = "Completed";
                        payment.TransactionId = response.TransactionId;
                        payment.CompletedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"));
                        db.SaveChanges();

                        TempData["Success"] = "Thanh toán VNPay thành công!";
                        return RedirectToAction("OrderSuccess", new { orderId = response.OrderId });
                    }
                }

                TempData["Error"] = "Thanh toán VNPay thất bại!";
                return RedirectToAction("Index", "Order");
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Order");
            }
        }

        public IActionResult OrderSuccess(int orderId)
        {
            var order = db.Orders
                .Include(o => o.Items)
                .ThenInclude(oi => oi.MenuItem)
                .Include(o => o.Payment)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }
}
