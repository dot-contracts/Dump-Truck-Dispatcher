using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class UpdateQuickbooksOnlineSetting : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update AbpSettings set Value = '3' where Name = 'App.Invoice.Quickbooks.IntegrationKind' and Value = '2'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
