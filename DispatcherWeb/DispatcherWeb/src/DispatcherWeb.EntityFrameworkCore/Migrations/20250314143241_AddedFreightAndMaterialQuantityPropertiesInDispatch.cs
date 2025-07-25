using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedFreightAndMaterialQuantityPropertiesInDispatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FreightQuantity",
                table: "Dispatch",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialQuantity",
                table: "Dispatch",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE d
                SET 
                    d.MaterialQuantity = ol.MaterialQuantity,
                    d.FreightQuantity = ol.FreightQuantity
                FROM Dispatch d
                INNER JOIN OrderLine ol WITH (NOLOCK) ON d.OrderLineId = ol.Id
                ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreightQuantity",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "MaterialQuantity",
                table: "Dispatch");
        }
    }
}
