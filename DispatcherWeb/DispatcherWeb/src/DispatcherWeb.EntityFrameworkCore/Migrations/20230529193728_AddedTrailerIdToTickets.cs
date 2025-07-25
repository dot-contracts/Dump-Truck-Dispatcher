using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedTrailerIdToTickets : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Truck_TruckId",
                table: "Ticket");

            migrationBuilder.AddColumn<int>(
                name: "TrailerId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_TrailerId",
                table: "Ticket",
                column: "TrailerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Truck_TrailerId",
                table: "Ticket",
                column: "TrailerId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Truck_TruckId",
                table: "Ticket",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Truck_TrailerId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Truck_TruckId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_TrailerId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "TrailerId",
                table: "Ticket");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Truck_TruckId",
                table: "Ticket",
                column: "TruckId",
                principalTable: "Truck",
                principalColumn: "Id");
        }
    }
}
