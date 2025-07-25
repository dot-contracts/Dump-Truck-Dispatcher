using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedIsFreightRateToPayDriversOverriddenToOrderLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFreightRateToPayDriversOverridden",
                table: "OrderLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                update OrderLine set IsFreightRateToPayDriversOverridden = 1
                where FreightPricePerUnit != FreightRateToPayDrivers
                or FreightPricePerUnit is null and FreightRateToPayDrivers is not null
                or FreightPricePerUnit is not null and FreightRateToPayDrivers is null
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFreightRateToPayDriversOverridden",
                table: "OrderLine");
        }
    }
}
