using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedHaulingCompanyIdsToTrucksAndDrivers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyTenantId",
                table: "Truck",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyTruckId",
                table: "Truck",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyDriverId",
                table: "Driver",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HaulingCompanyTenantId",
                table: "Driver",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HaulingCompanyTenantId",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyTruckId",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyDriverId",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "HaulingCompanyTenantId",
                table: "Driver");
        }
    }
}
