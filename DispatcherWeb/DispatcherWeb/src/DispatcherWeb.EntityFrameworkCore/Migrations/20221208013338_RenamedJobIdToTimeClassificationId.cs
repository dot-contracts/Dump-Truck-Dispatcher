using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RenamedJobIdToTimeClassificationId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_TimeClassification_JobId",
                table: "EmployeeTime");

            migrationBuilder.RenameColumn(
                name: "JobId",
                table: "EmployeeTime",
                newName: "TimeClassificationId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTime_JobId",
                table: "EmployeeTime",
                newName: "IX_EmployeeTime_TimeClassificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_TimeClassification_TimeClassificationId",
                table: "EmployeeTime",
                column: "TimeClassificationId",
                principalTable: "TimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql("update AbpSettings set Name = 'App.General.TimeTrackingDefaultTimeClassificationId' where Name = 'App.General.TimeTrackingDefaultJobId'");
            migrationBuilder.Sql("update AbpPermissions set Name = 'Pages.TimeEntry.EditTimeClassifications' where Name = 'Pages.TimeEntry.EditJobs'");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_TimeClassification_TimeClassificationId",
                table: "EmployeeTime");

            migrationBuilder.RenameColumn(
                name: "TimeClassificationId",
                table: "EmployeeTime",
                newName: "JobId");

            migrationBuilder.RenameIndex(
                name: "IX_EmployeeTime_TimeClassificationId",
                table: "EmployeeTime",
                newName: "IX_EmployeeTime_JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_TimeClassification_JobId",
                table: "EmployeeTime",
                column: "JobId",
                principalTable: "TimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
