using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RenamedSomeDateFieldsToDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "TruxEarnings",
                newName: "StartDateTime");

            migrationBuilder.RenameColumn(
                name: "TicketDate",
                table: "Ticket",
                newName: "TicketDateTime");

            migrationBuilder.RenameColumn(
                name: "DeliveryDate",
                table: "InvoiceLine",
                newName: "DeliveryDateTime");

            migrationBuilder.RenameColumn(
                name: "QuickbooksExportDate",
                table: "Invoice",
                newName: "QuickbooksExportDateTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDateTime",
                table: "TruxEarnings",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "TicketDateTime",
                table: "Ticket",
                newName: "TicketDate");

            migrationBuilder.RenameColumn(
                name: "DeliveryDateTime",
                table: "InvoiceLine",
                newName: "DeliveryDate");

            migrationBuilder.RenameColumn(
                name: "QuickbooksExportDateTime",
                table: "Invoice",
                newName: "QuickbooksExportDate");
        }
    }
}
