using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class MadeDriverApplicationLogFieldsOptional : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverApplicationLog_Driver_DriverId",
                table: "DriverApplicationLog");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverApplicationLog_AbpUsers_UserId",
                table: "DriverApplicationLog");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "DriverApplicationLog",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "DriverApplicationLog",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "DriverApplicationLog",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<Guid>(
                name: "DeviceGuid",
                table: "DriverApplicationLog",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverApplicationLog_Driver_DriverId",
                table: "DriverApplicationLog",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DriverApplicationLog_AbpUsers_UserId",
                table: "DriverApplicationLog",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DriverApplicationLog_Driver_DriverId",
                table: "DriverApplicationLog");

            migrationBuilder.DropForeignKey(
                name: "FK_DriverApplicationLog_AbpUsers_UserId",
                table: "DriverApplicationLog");

            migrationBuilder.DropColumn(
                name: "DeviceGuid",
                table: "DriverApplicationLog");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "DriverApplicationLog",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "DriverApplicationLog",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "DriverId",
                table: "DriverApplicationLog",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverApplicationLog_Driver_DriverId",
                table: "DriverApplicationLog",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DriverApplicationLog_AbpUsers_UserId",
                table: "DriverApplicationLog",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
