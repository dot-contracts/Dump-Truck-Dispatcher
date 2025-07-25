using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsFreightRateToPayDriversOverriddenToQuoteLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFreightRateToPayDriversOverridden",
                table: "QuoteLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                update QuoteLine set IsFreightRateToPayDriversOverridden = 1
                where FreightRate != FreightRateToPayDrivers
                or FreightRate is null and FreightRateToPayDrivers is not null
                or FreightRate is not null and FreightRateToPayDrivers is null
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFreightRateToPayDriversOverridden",
                table: "QuoteLine");
        }
    }
}
