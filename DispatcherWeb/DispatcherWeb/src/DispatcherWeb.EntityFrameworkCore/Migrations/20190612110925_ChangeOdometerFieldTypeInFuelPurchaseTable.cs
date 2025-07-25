using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class ChangeOdometerFieldTypeInFuelPurchaseTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Odometer",
                table: "FuelPurchase",
                type: "decimal(19, 1)",
                nullable: true,
                oldClrType: typeof(float),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Odometer",
                table: "FuelPurchase",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 1)",
                oldNullable: true);
        }
    }
}
