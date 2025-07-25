using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedLogoFieldsToOffices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LogoFileType",
                table: "Office",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LogoId",
                table: "Office",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReportsLogoFileType",
                table: "Office",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReportsLogoId",
                table: "Office",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ReportsLogoFileType",
                table: "AbpTenants",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LogoFileType",
                table: "Office");

            migrationBuilder.DropColumn(
                name: "LogoId",
                table: "Office");

            migrationBuilder.DropColumn(
                name: "ReportsLogoFileType",
                table: "Office");

            migrationBuilder.DropColumn(
                name: "ReportsLogoId",
                table: "Office");

            migrationBuilder.AlterColumn<string>(
                name: "ReportsLogoFileType",
                table: "AbpTenants",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);
        }
    }
}
