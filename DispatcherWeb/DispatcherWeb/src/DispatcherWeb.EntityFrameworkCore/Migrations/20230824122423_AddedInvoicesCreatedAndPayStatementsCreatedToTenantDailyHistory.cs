using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedInvoicesCreatedAndPayStatementsCreatedToTenantDailyHistory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InvoicesCreated",
                table: "TenantDailyHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PayStatementsCreated",
                table: "TenantDailyHistory",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InvoicesCreated",
                table: "TenantDailyHistory");

            migrationBuilder.DropColumn(
                name: "PayStatementsCreated",
                table: "TenantDailyHistory");
        }
    }
}
