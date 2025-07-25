using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedMaterialCostToOrderLineAndQuoteLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCost",
                table: "QuoteLine",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialCost",
                table: "OrderLine",
                type: "decimal(19,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaterialCost",
                table: "QuoteLine");

            migrationBuilder.DropColumn(
                name: "MaterialCost",
                table: "OrderLine");
        }
    }
}
