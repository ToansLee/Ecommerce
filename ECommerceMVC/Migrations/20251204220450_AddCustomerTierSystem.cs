using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerceMVC.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerTierSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerTier",
                table: "Customers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "LastTierUpdate",
                table: "Customers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlySpending",
                table: "Customers",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerTier",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "LastTierUpdate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MonthlySpending",
                table: "Customers");
        }
    }
}
