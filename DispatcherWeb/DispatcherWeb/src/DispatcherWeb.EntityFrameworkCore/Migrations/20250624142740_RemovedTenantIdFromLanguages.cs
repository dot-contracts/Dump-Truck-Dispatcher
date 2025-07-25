using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemovedTenantIdFromLanguages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpLanguages_TenantId_Name",
                table: "AbpLanguages");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "AbpLanguages");

            migrationBuilder.CreateIndex(
                name: "IX_AbpLanguages_Name",
                table: "AbpLanguages",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AbpLanguages_Name",
                table: "AbpLanguages");

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "AbpLanguages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AbpLanguages_TenantId_Name",
                table: "AbpLanguages",
                columns: new[] { "TenantId", "Name" });
        }
    }
}
