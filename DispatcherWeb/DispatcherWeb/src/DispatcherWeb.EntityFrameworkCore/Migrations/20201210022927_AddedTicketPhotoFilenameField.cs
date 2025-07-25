using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedTicketPhotoFilenameField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TicketPhotoFilename",
                table: "Ticket",
                maxLength: 255,
                nullable: true);

            migrationBuilder.Sql(@"Update Ticket set TicketPhotoFilename = CONCAT(TicketPhotoId, '.jpg') where TicketPhotoId is not null");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TicketPhotoFilename",
                table: "Ticket");
        }
    }
}
