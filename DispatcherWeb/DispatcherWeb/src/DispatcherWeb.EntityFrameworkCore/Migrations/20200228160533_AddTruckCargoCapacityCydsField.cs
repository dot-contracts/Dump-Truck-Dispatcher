using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddTruckCargoCapacityCydsField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "CargoCapacity",
                table: "Truck",
                nullable: true,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CargoCapacityCyds",
                table: "Truck",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CargoCapacityCyds",
                table: "Truck");

            migrationBuilder.AlterColumn<int>(
                name: "CargoCapacity",
                table: "Truck",
                nullable: true,
                oldClrType: typeof(decimal),
                oldNullable: true);
        }
    }
}
