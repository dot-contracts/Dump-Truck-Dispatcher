using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class ClearedTicketReceiptLineIdFKForDeletedReceiptLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"update t
set t.ReceiptLineId = NULL
from Ticket t
left join ReceiptLine rl on rl.Id = t.ReceiptLineId
where rl.Id is not null and rl.IsDeleted = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
