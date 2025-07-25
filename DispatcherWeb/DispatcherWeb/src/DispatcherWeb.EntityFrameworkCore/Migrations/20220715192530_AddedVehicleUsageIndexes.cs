using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedVehicleUsageIndexes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_VehicleUsage_IsDeleted_TenantId_ReadingType_ReadingDateTime]
                ON [dbo].[VehicleUsage] ([IsDeleted],[TenantId],[ReadingType],[ReadingDateTime])
                INCLUDE ([TruckId])
            ");

            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_VehicleUsage_ReadingType_ReadingDateTime]
                ON [dbo].[VehicleUsage] ([ReadingType],[ReadingDateTime])
                INCLUDE ([IsDeleted],[TenantId],[TruckId])
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
