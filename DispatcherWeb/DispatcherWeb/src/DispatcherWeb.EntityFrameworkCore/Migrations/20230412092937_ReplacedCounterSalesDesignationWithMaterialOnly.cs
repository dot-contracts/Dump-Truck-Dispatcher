using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class ReplacedCounterSalesDesignationWithMaterialOnly : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Update OrderLine set Designation = 2 where Designation = 10");
            migrationBuilder.Sql("Update ReceiptLine set Designation = 2 where Designation = 10");
            migrationBuilder.Sql("Update ProjectService set Designation = 2 where Designation = 10");
            migrationBuilder.Sql("Update QuoteService set Designation = 2 where Designation = 10");
            migrationBuilder.Sql("Update OfficeServicePrice set Designation = 2 where Designation = 10");
            migrationBuilder.Sql("Update AbpSettings set [Name] = 'App.DispatchingAndMessaging.DefaultDesignationToMaterialOnly' where [Name] = 'App.DispatchingAndMessaging.DefaultDesignationToCounterSales'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
