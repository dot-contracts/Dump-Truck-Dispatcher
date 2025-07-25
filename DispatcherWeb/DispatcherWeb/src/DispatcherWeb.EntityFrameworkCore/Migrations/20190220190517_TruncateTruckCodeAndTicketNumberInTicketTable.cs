using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class TruncateTruckCodeAndTicketNumberInTicketTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string sql = "update Ticket set TruckCode = SUBSTRING(TruckCode, 0, 25), TicketNumber = SUBSTRING(TicketNumber, 0, 20) where len(TruckCode) > 25 OR len(TicketNumber) > 20";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
