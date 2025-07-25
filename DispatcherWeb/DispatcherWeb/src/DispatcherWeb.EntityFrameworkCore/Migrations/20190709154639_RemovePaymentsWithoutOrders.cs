using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovePaymentsWithoutOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
delete from Payment where Id in (
  select p.Id
  FROM [dbo].[Payment] p
  left join OrderPayment op on op.PaymentId = p.Id
  where op.Id is null
)
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
