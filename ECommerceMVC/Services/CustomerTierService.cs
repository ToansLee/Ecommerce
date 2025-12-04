using ECommerceMVC.Data;
using ECommerceMVC.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceMVC.Services
{
    public class CustomerTierService
    {
        private readonly FoodOrderingContext _db;

        public CustomerTierService(FoodOrderingContext db)
        {
            _db = db;
        }

        // Tính toán và cập nhật hạng thành viên
        public async Task UpdateCustomerTier(int customerId)
        {
            var customer = await _db.Customers.FindAsync(customerId);
            if (customer == null) return;

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            // Reset nếu sang tháng mới
            if (customer.LastTierUpdate.Month != vietnamNow.Month || 
                customer.LastTierUpdate.Year != vietnamNow.Year)
            {
                customer.MonthlySpending = 0;
                customer.CustomerTier = "Đồng";
            }

            // Tính tổng chi tiêu trong tháng (chỉ tính đơn Hoàn thành)
            var firstDayOfMonth = new DateTime(vietnamNow.Year, vietnamNow.Month, 1);
            var monthlyTotal = await _db.Orders
                .Where(o => o.CustomerId == customerId && 
                           o.Status == "Hoàn thành" &&
                           o.CreatedAt >= firstDayOfMonth)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            customer.MonthlySpending = monthlyTotal;
            customer.LastTierUpdate = vietnamNow;

            // Xác định hạng
            if (monthlyTotal >= 5000000)
            {
                customer.CustomerTier = "Kim cương";
            }
            else if (monthlyTotal >= 3000000)
            {
                customer.CustomerTier = "Vàng";
            }
            else if (monthlyTotal >= 1000000)
            {
                customer.CustomerTier = "Bạc";
            }
            else
            {
                customer.CustomerTier = "Đồng"; // Mặc định
            }

            await _db.SaveChangesAsync();
        }

        // Lấy phần trăm giảm giá theo hạng
        public decimal GetDiscountPercentage(string tier)
        {
            return tier switch
            {
                "Kim cương" => 0.10m, // 10%
                "Vàng" => 0.05m,       // 5%
                "Bạc" => 0.03m,        // 3%
                "Đồng" => 0m,          // 0%
                _ => 0m
            };
        }

        // Tính số tiền giảm giá
        public decimal CalculateDiscount(decimal totalAmount, string tier)
        {
            var discountPercentage = GetDiscountPercentage(tier);
            return totalAmount * discountPercentage;
        }

        // Lấy thông tin hạng của khách hàng
        public async Task<CustomerTierInfo> GetCustomerTierInfo(int customerId)
        {
            var customer = await _db.Customers.FindAsync(customerId);
            if (customer == null)
            {
                return new CustomerTierInfo
                {
                    Tier = "Đồng",
                    MonthlySpending = 0,
                    DiscountPercentage = 0,
                    NextTier = "Bạc",
                    AmountToNextTier = 1000000
                };
            }

            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            // Reset nếu sang tháng mới
            if (customer.LastTierUpdate.Month != vietnamNow.Month || 
                customer.LastTierUpdate.Year != vietnamNow.Year)
            {
                await UpdateCustomerTier(customerId);
                customer = await _db.Customers.FindAsync(customerId);
            }

            var info = new CustomerTierInfo
            {
                Tier = customer!.CustomerTier,
                MonthlySpending = customer.MonthlySpending,
                DiscountPercentage = (int)(GetDiscountPercentage(customer.CustomerTier) * 100)
            };

            // Tính số tiền cần để lên hạng tiếp theo
            switch (customer.CustomerTier)
            {
                case "Đồng":
                    info.NextTier = "Bạc";
                    info.AmountToNextTier = 1000000 - customer.MonthlySpending;
                    break;
                case "Bạc":
                    info.NextTier = "Vàng";
                    info.AmountToNextTier = 3000000 - customer.MonthlySpending;
                    break;
                case "Vàng":
                    info.NextTier = "Kim cương";
                    info.AmountToNextTier = 5000000 - customer.MonthlySpending;
                    break;
                case "Kim cương":
                    info.NextTier = null;
                    info.AmountToNextTier = 0;
                    break;
            }

            return info;
        }

        // Reset tất cả khách hàng khi sang tháng mới (có thể chạy bằng background job)
        public async Task ResetMonthlyTiers()
        {
            var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
            var vietnamNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, vietnamTimeZone);

            var customers = await _db.Customers
                .Where(c => c.LastTierUpdate.Month != vietnamNow.Month || 
                           c.LastTierUpdate.Year != vietnamNow.Year)
                .ToListAsync();

            foreach (var customer in customers)
            {
                customer.MonthlySpending = 0;
                customer.CustomerTier = "Đồng";
                customer.LastTierUpdate = vietnamNow;
            }

            await _db.SaveChangesAsync();
        }
    }

    public class CustomerTierInfo
    {
        public string Tier { get; set; } = "Đồng";
        public decimal MonthlySpending { get; set; }
        public int DiscountPercentage { get; set; }
        public string? NextTier { get; set; }
        public decimal AmountToNextTier { get; set; }
    }
}
