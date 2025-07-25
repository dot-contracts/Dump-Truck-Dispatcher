using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class PopulateHistoricalInvoiceLineItemIdFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"

update l set l.ItemId = t.ServiceId, l.Quantity = t.Quantity, l.FreightRate = ol.FreightPricePerUnit, l.MaterialRate = ol.MaterialPricePerUnit
from InvoiceLine l
inner join Ticket t on t.Id = l.TicketId
inner join OrderLine ol on ol.Id = t.OrderLineId
where l.ItemId is null

");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
