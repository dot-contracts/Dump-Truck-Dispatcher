using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedDispatchWasMultipleLoadsField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "WasMultipleLoads",
                table: "Dispatch",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("Update Dispatch set WasMultipleLoads = IsMultipleLoads where IsMultipleLoads = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WasMultipleLoads",
                table: "Dispatch");
        }
    }
}
