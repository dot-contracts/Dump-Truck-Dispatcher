using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedAuditableColumnsToVehicleCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "VehicleCategory",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("Update VehicleCategory SET CreationTime = GETDATE();");

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "VehicleCategory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "VehicleCategory",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "VehicleCategory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "VehicleCategory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "VehicleCategory",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "VehicleCategory",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "VehicleCategory");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "VehicleCategory");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "VehicleCategory");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "VehicleCategory");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "VehicleCategory");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "VehicleCategory");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "VehicleCategory");
        }
    }
}
