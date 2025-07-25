using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RenamedSupplierPermissions : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
update [AbpPermissions] set Name = 'Pages.Locations' where Name = 'Pages.Suppliers';
update [AbpPermissions] set Name = 'Pages.Locations.Merge' where Name = 'Pages.Suppliers.Merge';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
