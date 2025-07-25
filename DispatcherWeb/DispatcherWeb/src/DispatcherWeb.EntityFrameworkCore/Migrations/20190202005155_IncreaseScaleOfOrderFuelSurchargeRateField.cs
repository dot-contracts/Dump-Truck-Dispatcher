using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class IncreaseScaleOfOrderFuelSurchargeRateField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "FuelSurchargeRate",
                table: "Order",
                type: "decimal(19, 4)",
                nullable: false,
                oldClrType: typeof(decimal));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "FuelSurchargeRate",
                table: "Order",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 4)");
        }
    }
}
