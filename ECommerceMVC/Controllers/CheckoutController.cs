using ECommerceMVC.Data;
using ECommerceMVC.Models;
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

        public CheckoutController(FoodOrderingContext context)
        {
            db = context;
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

        public IActionResult Index()
        {
            try
            {
                var cart = GetCart();
                return View(cart);
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index", "Cart");
            }
        }

        [HttpPost]
        public IActionResult ProcessPayment(string? deliveryAddress, string? notes)
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

                // Calculate totals
                var orderTotal = cart.CartItems.Sum(ci => ci.Quantity * ci.Price);
                var shippingFee = orderTotal >= 500000 ? 0 : 35000;
                var totalAmount = orderTotal + shippingFee;

                // Create single order for all items
                var order = new Order
                {
                    CustomerId = customerId,
                    TotalAmount = totalAmount,
                    Status = "Pending",
                    Notes = notes,
                    CreatedAt = DateTime.UtcNow
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

                // Create payment record (COD)
                var payment = new Payment
                {
                    OrderId = order.Id,
                    Method = "COD",
                    Amount = totalAmount,
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };
                db.Payments.Add(payment);

                db.SaveChanges();

                // Clear cart
                db.CartItems.RemoveRange(cart.CartItems);
                db.SaveChanges();

                TempData["Success"] = "Đặt hàng thành công! Bạn sẽ thanh toán khi nhận hàng.";
                return RedirectToAction("OrderSuccess", new { orderId = order.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction("Index");
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
