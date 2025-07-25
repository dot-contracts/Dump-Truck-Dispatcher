using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedDriverSettlements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketSettlements");

            migrationBuilder.DropTable(
                name: "DriverSettlement");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverSettlement",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    NumberOfDriversPaid = table.Column<short>(type: "smallint", nullable: false),
                    SettlementAmount = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    SettlementDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverSettlement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketSettlements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    DriverSettlementId = table.Column<int>(type: "int", nullable: false),
                    TicketId = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    PayAmount = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                    PayMethod = table.Column<int>(type: "int", nullable: true),
                    PayRate = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(16,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketSettlements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketSettlements_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketSettlements_DriverSettlement_DriverSettlementId",
                        column: x => x.DriverSettlementId,
                        principalTable: "DriverSettlement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketSettlements_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketSettlements_DriverId",
                table: "TicketSettlements",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSettlements_DriverSettlementId",
                table: "TicketSettlements",
                column: "DriverSettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketSettlements_TicketId",
                table: "TicketSettlements",
                column: "TicketId");
        }
    }
}
