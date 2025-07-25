using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedJobNumberToInvoiceLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "JobNumber",
                table: "InvoiceLine",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.Sql(@"
                update il set JobNumber = ol.JobNumber
                from InvoiceLine il
                inner join Ticket t on t.Id = il.TicketId
                inner join OrderLine ol on ol.Id = t.OrderLineId
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "JobNumber",
                table: "InvoiceLine");
        }
    }
}
