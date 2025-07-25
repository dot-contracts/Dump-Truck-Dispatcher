using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class FreightRateToPayDriversColumn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FreightRateToPayDrivers",
                table: "QuoteService",
                type: "decimal(19,4)",
                nullable: true);

            var sql = @"UPDATE QuoteService SET FreightRateToPayDrivers = FreightRate;";
            migrationBuilder.Sql(sql);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightRateToPayDrivers",
                table: "OrderLine",
                type: "decimal(19,4)",
                nullable: true);

            sql = @"UPDATE OrderLine SET FreightRateToPayDrivers = FreightPricePerUnit;";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreightRateToPayDrivers",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "FreightRateToPayDrivers",
                table: "OrderLine");
        }
    }
}
