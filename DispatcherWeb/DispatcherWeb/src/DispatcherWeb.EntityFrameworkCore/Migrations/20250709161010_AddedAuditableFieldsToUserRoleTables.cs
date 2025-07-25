using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedAuditableFieldsToUserRoleTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "AbpUserRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AbpUserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AbpUserRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AbpUserRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "AbpUserRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "AbpUserOrganizationUnits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AbpUserOrganizationUnits",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AbpUserOrganizationUnits",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "AbpUserOrganizationUnits",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreationTime",
                table: "AbpUserLogins",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.Sql("UPDATE AbpUserLogins SET CreationTime = GETDATE()");

            migrationBuilder.AddColumn<long>(
                name: "CreatorUserId",
                table: "AbpUserLogins",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "AbpUserLogins",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AbpUserLogins",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "AbpUserLogins",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AbpUserLogins",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "AbpUserLogins",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "DeleterUserId",
                table: "AbpOrganizationUnitRoles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletionTime",
                table: "AbpOrganizationUnitRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModificationTime",
                table: "AbpOrganizationUnitRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "LastModifierUserId",
                table: "AbpOrganizationUnitRoles",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "AbpUserRoles");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AbpUserRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AbpUserRoles");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AbpUserRoles");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "AbpUserRoles");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "AbpUserOrganizationUnits");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AbpUserOrganizationUnits");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AbpUserOrganizationUnits");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "AbpUserOrganizationUnits");

            migrationBuilder.DropColumn(
                name: "CreationTime",
                table: "AbpUserLogins");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                table: "AbpUserLogins");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "AbpUserLogins");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AbpUserLogins");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "AbpUserLogins");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AbpUserLogins");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "AbpUserLogins");

            migrationBuilder.DropColumn(
                name: "DeleterUserId",
                table: "AbpOrganizationUnitRoles");

            migrationBuilder.DropColumn(
                name: "DeletionTime",
                table: "AbpOrganizationUnitRoles");

            migrationBuilder.DropColumn(
                name: "LastModificationTime",
                table: "AbpOrganizationUnitRoles");

            migrationBuilder.DropColumn(
                name: "LastModifierUserId",
                table: "AbpOrganizationUnitRoles");
        }
    }
}
