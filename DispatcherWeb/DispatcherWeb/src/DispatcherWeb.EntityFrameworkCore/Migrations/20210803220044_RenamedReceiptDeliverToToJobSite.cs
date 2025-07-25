using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RenamedReceiptDeliverToToJobSite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "DeliverTo",
                table: "Receipt",
                newName: "JobSite");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "JobSite",
                table: "Receipt",
                newName: "DeliverTo");
        }
    }
}
