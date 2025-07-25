using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedTypeToFuelSurchargeCalculation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "FuelSurchargeCalculation",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql($"Update FuelSurchargeCalculation set [Type] = {(int)FuelSurchargeCalculationType.BasedOnActualFuelCost}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Type",
                table: "FuelSurchargeCalculation");
        }
    }
}
