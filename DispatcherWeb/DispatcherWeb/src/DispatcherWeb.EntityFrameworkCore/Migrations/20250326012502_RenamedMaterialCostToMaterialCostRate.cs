using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedMaterialCostToMaterialCostRate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaterialCost",
                table: "QuoteLine",
                newName: "MaterialCostRate");

            migrationBuilder.RenameColumn(
                name: "MaterialCost",
                table: "OrderLine",
                newName: "MaterialCostRate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "MaterialCostRate",
                table: "QuoteLine",
                newName: "MaterialCost");

            migrationBuilder.RenameColumn(
                name: "MaterialCostRate",
                table: "OrderLine",
                newName: "MaterialCost");
        }
    }
}
