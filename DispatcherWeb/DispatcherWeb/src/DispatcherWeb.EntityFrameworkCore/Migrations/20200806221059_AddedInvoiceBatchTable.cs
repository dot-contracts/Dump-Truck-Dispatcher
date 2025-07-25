using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedInvoiceBatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BatchId",
                table: "Invoice",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvoiceBatch",
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
                    TenantId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceBatch", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_BatchId",
                table: "Invoice",
                column: "BatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_InvoiceBatch_BatchId",
                table: "Invoice",
                column: "BatchId",
                principalTable: "InvoiceBatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_InvoiceBatch_BatchId",
                table: "Invoice");

            migrationBuilder.DropTable(
                name: "InvoiceBatch");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_BatchId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "BatchId",
                table: "Invoice");
        }
    }
}
