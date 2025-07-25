using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedInvoiceLineIsTaxable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update InvoiceLine set IsTaxable = 0");

            migrationBuilder.DropColumn(
                name: "IsTaxable",
                table: "InvoiceLine");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTaxable",
                table: "InvoiceLine",
                nullable: false,
                defaultValue: false);
        }
    }
}
