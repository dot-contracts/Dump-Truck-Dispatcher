using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class ChangeFieldsInEmployeeTimeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_AbpUsers_UserId",
                table: "EmployeeTime");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "EmployeeTime",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDateTime",
                table: "EmployeeTime",
                nullable: true,
                oldClrType: typeof(DateTime));

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_AbpUsers_UserId",
                table: "EmployeeTime",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_AbpUsers_UserId",
                table: "EmployeeTime");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "EmployeeTime",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDateTime",
                table: "EmployeeTime",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_AbpUsers_UserId",
                table: "EmployeeTime",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
