# Hướng dẫn cập nhật hệ thống trạng thái đơn hàng

## Tổng quan thay đổi

Hệ thống đã được cập nhật để sử dụng trạng thái đơn hàng bằng tiếng Việt:

### Trạng thái đơn hàng:
1. **Chờ xác nhận** - Đơn hàng mới được tạo (mặc định)
2. **Chuẩn bị món** - Admin đã xác nhận và bắt đầu chuẩn bị
3. **Đang giao** - Đơn hàng đang được giao đến khách
4. **Hoàn thành** - Đơn hàng đã được giao thành công
5. **Huỷ đơn** - Đơn hàng bị hủy

## Các thay đổi đã thực hiện:

### 1. Model (Order.cs)
- Thay đổi `MaxLength` của `Status` từ 20 lên 50 ký tự
- Trạng thái mặc định: `"Chờ xác nhận"`

### 2. Controllers
- **CheckoutController**: Đặt trạng thái "Chờ xác nhận" khi tạo đơn mới
- **AdminController**: Cập nhật logic xử lý tất cả trạng thái tiếng Việt
- **OrderController**: Kiểm tra trạng thái "Huỷ đơn"

### 3. Views
- **Admin/Orders.cshtml**: Filter và hiển thị trạng thái tiếng Việt
- **Admin/OrderDetails.cshtml**: Các nút chuyển trạng thái với tên tiếng Việt
- **Order/Index.cshtml**: Hiển thị trạng thái cho khách hàng
- **Order/Details.cshtml**: Timeline trạng thái tiếng Việt

### 4. Database Migration
- File: `20251205_UpdateOrderStatusToVietnamese.cs`
- Tự động chuyển đổi các trạng thái cũ sang tiếng Việt

## Cách áp dụng migration:

### Lưu ý quan trọng:
Migration file đã được tạo thủ công, bạn cần đổi tên file để phù hợp với format của EF Core.

### Cách 1: Sử dụng dotnet ef (Khuyến nghị)

```powershell
# Di chuyển đến thư mục project
cd d:\Workspace\ASP_core_MVC\Food\Ecommerce\ECommerceMVC

# Cài đặt EF Core tools nếu chưa có
dotnet tool install --global dotnet-ef

# Tạo migration mới (EF sẽ tự động phát hiện thay đổi)
dotnet ef migrations add UpdateOrderStatusToVietnamese

# Áp dụng migration vào database
dotnet ef database update
```

### Cách 2: Chạy migration thủ công trong SQL Server

Nếu không dùng được dotnet ef, bạn có thể chạy SQL trực tiếp:

```sql
-- 1. Thay đổi kích thước cột Status
ALTER TABLE Orders 
ALTER COLUMN Status NVARCHAR(50) NOT NULL;

-- 2. Cập nhật các trạng thái cũ sang tiếng Việt
UPDATE Orders 
SET Status = CASE Status
    WHEN 'Pending' THEN N'Chờ xác nhận'
    WHEN 'Preparing' THEN N'Chuẩn bị món'
    WHEN 'Delivering' THEN N'Đang giao'
    WHEN 'Delivered' THEN N'Hoàn thành'
    WHEN 'Cancelled' THEN N'Huỷ đơn'
    ELSE Status
END
WHERE Status IN ('Pending', 'Preparing', 'Delivering', 'Delivered', 'Cancelled');

-- 3. Thêm record vào __EFMigrationsHistory
INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion)
VALUES ('20251205_UpdateOrderStatusToVietnamese', '8.0.0');
```

### Cách 3: Chạy từ code khi khởi động ứng dụng

Thêm vào `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FoodOrderingContext>();
    db.Database.Migrate(); // Tự động áp dụng migration
}
```

## Kiểm tra sau khi cập nhật:

1. **Kiểm tra database**:
   ```sql
   SELECT DISTINCT Status FROM Orders;
   ```
   Kết quả nên hiển thị: Chờ xác nhận, Chuẩn bị món, Đang giao, Hoàn thành, Huỷ đơn

2. **Kiểm tra chức năng Admin**:
   - Vào trang quản lý đơn hàng
   - Kiểm tra filter theo trạng thái
   - Thử chuyển trạng thái đơn hàng

3. **Kiểm tra chức năng Khách hàng**:
   - Đặt đơn hàng mới (phải có trạng thái "Chờ xác nhận")
   - Xem lịch sử đơn hàng
   - Xem chi tiết đơn hàng (timeline trạng thái)

## Quy trình chuyển trạng thái:

```
Chờ xác nhận → Chuẩn bị món → Đang giao → Hoàn thành
       ↓              ↓            ↓
              Huỷ đơn (có thể hủy ở 3 trạng thái đầu)
```

## Lưu ý:
- Chỉ đơn hàng "Hoàn thành" mới được tính vào doanh thu
- Không thể xuất hóa đơn cho đơn hàng "Huỷ đơn"
- Admin có thể hủy đơn ở bất kỳ trạng thái nào trừ "Hoàn thành"
