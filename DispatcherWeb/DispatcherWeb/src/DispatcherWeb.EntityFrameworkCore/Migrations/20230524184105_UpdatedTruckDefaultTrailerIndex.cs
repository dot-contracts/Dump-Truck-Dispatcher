using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class UpdatedTruckDefaultTrailerIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Truck_Driver_DefaultDriverId",
                table: "Truck");

            migrationBuilder.DropForeignKey(
                name: "FK_Truck_Truck_DefaultTrailerId",
                table: "Truck");

            migrationBuilder.AddForeignKey(
                name: "FK_Truck_Driver_DefaultDriverId",
                table: "Truck",
                column: "DefaultDriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Truck_Truck_DefaultTrailerId",
                table: "Truck",
                column: "DefaultTrailerId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Truck_Driver_DefaultDriverId",
                table: "Truck");

            migrationBuilder.DropForeignKey(
                name: "FK_Truck_Truck_DefaultTrailerId",
                table: "Truck");

            migrationBuilder.AddForeignKey(
                name: "FK_Truck_Driver_DefaultDriverId",
                table: "Truck",
                column: "DefaultDriverId",
                principalTable: "Driver",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Truck_Truck_DefaultTrailerId",
                table: "Truck",
                column: "DefaultTrailerId",
                principalTable: "Truck",
                principalColumn: "Id");
        }
    }
}
