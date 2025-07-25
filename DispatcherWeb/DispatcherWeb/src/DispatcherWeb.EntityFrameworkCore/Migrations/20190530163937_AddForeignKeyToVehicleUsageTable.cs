using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddForeignKeyToVehicleUsageTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_VehicleUsage_TruckId",
                table: "VehicleUsage",
                column: "TruckId");

            migrationBuilder.AddForeignKey(
                name: "FK_VehicleUsage_Truck_TruckId",
                table: "VehicleUsage",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_VehicleUsage_Truck_TruckId",
                table: "VehicleUsage");

            migrationBuilder.DropIndex(
                name: "IX_VehicleUsage_TruckId",
                table: "VehicleUsage");
        }
    }
}
