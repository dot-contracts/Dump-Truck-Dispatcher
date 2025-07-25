using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedPayStatementTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "PayRate",
                table: "Driver",
                type: "money",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(16,4)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PayStatement",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    OfficeId = table.Column<int>(nullable: false),
                    StatementDate = table.Column<DateTime>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false),
                    IncludeHourly = table.Column<bool>(nullable: false),
                    IncludeSalary = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayStatement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PayStatementDetail",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    PayStatementId = table.Column<int>(nullable: false),
                    DriverId = table.Column<int>(nullable: false),
                    PayMethod = table.Column<int>(nullable: false),
                    PayRate = table.Column<decimal>(type: "money", nullable: false),
                    Total = table.Column<decimal>(type: "money", nullable: false),
                    PayAmount = table.Column<decimal>(type: "money", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayStatementDetail", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayStatementDetail_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayStatementDetail_PayStatement_PayStatementId",
                        column: x => x.PayStatementId,
                        principalTable: "PayStatement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayStatementTicket",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    PayStatementDetailId = table.Column<int>(nullable: false),
                    TicketId = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(nullable: false),
                    FreightRate = table.Column<decimal>(nullable: false),
                    Total = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayStatementTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayStatementTicket_PayStatementDetail_PayStatementDetailId",
                        column: x => x.PayStatementDetailId,
                        principalTable: "PayStatementDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PayStatementTicket_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PayStatementTime",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    PayStatementDetailId = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Quantity = table.Column<decimal>(nullable: false),
                    Total = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PayStatementTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PayStatementTime_PayStatementDetail_PayStatementDetailId",
                        column: x => x.PayStatementDetailId,
                        principalTable: "PayStatementDetail",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementDetail_DriverId",
                table: "PayStatementDetail",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementDetail_PayStatementId",
                table: "PayStatementDetail",
                column: "PayStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementTicket_PayStatementDetailId",
                table: "PayStatementTicket",
                column: "PayStatementDetailId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementTicket_TicketId",
                table: "PayStatementTicket",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_PayStatementTime_PayStatementDetailId",
                table: "PayStatementTime",
                column: "PayStatementDetailId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PayStatementTicket");

            migrationBuilder.DropTable(
                name: "PayStatementTime");

            migrationBuilder.DropTable(
                name: "PayStatementDetail");

            migrationBuilder.DropTable(
                name: "PayStatement");

            migrationBuilder.AlterColumn<decimal>(
                name: "PayRate",
                table: "Driver",
                type: "decimal(16,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "money",
                oldNullable: true);
        }
    }
}
