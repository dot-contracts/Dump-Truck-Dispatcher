using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedShowFuelSurchargeOnInvoiceToInvoicesAndAddedParentInvoiceLineIdToInvoiceLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChildInvoiceLineKind",
                table: "InvoiceLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ParentInvoiceLineId",
                table: "InvoiceLine",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShowFuelSurchargeOnInvoice",
                table: "Invoice",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLine_ParentInvoiceLineId",
                table: "InvoiceLine",
                column: "ParentInvoiceLineId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_InvoiceLine_ParentInvoiceLineId",
                table: "InvoiceLine",
                column: "ParentInvoiceLineId",
                principalTable: "InvoiceLine",
                principalColumn: "Id");

            migrationBuilder.Sql(@"
                update l set l.Quantity = t.Quantity, l.ItemId = t.ServiceId
                from InvoiceLine l
                inner join Ticket t on t.Id = l.TicketId
                where l.Quantity != t.Quantity 
                or l.ItemId != t.ServiceId 
                or l.ItemId is null and t.ServiceId is not null
            ");
            migrationBuilder.Sql(@"
                update l set l.FreightRate = ol.FreightPricePerUnit, l.MaterialRate = ol.MaterialPricePerUnit
                from InvoiceLine l
                inner join Ticket t on t.Id = l.TicketId
                inner join OrderLine ol on ol.Id = t.OrderLineId
                where l.FreightRate is null and ol.FreightPricePerUnit is not null
                or l.MaterialRate is null and ol.MaterialPricePerUnit is not null
            ");

            migrationBuilder.Sql(@"
                delete from AbpSettings where name = 'App.Fuel.ShowFuelSurchargeOnInvoice' and value in ('1', '3')
            "); //1 is deprecated, 3 is the new default value (default values are not stored in the db)
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_InvoiceLine_ParentInvoiceLineId",
                table: "InvoiceLine");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceLine_ParentInvoiceLineId",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "ChildInvoiceLineKind",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "ParentInvoiceLineId",
                table: "InvoiceLine");

            migrationBuilder.DropColumn(
                name: "ShowFuelSurchargeOnInvoice",
                table: "Invoice");
        }
    }
}
