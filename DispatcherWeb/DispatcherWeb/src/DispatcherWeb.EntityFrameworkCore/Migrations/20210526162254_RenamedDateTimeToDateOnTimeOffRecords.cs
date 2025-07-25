using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RenamedDateTimeToDateOnTimeOffRecords : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from TimeOff");

            migrationBuilder.RenameColumn(
                name: "StartDateTime",
                table: "TimeOff",
                newName: "StartDate");

            migrationBuilder.RenameColumn(
                name: "EndDateTime",
                table: "TimeOff",
                newName: "EndDate");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartDate",
                table: "TimeOff",
                newName: "StartDateTime");

            migrationBuilder.RenameColumn(
                name: "EndDate",
                table: "TimeOff",
                newName: "EndDateTime");
        }
    }
}
