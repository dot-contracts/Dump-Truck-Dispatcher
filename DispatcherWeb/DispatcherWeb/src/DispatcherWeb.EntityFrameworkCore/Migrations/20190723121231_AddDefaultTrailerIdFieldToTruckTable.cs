using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddDefaultTrailerIdFieldToTruckTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DefaultTrailerId",
                table: "Truck",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Truck_DefaultTrailerId",
                table: "Truck",
                column: "DefaultTrailerId");

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
                name: "FK_Truck_Truck_DefaultTrailerId",
                table: "Truck");

            migrationBuilder.DropIndex(
                name: "IX_Truck_DefaultTrailerId",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "DefaultTrailerId",
                table: "Truck");
        }
    }
}
