using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class PopulateMissingDriverIdOnTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
update t set t.DriverId = da.DriverId
from Ticket t
left join OrderLine ol on ol.Id = t.OrderLineId
left join [Order] o on o.Id = ol.OrderId
left join DriverAssignment da on da.[Date] = o.[DateTime] and da.[Shift] = o.[Shift] and da.TruckId = t.TruckId
where t.OrderLineId is not null
and t.DriverId is null
and t.TruckId is not null
and da.DriverId is not null
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
