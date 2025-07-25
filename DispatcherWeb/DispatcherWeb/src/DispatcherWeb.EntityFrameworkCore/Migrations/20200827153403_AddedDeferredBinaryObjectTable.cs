using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedDeferredBinaryObjectTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeferredTicketPhotoId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeferredSignatureId",
                table: "Load",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DeferredBinaryObject",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TenantId = table.Column<int>(nullable: true),
                    Destination = table.Column<int>(nullable: false),
                    BinaryObjectId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeferredBinaryObject", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_DeferredTicketPhotoId",
                table: "Ticket",
                column: "DeferredTicketPhotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Load_DeferredSignatureId",
                table: "Load",
                column: "DeferredSignatureId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeferredBinaryObject");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_DeferredTicketPhotoId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Load_DeferredSignatureId",
                table: "Load");

            migrationBuilder.DropColumn(
                name: "DeferredTicketPhotoId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "DeferredSignatureId",
                table: "Load");
        }
    }
}
