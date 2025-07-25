using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedOrganizationUnitIdFromOffices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                update Office set OrganizationUnitId = null
            ");

            migrationBuilder.DropForeignKey(
                name: "FK_Office_AbpOrganizationUnits_OrganizationUnitId",
                table: "Office");

            migrationBuilder.DropIndex(
                name: "IX_Office_OrganizationUnitId",
                table: "Office");

            migrationBuilder.DropColumn(
                name: "OrganizationUnitId",
                table: "Office");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrganizationUnitId",
                table: "Office",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Office_OrganizationUnitId",
                table: "Office",
                column: "OrganizationUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Office_AbpOrganizationUnits_OrganizationUnitId",
                table: "Office",
                column: "OrganizationUnitId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
