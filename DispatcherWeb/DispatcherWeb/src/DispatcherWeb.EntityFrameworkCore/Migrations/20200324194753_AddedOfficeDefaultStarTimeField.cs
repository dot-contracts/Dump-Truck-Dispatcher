using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedOfficeDefaultStarTimeField : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatch_AbpUsers_UserId",
                table: "Dispatch");

            migrationBuilder.AddColumn<DateTime>(
                name: "DefaultStartTime",
                table: "Office",
                nullable: false,
                defaultValue: new DateTime(2000, 1, 1, 7, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Dispatch",
                nullable: true,
                oldClrType: typeof(long));

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatch_AbpUsers_UserId",
                table: "Dispatch",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Dispatch_AbpUsers_UserId",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "DefaultStartTime",
                table: "Office");

            migrationBuilder.AlterColumn<long>(
                name: "UserId",
                table: "Dispatch",
                nullable: false,
                oldClrType: typeof(long),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Dispatch_AbpUsers_UserId",
                table: "Dispatch",
                column: "UserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
