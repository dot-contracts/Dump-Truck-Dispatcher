using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedFuelSurchargeCalculationIdToOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BaseFuelCost",
                table: "Order",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FuelSurchargeCalculationId",
                table: "Order",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Order_FuelSurchargeCalculationId",
                table: "Order",
                column: "FuelSurchargeCalculationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_FuelSurchargeCalculation_FuelSurchargeCalculationId",
                table: "Order",
                column: "FuelSurchargeCalculationId",
                principalTable: "FuelSurchargeCalculation",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_FuelSurchargeCalculation_FuelSurchargeCalculationId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_FuelSurchargeCalculationId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "BaseFuelCost",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "FuelSurchargeCalculationId",
                table: "Order");
        }
    }
}
