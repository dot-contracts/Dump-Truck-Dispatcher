using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedOverriddenRateColumnsToQuoteLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFreightRateOverridden",
                table: "QuoteLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsLeaseHaulerRateOverridden",
                table: "QuoteLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPricePerUnitOverridden",
                table: "QuoteLine",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFreightRateOverridden",
                table: "QuoteLine");

            migrationBuilder.DropColumn(
                name: "IsLeaseHaulerRateOverridden",
                table: "QuoteLine");

            migrationBuilder.DropColumn(
                name: "IsPricePerUnitOverridden",
                table: "QuoteLine");
        }
    }
}
