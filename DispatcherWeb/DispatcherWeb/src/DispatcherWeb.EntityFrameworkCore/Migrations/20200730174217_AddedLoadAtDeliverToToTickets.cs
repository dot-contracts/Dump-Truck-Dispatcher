using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedLoadAtDeliverToToTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliverToId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LoadAtId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.Sql(@"update t set t.LoadAtId = ol.SupplierId, t.DeliverToId = ol.DeliverToId
from Ticket t
left join OrderLine ol on ol.Id = t.OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_DeliverToId",
                table: "Ticket",
                column: "DeliverToId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_LoadAtId",
                table: "Ticket",
                column: "LoadAtId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Supplier_DeliverToId",
                table: "Ticket",
                column: "DeliverToId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Supplier_LoadAtId",
                table: "Ticket",
                column: "LoadAtId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Supplier_DeliverToId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Supplier_LoadAtId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_DeliverToId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_LoadAtId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "DeliverToId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "LoadAtId",
                table: "Ticket");
        }
    }
}
