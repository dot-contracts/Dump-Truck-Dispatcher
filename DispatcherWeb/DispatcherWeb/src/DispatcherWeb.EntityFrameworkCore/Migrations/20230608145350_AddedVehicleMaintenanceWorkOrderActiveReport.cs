using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedVehicleMaintenanceWorkOrderActiveReport : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var commandText = @"
SET IDENTITY_INSERT [dbo].[ActiveReport] ON;
INSERT INTO [dbo].[ActiveReport] (ID, [Name], [Description], [Path], CategoryId, CreationTime, IsDeleted)
VALUES (2, 'VehicleMaintenanceWorkOrderReport', 'Vehicle Maintenance Work Order Report', 'VehicleMaintenanceWorkOrderReport', 1, GETDATE(), 0);
SET IDENTITY_INSERT [dbo].[ActiveReport] OFF;
";

            migrationBuilder.Sql(commandText);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
