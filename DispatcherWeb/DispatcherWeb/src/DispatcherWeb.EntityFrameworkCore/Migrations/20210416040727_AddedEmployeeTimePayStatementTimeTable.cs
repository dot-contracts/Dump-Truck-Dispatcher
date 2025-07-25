using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedEmployeeTimePayStatementTimeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeTimePayStatementTime",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    EmployeeTimeId = table.Column<int>(nullable: false),
                    PayStatementTimeId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTimePayStatementTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTimePayStatementTime_EmployeeTime_EmployeeTimeId",
                        column: x => x.EmployeeTimeId,
                        principalTable: "EmployeeTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeTimePayStatementTime_PayStatementTime_PayStatementTimeId",
                        column: x => x.PayStatementTimeId,
                        principalTable: "PayStatementTime",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimePayStatementTime_EmployeeTimeId",
                table: "EmployeeTimePayStatementTime",
                column: "EmployeeTimeId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimePayStatementTime_PayStatementTimeId",
                table: "EmployeeTimePayStatementTime",
                column: "PayStatementTimeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeTimePayStatementTime");
        }
    }
}
