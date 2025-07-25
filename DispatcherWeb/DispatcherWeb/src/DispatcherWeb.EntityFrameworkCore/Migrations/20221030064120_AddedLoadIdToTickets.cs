using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedLoadIdToTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Load_Ticket_TicketId",
                table: "Load");

            migrationBuilder.DropIndex(
                name: "IX_Load_TicketId",
                table: "Load");

            migrationBuilder.DropColumn(
                name: "DeliveryNote",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "PickupNote",
                table: "Dispatch");

            migrationBuilder.AddColumn<int>(
                name: "LoadId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_LoadId",
                table: "Ticket",
                column: "LoadId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Load_LoadId",
                table: "Ticket",
                column: "LoadId",
                principalTable: "Load",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                update t set LoadId = l.Id
                from Ticket t
                inner join Load l on l.TicketId = t.Id
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Load_LoadId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_LoadId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "LoadId",
                table: "Ticket");

            migrationBuilder.AddColumn<string>(
                name: "DeliveryNote",
                table: "Dispatch",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PickupNote",
                table: "Dispatch",
                type: "nvarchar(400)",
                maxLength: 400,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Load_TicketId",
                table: "Load",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Load_Ticket_TicketId",
                table: "Load",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id");
        }
    }
}
