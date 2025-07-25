using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedTaxRateToInvoices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTaxable",
                table: "InvoiceLine",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"Update InvoiceLine set IsTaxable = 1 where Tax > 0");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxRate",
                table: "Invoice",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTaxable",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "TaxRate",
                table: "Invoice");
        }
    }
}
