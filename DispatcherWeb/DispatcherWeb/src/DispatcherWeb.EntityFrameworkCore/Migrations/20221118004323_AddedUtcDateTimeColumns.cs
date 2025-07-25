using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedUtcDateTimeColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOnJob",
                table: "OrderLineTruck",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstStaggeredTimeOnJob",
                table: "OrderLine",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOnJob",
                table: "OrderLine",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DefaultStartTime",
                table: "Office",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TicketDateTime",
                table: "LuckStoneEarnings",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<bool>(
                name: "TicketDateTimeWasConverted",
                table: "LuckStoneEarnings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "DriverAssignment",
                type: "datetime2",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeOnJob",
                table: "OrderLineTruck");

            migrationBuilder.DropColumn(
                name: "FirstStaggeredTimeOnJob",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "TimeOnJob",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "DefaultStartTime",
                table: "Office");

            migrationBuilder.DropColumn(
                name: "TicketDateTime",
                table: "LuckStoneEarnings");

            migrationBuilder.DropColumn(
                name: "TicketDateTimeWasConverted",
                table: "LuckStoneEarnings");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "DriverAssignment");
        }
    }
}
