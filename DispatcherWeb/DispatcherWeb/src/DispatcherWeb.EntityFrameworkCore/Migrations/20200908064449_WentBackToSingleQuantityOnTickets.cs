using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class WentBackToSingleQuantityOnTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
update Ticket set Quantity = FreightQuantity, UnitOfMeasureId = FreightUomId
where FreightUomId is not null and FreightQuantity > 0");

            migrationBuilder.Sql(@"
update Ticket set Quantity = MaterialQuantity, UnitOfMeasureId = MaterialUomId
where (FreightUomId is null or FreightQuantity = 0) 
and MaterialUomId is not null and MaterialQuantity > 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
