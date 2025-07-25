using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedTicketToInvoiceLineFK : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_TicketId",
                table: "InvoiceLine");

            migrationBuilder.Sql("update InvoiceLine set TicketId = null");
            migrationBuilder.Sql("delete from InvoiceLine");
            migrationBuilder.Sql("delete from Invoice");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_TicketId",
                table: "InvoiceLine",
                column: "TicketId",
                unique: true,
                filter: "[TicketId] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_TicketId",
                table: "InvoiceLine");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_TicketId",
                table: "InvoiceLine",
                column: "TicketId");
        }
    }
}
