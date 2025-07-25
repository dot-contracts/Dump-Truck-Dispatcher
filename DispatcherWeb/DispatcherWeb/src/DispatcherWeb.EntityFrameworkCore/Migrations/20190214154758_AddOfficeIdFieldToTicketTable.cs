using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddOfficeIdFieldToTicketTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_OfficeId",
                table: "Ticket",
                column: "OfficeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Office_OfficeId",
                table: "Ticket",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Office_OfficeId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_OfficeId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "Ticket");
        }
    }
}
