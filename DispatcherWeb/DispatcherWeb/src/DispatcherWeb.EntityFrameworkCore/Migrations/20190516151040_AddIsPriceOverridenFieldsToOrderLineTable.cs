using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddIsPriceOverridenFieldsToOrderLineTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFreightPriceOverridden",
                table: "OrderLine",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMaterialPriceOverridden",
                table: "OrderLine",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFreightPriceOverridden",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "IsMaterialPriceOverridden",
                table: "OrderLine");
        }
    }
}
