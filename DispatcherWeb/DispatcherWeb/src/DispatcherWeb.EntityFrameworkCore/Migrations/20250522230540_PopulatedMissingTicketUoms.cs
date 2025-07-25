using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class PopulatedMissingTicketUoms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                update OrderLine
                set MaterialUomId = FreightUomId
                where MaterialUomId is null
            ");

            migrationBuilder.Sql(@"
                update t
                set MaterialUomId = ol.MaterialUomId
                from Ticket t
                inner join OrderLine ol on ol.Id = t.OrderLineId
                where t.MaterialUomId is null
                and ol.MaterialUomId is not null
            ");

            migrationBuilder.Sql(@"
                update Ticket
                set MaterialUomId = FreightUomId
                where MaterialUomId is null and OrderLineId is null
            ");

            migrationBuilder.Sql(@"
                update Ticket
                set MaterialQuantity = Quantity, FreightQuantity = Quantity
                where OrderLineId is null
                and MaterialQuantity is null and FreightQuantity is null
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
