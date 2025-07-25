using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedWorkorderIsCostOverriddenFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsTotalLaborCostOverridden",
                table: "WorkOrder",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsTotalPartsCostOverridden",
                table: "WorkOrder",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTotalLaborCostOverridden",
                table: "WorkOrder");

            migrationBuilder.DropColumn(
                name: "IsTotalPartsCostOverridden",
                table: "WorkOrder");
        }
    }
}
