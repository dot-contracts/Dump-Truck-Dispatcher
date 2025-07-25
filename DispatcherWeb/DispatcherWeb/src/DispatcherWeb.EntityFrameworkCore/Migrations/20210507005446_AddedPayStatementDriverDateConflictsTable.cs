using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedPayStatementDriverDateConflictsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PayStatementDriverDateConflict",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    TenantId = table.Column<int>(nullable: false),
                    PayStatementId = table.Column<int>(nullable: false),
                    DriverId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayStatementDriverDateConflict", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayStatementDriverDateConflict_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayStatementDriverDateConflict_PayStatement_PayStatementId",
                        column: x => x.PayStatementId,
                        principalTable: "PayStatement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementDriverDateConflict_DriverId",
                table: "PayStatementDriverDateConflict",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementDriverDateConflict_PayStatementId",
                table: "PayStatementDriverDateConflict",
                column: "PayStatementId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayStatementDriverDateConflict");
        }
    }
}
