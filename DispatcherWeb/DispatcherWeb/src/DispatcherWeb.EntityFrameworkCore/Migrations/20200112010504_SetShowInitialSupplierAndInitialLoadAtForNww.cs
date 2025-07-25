using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class SetShowInitialSupplierAndInitialLoadAtForNww : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
insert into AbpSettings (TenantId, Name, [Value], CreationTime)
select TenantId, 'App.General.ShowInitialSupplierAndInitialLoadAt', 'true', getDate()
from AbpSettings a where a.Name='App.General.CompanyName' and a.Value='N.W. White'
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
