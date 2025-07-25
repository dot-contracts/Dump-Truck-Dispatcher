using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedIsCodFieldToCustomers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCod",
                table: "Customer",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
update c set c.IsCod = 1
from Customer c
inner join AbpSettings a on a.Name='App.General.CompanyName' and a.Value='N.W. White' and a.TenantId = c.TenantId
where c.AccountNumber is null or c.AccountNumber = '' or c.AccountNumber like 'cod%' or c.AccountNumber like 'cash%'
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCod",
                table: "Customer");
        }
    }
}
