using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedHasAllActualAmountsFromOrdersAndOrderLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasAllActualAmounts",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "HasAllActualAmounts",
                table: "Order");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasAllActualAmounts",
                table: "OrderLine",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasAllActualAmounts",
                table: "Order",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
