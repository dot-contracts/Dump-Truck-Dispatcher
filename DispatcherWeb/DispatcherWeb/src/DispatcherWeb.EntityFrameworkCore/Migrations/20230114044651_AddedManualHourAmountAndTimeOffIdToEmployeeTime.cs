using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedManualHourAmountAndTimeOffIdToEmployeeTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowForManualTime",
                table: "EmployeeTimeClassification",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ManualHourAmount",
                table: "EmployeeTime",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeOffId",
                table: "EmployeeTime",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTime_TimeOffId",
                table: "EmployeeTime",
                column: "TimeOffId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_TimeOff_TimeOffId",
                table: "EmployeeTime",
                column: "TimeOffId",
                principalTable: "TimeOff",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_TimeOff_TimeOffId",
                table: "EmployeeTime");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTime_TimeOffId",
                table: "EmployeeTime");

            migrationBuilder.DropColumn(
                name: "AllowForManualTime",
                table: "EmployeeTimeClassification");

            migrationBuilder.DropColumn(
                name: "ManualHourAmount",
                table: "EmployeeTime");

            migrationBuilder.DropColumn(
                name: "TimeOffId",
                table: "EmployeeTime");
        }
    }
}
