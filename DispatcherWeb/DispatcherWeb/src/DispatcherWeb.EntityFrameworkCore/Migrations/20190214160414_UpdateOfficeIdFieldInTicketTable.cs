using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class UpdateOfficeIdFieldInTicketTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string sql = @"update T
                set T.OfficeId = O.LocationId
                from Ticket T 
                left join OrderLine OL on T.OrderLineId = OL.Id 
                left join [Order] O on OL.OrderId = O.Id 
                where T.OfficeId is null
                ";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
