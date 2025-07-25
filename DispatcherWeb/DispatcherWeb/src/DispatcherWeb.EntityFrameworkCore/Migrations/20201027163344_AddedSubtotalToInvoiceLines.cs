using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedSubtotalToInvoiceLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Subtotal",
                table: "InvoiceLine",
                type: "decimal(19, 4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.Sql("Update InvoiceLine set Subtotal = MaterialExtendedAmount + FreightExtendedAmount");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Subtotal",
                table: "InvoiceLine");
        }
    }
}
