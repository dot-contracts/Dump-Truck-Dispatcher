using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedFuelSurchargeCalculationIdToQuotes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "FuelSurchargeCalculationId",
                table: "Quote",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Quote_FuelSurchargeCalculationId",
                table: "Quote",
                column: "FuelSurchargeCalculationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Quote_FuelSurchargeCalculation_FuelSurchargeCalculationId",
                table: "Quote",
                column: "FuelSurchargeCalculationId",
                principalTable: "FuelSurchargeCalculation",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Quote_FuelSurchargeCalculation_FuelSurchargeCalculationId",
                table: "Quote");

            migrationBuilder.DropIndex(
                name: "IX_Quote_FuelSurchargeCalculationId",
                table: "Quote");

            migrationBuilder.DropColumn(
                name: "FuelSurchargeCalculationId",
                table: "Quote");
        }
    }
}
