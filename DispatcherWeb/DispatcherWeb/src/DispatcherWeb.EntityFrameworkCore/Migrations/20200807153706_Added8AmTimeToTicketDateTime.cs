using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class Added8AmTimeToTicketDateTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"update Ticket
set TicketDate = DateAdd(HOUR, 8, TicketDate)
where cast(TicketDate as time) = '00:00:00.0000000'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
