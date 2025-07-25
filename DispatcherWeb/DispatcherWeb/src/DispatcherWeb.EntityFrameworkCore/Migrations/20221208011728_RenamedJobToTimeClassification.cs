using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RenamedJobToTimeClassification : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_Job_JobId",
                table: "EmployeeTime");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeClassification_Job_TimeClassificationId",
                table: "EmployeeTimeClassification");

            migrationBuilder.DropForeignKey(
                name: "FK_PayStatementTicket_Job_TimeClassificationId",
                table: "PayStatementTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_PayStatementTime_Job_TimeClassificationId",
                table: "PayStatementTime");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Job",
                table: "Job");

            migrationBuilder.RenameTable(
                name: "Job",
                newName: "TimeClassification");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TimeClassification",
                table: "TimeClassification",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_TimeClassification_JobId",
                table: "EmployeeTime",
                column: "JobId",
                principalTable: "TimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeClassification_TimeClassification_TimeClassificationId",
                table: "EmployeeTimeClassification",
                column: "TimeClassificationId",
                principalTable: "TimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayStatementTicket_TimeClassification_TimeClassificationId",
                table: "PayStatementTicket",
                column: "TimeClassificationId",
                principalTable: "TimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayStatementTime_TimeClassification_TimeClassificationId",
                table: "PayStatementTime",
                column: "TimeClassificationId",
                principalTable: "TimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_TimeClassification_JobId",
                table: "EmployeeTime");

            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTimeClassification_TimeClassification_TimeClassificationId",
                table: "EmployeeTimeClassification");

            migrationBuilder.DropForeignKey(
                name: "FK_PayStatementTicket_TimeClassification_TimeClassificationId",
                table: "PayStatementTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_PayStatementTime_TimeClassification_TimeClassificationId",
                table: "PayStatementTime");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TimeClassification",
                table: "TimeClassification");

            migrationBuilder.RenameTable(
                name: "TimeClassification",
                newName: "Job");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Job",
                table: "Job",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_Job_JobId",
                table: "EmployeeTime",
                column: "JobId",
                principalTable: "Job",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTimeClassification_Job_TimeClassificationId",
                table: "EmployeeTimeClassification",
                column: "TimeClassificationId",
                principalTable: "Job",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayStatementTicket_Job_TimeClassificationId",
                table: "PayStatementTicket",
                column: "TimeClassificationId",
                principalTable: "Job",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PayStatementTime_Job_TimeClassificationId",
                table: "PayStatementTime",
                column: "TimeClassificationId",
                principalTable: "Job",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
