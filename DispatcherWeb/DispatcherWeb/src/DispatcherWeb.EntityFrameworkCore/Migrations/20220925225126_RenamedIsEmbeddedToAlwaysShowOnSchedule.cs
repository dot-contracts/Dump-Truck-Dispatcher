using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RenamedIsEmbeddedToAlwaysShowOnSchedule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsEmbedded",
                table: "LeaseHaulerTruck",
                newName: "AlwaysShowOnSchedule");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AlwaysShowOnSchedule",
                table: "LeaseHaulerTruck",
                newName: "IsEmbedded");
        }
    }
}
