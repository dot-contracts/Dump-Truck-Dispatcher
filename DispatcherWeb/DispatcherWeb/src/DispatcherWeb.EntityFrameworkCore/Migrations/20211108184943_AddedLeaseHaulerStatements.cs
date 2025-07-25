using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedLeaseHaulerStatements : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Customer_CarrierId",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Customer_CarrierId",
                table: "Ticket");

            migrationBuilder.Sql("update Ticket set CarrierId = null where CarrierId is not null");
            migrationBuilder.Sql("update InvoiceLine set CarrierId = null where CarrierId is not null");

            migrationBuilder.AddColumn<decimal>(
                name: "LeaseHaulerRate",
                table: "QuoteService",
                type: "decimal(19, 4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "LeaseHaulerRate",
                table: "OrderLine",
                type: "decimal(19, 4)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LeaseHaulerStatement",
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
                    StatementDate = table.Column<DateTime>(nullable: false),
                    StartDate = table.Column<DateTime>(nullable: false),
                    EndDate = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerStatement", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeaseHaulerStatementTicket",
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
                    LeaseHaulerStatementId = table.Column<int>(nullable: false),
                    TicketId = table.Column<int>(nullable: false),
                    LeaseHaulerId = table.Column<int>(nullable: false),
                    TruckId = table.Column<int>(nullable: true),
                    Quantity = table.Column<decimal>(nullable: false),
                    Rate = table.Column<decimal>(nullable: true),
                    BrokerFee = table.Column<decimal>(nullable: false),
                    ExtendedAmount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerStatementTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerStatementTicket_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerStatementTicket_LeaseHaulerStatement_LeaseHaulerStatementId",
                        column: x => x.LeaseHaulerStatementId,
                        principalTable: "LeaseHaulerStatement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerStatementTicket_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerStatementTicket_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerStatementTicket_LeaseHaulerId",
                table: "LeaseHaulerStatementTicket",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerStatementTicket_LeaseHaulerStatementId",
                table: "LeaseHaulerStatementTicket",
                column: "LeaseHaulerStatementId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerStatementTicket_TicketId",
                table: "LeaseHaulerStatementTicket",
                column: "TicketId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerStatementTicket_TruckId",
                table: "LeaseHaulerStatementTicket",
                column: "TruckId");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_LeaseHauler_CarrierId",
                table: "InvoiceLine",
                column: "CarrierId",
                principalTable: "LeaseHauler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_LeaseHauler_CarrierId",
                table: "Ticket",
                column: "CarrierId",
                principalTable: "LeaseHauler",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_LeaseHauler_CarrierId",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_LeaseHauler_CarrierId",
                table: "Ticket");

            migrationBuilder.DropTable(
                name: "LeaseHaulerStatementTicket");

            migrationBuilder.DropTable(
                name: "LeaseHaulerStatement");

            migrationBuilder.DropColumn(
                name: "LeaseHaulerRate",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "LeaseHaulerRate",
                table: "OrderLine");

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Customer_CarrierId",
                table: "InvoiceLine",
                column: "CarrierId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Customer_CarrierId",
                table: "Ticket",
                column: "CarrierId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
