using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedTravelTimeToOrderLinesAndLoads : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "TravelTime",
                table: "QuoteLine",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "TravelTime",
                table: "OrderLine",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "TravelTime",
                table: "Load",
                type: "smallint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TravelTime",
                table: "QuoteLine");

            migrationBuilder.DropColumn(
                name: "TravelTime",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "TravelTime",
                table: "Load");
        }
    }
}
