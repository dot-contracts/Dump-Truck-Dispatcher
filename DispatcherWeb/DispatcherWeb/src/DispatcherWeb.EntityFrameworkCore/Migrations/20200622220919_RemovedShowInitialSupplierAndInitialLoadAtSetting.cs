using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedShowInitialSupplierAndInitialLoadAtSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from AbpSettings where [Name] = 'App.General.ShowInitialSupplierAndInitialLoadAt'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
