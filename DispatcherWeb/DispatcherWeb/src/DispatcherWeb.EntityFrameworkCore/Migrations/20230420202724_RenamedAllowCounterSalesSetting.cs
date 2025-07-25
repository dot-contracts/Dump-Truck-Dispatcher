using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RenamedAllowCounterSalesSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Update AbpSettings set Name = 'App.DispatchingAndMessaging.AllowCounterSalesForTenant' where Name = 'App.DispatchingAndMessaging.AllowCounterSales'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
