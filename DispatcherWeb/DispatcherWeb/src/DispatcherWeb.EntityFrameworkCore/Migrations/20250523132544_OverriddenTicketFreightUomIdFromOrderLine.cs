using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class OverriddenTicketFreightUomIdFromOrderLine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HistoricalFreightUomId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                update t
                set HistoricalFreightUomId = t.FreightUomId, FreightUomId = ol.FreightUomId
                from Ticket t
                inner join OrderLine ol on ol.Id = t.OrderLineId
                where
                (t.FreightUomId is null and ol.FreightUomId is not null
                or t.FreightUomId is not null and ol.FreightUomId is null
                or t.FreightUomId != ol.FreightUomId)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HistoricalFreightUomId",
                table: "Ticket");
        }
    }
}
