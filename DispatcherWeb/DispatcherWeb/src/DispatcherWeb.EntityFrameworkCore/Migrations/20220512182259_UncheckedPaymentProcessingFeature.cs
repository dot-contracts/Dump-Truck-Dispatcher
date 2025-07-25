using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class UncheckedPaymentProcessingFeature : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from AbpFeatures where Name = 'App.AllowPaymentProcessingFeature'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
