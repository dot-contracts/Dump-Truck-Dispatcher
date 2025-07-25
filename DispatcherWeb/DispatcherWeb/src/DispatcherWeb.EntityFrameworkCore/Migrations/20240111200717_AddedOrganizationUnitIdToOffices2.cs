using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedOrganizationUnitIdToOffices2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrganizationUnitId",
                table: "Office",
                type: "bigint",
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
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
    }
}
