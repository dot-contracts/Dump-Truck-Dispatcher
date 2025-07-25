using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class MarkAllExistingLeaseHaulersActive : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update LeaseHauler set Active = 1");

            migrationBuilder.RenameColumn(
                name: "Active",
                table: "LeaseHauler",
                newName: "IsActive");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsActive",
                table: "LeaseHauler",
                newName: "Active");
        }
    }
}
