using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class MigratedAllTicketsToTwoQuantities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var ticketQuery = @$"
                from Ticket t
                inner join OrderLine ol on ol.Id = t.OrderLineId
                where t.MaterialQuantity is null and t.FreightQuantity is null
            ";

            var designationIsMaterialOnly = "and ol.Designation in (2)";
            var designationIsFreightOnly = "and ol.Designation in (1, 5)";
            //var designationIsFreightAndMaterial = "and ol.Designation in (3, 9, 6, 7, 8)";

            migrationBuilder.Sql($@"
                update t set NonbillableFreight = 1, NonbillableMaterial = 1
                {ticketQuery}
                and t.Nonbillable = 1
            ");

            migrationBuilder.Sql($@"
                update t set NonbillableFreight = 1
                {ticketQuery}
                {designationIsMaterialOnly}
            ");

            migrationBuilder.Sql($@"
                update t set NonbillableMaterial = 1
                {ticketQuery}
                {designationIsFreightOnly}
            ");



            migrationBuilder.Sql($@"
                update t set MaterialQuantity = t.Quantity, FreightQuantity = 0
                {ticketQuery}
                {designationIsMaterialOnly}
            ");

            migrationBuilder.Sql($@"
                update t set MaterialQuantity = t.Quantity, FreightQuantity = t.Quantity
                {ticketQuery}
            ");



            migrationBuilder.Sql($@"
                update InvoiceLine set IsMaterialTaxable = IsFreightTaxable, FreightQuantity = Quantity, MaterialQuantity = Quantity
                where FreightQuantity is null and MaterialQuantity is null
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
