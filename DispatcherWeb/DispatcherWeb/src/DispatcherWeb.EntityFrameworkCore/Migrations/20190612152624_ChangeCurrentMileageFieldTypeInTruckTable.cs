using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class ChangeCurrentMileageFieldTypeInTruckTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "CurrentMileage",
                table: "Truck",
                type: "decimal(19, 1)",
                nullable: false,
                oldClrType: typeof(int));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "CurrentMileage",
                table: "Truck",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(19, 1)");
        }
    }
}
