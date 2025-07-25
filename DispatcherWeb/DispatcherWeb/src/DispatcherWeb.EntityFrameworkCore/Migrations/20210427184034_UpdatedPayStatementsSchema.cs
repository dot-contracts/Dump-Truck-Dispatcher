using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class UpdatedPayStatementsSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from PayStatementTicket");
            migrationBuilder.Sql("delete from EmployeeTimePayStatementTime");
            migrationBuilder.Sql("delete from PayStatementTime");
            migrationBuilder.Sql("delete from PayStatementDetail");
            migrationBuilder.Sql("delete from PayStatement");

            migrationBuilder.DropColumn(
                name: "PayAmount",
                table: "PayStatementDetail");

            migrationBuilder.DropColumn(
                name: "PayMethod",
                table: "PayStatementDetail");

            migrationBuilder.DropColumn(
                name: "PayRate",
                table: "PayStatementDetail");

            migrationBuilder.AddColumn<decimal>(
                name: "DriverPayRate",
                table: "Ticket",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeClassificationId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DriverPayRate",
                table: "PayStatementTime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TimeClassificationId",
                table: "PayStatementTime",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "DriverPayRate",
                table: "PayStatementTicket",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TimeClassificationId",
                table: "PayStatementTicket",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductionBasedTotal",
                table: "PayStatementDetail",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "TimeBasedTotal",
                table: "PayStatementDetail",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_TimeClassificationId",
                table: "Ticket",
                column: "TimeClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementTime_TimeClassificationId",
                table: "PayStatementTime",
                column: "TimeClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementTicket_TimeClassificationId",
                table: "PayStatementTicket",
                column: "TimeClassificationId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Job_TimeClassificationId",
                table: "Ticket",
                column: "TimeClassificationId",
                principalTable: "Job",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayStatementTicket_Job_TimeClassificationId",
                table: "PayStatementTicket");

            migrationBuilder.DropForeignKey(
                name: "FK_PayStatementTime_Job_TimeClassificationId",
                table: "PayStatementTime");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Job_TimeClassificationId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_TimeClassificationId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_PayStatementTime_TimeClassificationId",
                table: "PayStatementTime");

            migrationBuilder.DropIndex(
                name: "IX_PayStatementTicket_TimeClassificationId",
                table: "PayStatementTicket");

            migrationBuilder.DropColumn(
                name: "DriverPayRate",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "TimeClassificationId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "DriverPayRate",
                table: "PayStatementTime");

            migrationBuilder.DropColumn(
                name: "TimeClassificationId",
                table: "PayStatementTime");

            migrationBuilder.DropColumn(
                name: "DriverPayRate",
                table: "PayStatementTicket");

            migrationBuilder.DropColumn(
                name: "TimeClassificationId",
                table: "PayStatementTicket");

            migrationBuilder.DropColumn(
                name: "ProductionBasedTotal",
                table: "PayStatementDetail");

            migrationBuilder.DropColumn(
                name: "TimeBasedTotal",
                table: "PayStatementDetail");

            migrationBuilder.AddColumn<decimal>(
                name: "PayAmount",
                table: "PayStatementDetail",
                type: "money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PayMethod",
                table: "PayStatementDetail",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PayRate",
                table: "PayStatementDetail",
                type: "money",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
