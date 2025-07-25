using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedBedConstructionToQuoteServicesAndToOrderLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BedConstruction",
                table: "QuoteService",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BedConstruction",
                table: "OrderLine",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BedConstruction",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "BedConstruction",
                table: "OrderLine");
        }
    }
}
