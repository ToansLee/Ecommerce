using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceMVC.Migrations
{
    public partial class del_1role : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Restaurants_SellerId",
                table: "Restaurants");

            migrationBuilder.CreateIndex(
                name: "IX_Restaurants_SellerId",
                table: "Restaurants",
                column: "SellerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
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
    }
}
