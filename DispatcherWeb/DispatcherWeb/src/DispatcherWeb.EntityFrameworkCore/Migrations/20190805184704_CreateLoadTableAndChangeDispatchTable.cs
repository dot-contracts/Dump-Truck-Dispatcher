using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class CreateLoadTableAndChangeDispatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Complete",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "DestinationLatitude",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "DestinationLongitude",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "Loaded",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "SourceLatitude",
                table: "Dispatch");

            migrationBuilder.DropColumn(
                name: "SourceLongitude",
                table: "Dispatch");

            migrationBuilder.AddColumn<bool>(
                name: "IsMultipleLoads",
                table: "Dispatch",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Load",
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
                    DispatchId = table.Column<int>(nullable: false),
                    SourceDateTime = table.Column<DateTime>(nullable: true),
                    DestinationDateTime = table.Column<DateTime>(nullable: true),
                    TicketId = table.Column<int>(nullable: true),
                    SourceLatitude = table.Column<double>(nullable: true),
                    SourceLongitude = table.Column<double>(nullable: true),
                    DestinationLatitude = table.Column<double>(nullable: true),
                    DestinationLongitude = table.Column<double>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Load", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Load_Dispatch_DispatchId",
                        column: x => x.DispatchId,
                        principalTable: "Dispatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Load_Ticket_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Ticket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Load_DispatchId",
                table: "Load",
                column: "DispatchId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_TicketId",
                table: "Load",
                column: "TicketId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Load");

            migrationBuilder.DropColumn(
                name: "IsMultipleLoads",
                table: "Dispatch");

            migrationBuilder.AddColumn<DateTime>(
                name: "Complete",
                table: "Dispatch",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLatitude",
                table: "Dispatch",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DestinationLongitude",
                table: "Dispatch",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "Loaded",
                table: "Dispatch",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SourceLatitude",
                table: "Dispatch",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "SourceLongitude",
                table: "Dispatch",
                nullable: true);
        }
    }
}
