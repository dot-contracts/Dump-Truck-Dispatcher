using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddTruckIdForeignKeyToFuelPurchaseTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_FuelPurchase_TruckId",
                table: "FuelPurchase",
                column: "TruckId");

            migrationBuilder.AddForeignKey(
                name: "FK_FuelPurchase_Truck_TruckId",
                table: "FuelPurchase",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FuelPurchase_Truck_TruckId",
                table: "FuelPurchase");

            migrationBuilder.DropIndex(
                name: "IX_FuelPurchase_TruckId",
                table: "FuelPurchase");
        }
    }
}
