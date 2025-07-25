using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedHaulingCompanyOrderIdFromOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HaulingCompanyOrderId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyTenantId",
                table: "Order");

            migrationBuilder.AddColumn<bool>(
                name: "HasLinkedHaulingCompanyOrders",
                table: "Order",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasLinkedHaulingCompanyOrders",
                table: "Order");

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyOrderId",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyTenantId",
                table: "Order",
                type: "int",
                nullable: true);
        }
    }
}
