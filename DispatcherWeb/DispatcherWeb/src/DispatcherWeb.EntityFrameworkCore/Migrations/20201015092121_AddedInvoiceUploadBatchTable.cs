using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedInvoiceUploadBatchTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "UploadBatchId",
                table: "Invoice",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InvoiceUploadBatch",
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
                    FileGuid = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceUploadBatch", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Invoice_UploadBatchId",
                table: "Invoice",
                column: "UploadBatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Invoice_InvoiceUploadBatch_UploadBatchId",
                table: "Invoice",
                column: "UploadBatchId",
                principalTable: "InvoiceUploadBatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invoice_InvoiceUploadBatch_UploadBatchId",
                table: "Invoice");

            migrationBuilder.DropTable(
                name: "InvoiceUploadBatch");

            migrationBuilder.DropIndex(
                name: "IX_Invoice_UploadBatchId",
                table: "Invoice");

            migrationBuilder.DropColumn(
                name: "UploadBatchId",
                table: "Invoice");
        }
    }
}
