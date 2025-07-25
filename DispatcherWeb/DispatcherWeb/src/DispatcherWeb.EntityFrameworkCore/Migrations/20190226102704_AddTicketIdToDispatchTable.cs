using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddTicketIdToDispatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TicketId",
                table: "Dispatch",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dispatch_TicketId",
                table: "Dispatch",
                column: "TicketId");

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatch_Ticket_TicketId",
                table: "Dispatch",
                column: "TicketId",
                principalTable: "Ticket",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatch_Ticket_TicketId",
                table: "Dispatch");

            migrationBuilder.DropIndex(
                name: "IX_Dispatch_TicketId",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "TicketId",
                table: "Dispatch");
        }
    }
}
