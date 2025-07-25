using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RenamedCanChangeToCanChangeBaseFuelCost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CanChange",
                table: "FuelSurchargeCalculation",
                newName: "CanChangeBaseFuelCost");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CanChangeBaseFuelCost",
                table: "FuelSurchargeCalculation",
                newName: "CanChange");
        }
    }
}
