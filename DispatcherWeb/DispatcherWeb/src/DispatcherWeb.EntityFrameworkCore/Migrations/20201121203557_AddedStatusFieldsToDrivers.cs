using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedStatusFieldsToDrivers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "LastMvrDate",
                table: "Driver",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastPhysicalDate",
                table: "Driver",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LicenseExpirationDate",
                table: "Driver",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMvrDueDate",
                table: "Driver",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeOfLicense",
                table: "Driver",
                maxLength: 50,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastMvrDate",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "LastPhysicalDate",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "LicenseExpirationDate",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "NextMvrDueDate",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "TypeOfLicense",
                table: "Driver");
        }
    }
}
