using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemovedUseDriverSpecificHourlyRateCheckboxesFromOrderLinesAndQuoteLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseDriverSpecificHourlyRate",
                table: "QuoteLine");

            migrationBuilder.DropColumn(
                name: "UseDriverSpecificHourlyRate",
                table: "OrderLine");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UseDriverSpecificHourlyRate",
                table: "QuoteLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "UseDriverSpecificHourlyRate",
                table: "OrderLine",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
