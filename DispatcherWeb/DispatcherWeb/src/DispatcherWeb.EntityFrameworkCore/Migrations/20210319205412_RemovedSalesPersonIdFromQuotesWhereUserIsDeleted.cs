using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedSalesPersonIdFromQuotesWhereUserIsDeleted : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
update q set SalesPersonId = null
from Quote q
left join AbpUsers u on u.Id = q.SalesPersonId
where u.IsDeleted = 1
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
