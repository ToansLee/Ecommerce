using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceMVC.Migrations
{
    /// <inheritdoc />
    public partial class chat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_SellerId",
                table: "Restaurants");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_SellerId",
                table: "Restaurants",
                column: "SellerId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_SellerId",
                table: "Restaurants");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_SellerId",
                table: "Restaurants",
                column: "SellerId");
        }
    }
}
