using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class SetOfficeCopyDeliverToLoadAtChargeToToFalse : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.Sql("Update Office set CopyDeliverToLoadAtChargeTo = 0");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
