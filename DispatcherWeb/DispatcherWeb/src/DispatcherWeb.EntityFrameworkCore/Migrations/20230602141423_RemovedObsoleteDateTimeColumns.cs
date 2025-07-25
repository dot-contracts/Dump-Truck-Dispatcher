using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedObsoleteDateTimeColumns : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Sequence",
                table: "OrderLineTruck");

            migrationBuilder.DropColumn(
                name: "TimeOnJobObsolete",
                table: "OrderLineTruck");

            migrationBuilder.DropColumn(
                name: "FirstStaggeredTimeOnJobObsolete",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "TimeOnJobObsolete",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "DefaultStartTimeObsolete",
                table: "Office");

            migrationBuilder.DropColumn(
                name: "TicketDateTimeObsolete",
                table: "LuckStoneEarnings");

            migrationBuilder.DropColumn(
                name: "TicketDateTimeWasConverted",
                table: "LuckStoneEarnings");

            migrationBuilder.DropColumn(
                name: "StartTimeObsolete",
                table: "DriverAssignment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Sequence",
                table: "OrderLineTruck",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOnJobObsolete",
                table: "OrderLineTruck",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FirstStaggeredTimeOnJobObsolete",
                table: "OrderLine",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOnJobObsolete",
                table: "OrderLine",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DefaultStartTimeObsolete",
                table: "Office",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TicketDateTimeObsolete",
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
                name: "StartTimeObsolete",
                table: "DriverAssignment",
                type: "datetime2",
                nullable: true);
        }
    }
}
