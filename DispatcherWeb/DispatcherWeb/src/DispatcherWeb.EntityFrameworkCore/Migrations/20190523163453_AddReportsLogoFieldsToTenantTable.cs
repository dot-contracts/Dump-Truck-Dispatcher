using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddReportsLogoFieldsToTenantTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReportsLogoFileType",
                table: "AbpTenants",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReportsLogoId",
                table: "AbpTenants",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportsLogoFileType",
                table: "AbpTenants");

            migrationBuilder.DropColumn(
                name: "ReportsLogoId",
                table: "AbpTenants");
        }
    }
}
