using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RenamedNonUtcDateTimeColumnsToObsolete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeOnJob",
                table: "OrderLineTruck",
                newName: "TimeOnJobObsolete");

            migrationBuilder.RenameColumn(
                name: "TimeOnJob",
                table: "OrderLine",
                newName: "TimeOnJobObsolete");

            migrationBuilder.RenameColumn(
                name: "FirstStaggeredTimeOnJob",
                table: "OrderLine",
                newName: "FirstStaggeredTimeOnJobObsolete");

            migrationBuilder.RenameColumn(
                name: "DefaultStartTime",
                table: "Office",
                newName: "DefaultStartTimeObsolete");

            migrationBuilder.RenameColumn(
                name: "TicketDateTime",
                table: "LuckStoneEarnings",
                newName: "TicketDateTimeObsolete");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "DriverAssignment",
                newName: "StartTimeObsolete");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Dispatch",
                type: "nvarchar(550)",
                maxLength: 550,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeOnJobObsolete",
                table: "OrderLineTruck",
                newName: "TimeOnJob");

            migrationBuilder.RenameColumn(
                name: "TimeOnJobObsolete",
                table: "OrderLine",
                newName: "TimeOnJob");

            migrationBuilder.RenameColumn(
                name: "FirstStaggeredTimeOnJobObsolete",
                table: "OrderLine",
                newName: "FirstStaggeredTimeOnJob");

            migrationBuilder.RenameColumn(
                name: "DefaultStartTimeObsolete",
                table: "Office",
                newName: "DefaultStartTime");

            migrationBuilder.RenameColumn(
                name: "TicketDateTimeObsolete",
                table: "LuckStoneEarnings",
                newName: "TicketDateTime");

            migrationBuilder.RenameColumn(
                name: "StartTimeObsolete",
                table: "DriverAssignment",
                newName: "StartTime");

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Dispatch",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(550)",
                oldMaxLength: 550,
                oldNullable: true);
        }
    }
}
