using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class ClearTicketInvoiceLineFKForDeletedLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update InvoiceLine set TicketId = null where IsDeleted = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
