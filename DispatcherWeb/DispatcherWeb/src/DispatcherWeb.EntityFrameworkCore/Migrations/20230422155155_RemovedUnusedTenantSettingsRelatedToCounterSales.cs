using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedUnusedTenantSettingsRelatedToCounterSales : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE
                FROM AbpSettings
                WHERE UserId is null and Name in (
                    'App.DispatchingAndMessaging.DefaultDesignationToMaterialOnly',
                    'App.DispatchingAndMessaging.DefaultLoadAtLocationId',
                    'App.DispatchingAndMessaging.DefaultServiceId',
                    'App.DispatchingAndMessaging.DefaultMaterialUomId',
                    'App.DispatchingAndMessaging.DefaultAutoGenerateTicketNumber')
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
