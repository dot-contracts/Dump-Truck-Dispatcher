using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class TableCreateDriverSettlementTicketSettlementAndChanges : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DriverId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                table: "Driver",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmploymentStartDate",
                table: "Driver",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseNumber",
                table: "Driver",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextPhysicalDueDate",
                table: "Driver",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayMethod",
                table: "Driver",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PayRate",
                table: "Driver",
                type: "decimal(16,4)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DriverSettlement",
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
                    SettlementDateTime = table.Column<DateTime>(nullable: false),
                    SettlementAmount = table.Column<decimal>(type: "decimal(16,2)", nullable: false),
                    NumberOfDriversPaid = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverSettlement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TicketSettlements",
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
                    DriverSettlementId = table.Column<int>(nullable: false),
                    TicketId = table.Column<int>(nullable: false),
                    DriverId = table.Column<int>(nullable: false),
                    PayMethod = table.Column<int>(nullable: true),
                    PayRate = table.Column<decimal>(type: "decimal(16,4)", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(16,2)", nullable: true),
                    PayAmount = table.Column<decimal>(type: "decimal(16,2)", nullable: true)
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
                name: "IX_Ticket_DriverId",
                table: "Ticket",
                column: "DriverId");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Driver_DriverId",
                table: "Ticket",
                column: "DriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Driver_DriverId",
                table: "Ticket");

            migrationBuilder.DropTable(
                name: "TicketSettlements");

            migrationBuilder.DropTable(
                name: "DriverSettlement");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_DriverId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "DriverId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "EmploymentStartDate",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "LicenseNumber",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "NextPhysicalDueDate",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "PayMethod",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "PayRate",
                table: "Driver");
        }
    }
}
