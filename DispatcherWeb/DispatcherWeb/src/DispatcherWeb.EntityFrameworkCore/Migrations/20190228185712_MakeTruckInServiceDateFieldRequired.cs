using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class MakeTruckInServiceDateFieldRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("update [Truck] set InServiceDate = convert(date, CreationTime) where InServiceDate is null");

            migrationBuilder.AlterColumn<DateTime>(
                name: "InServiceDate",
                table: "Truck",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "InServiceDate",
                table: "Truck",
                nullable: true,
                oldClrType: typeof(DateTime));
        }
    }
}
