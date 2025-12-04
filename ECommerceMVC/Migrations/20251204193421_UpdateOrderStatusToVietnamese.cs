using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceMVC.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderStatusToVietnamese : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Thay đổi MaxLength của cột Status từ 20 lên 50
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            // Cập nhật các trạng thái cũ sang trạng thái mới (tiếng Việt)
            migrationBuilder.Sql(@"
                UPDATE Orders 
                SET Status = CASE Status
                    WHEN 'Pending' THEN N'Chờ xác nhận'
                    WHEN 'Preparing' THEN N'Chuẩn bị món'
                    WHEN 'Delivering' THEN N'Đang giao'
                    WHEN 'Delivered' THEN N'Hoàn thành'
                    WHEN 'Cancelled' THEN N'Huỷ đơn'
                    ELSE Status
                END
                WHERE Status IN ('Pending', 'Preparing', 'Delivering', 'Delivered', 'Cancelled')
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Chuyển ngược lại từ tiếng Việt sang tiếng Anh
            migrationBuilder.Sql(@"
                UPDATE Orders 
                SET Status = CASE Status
                    WHEN N'Chờ xác nhận' THEN 'Pending'
                    WHEN N'Chuẩn bị món' THEN 'Preparing'
                    WHEN N'Đang giao' THEN 'Delivering'
                    WHEN N'Hoàn thành' THEN 'Delivered'
                    WHEN N'Huỷ đơn' THEN 'Cancelled'
                    ELSE Status
                END
                WHERE Status IN (N'Chờ xác nhận', N'Chuẩn bị món', N'Đang giao', N'Hoàn thành', N'Huỷ đơn')
            ");

            // Thay đổi MaxLength của cột Status từ 50 về 20
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);
        }
    }
}
