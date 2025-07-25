using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedImportedEarningsBatchStep2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LuckStoneEarnings_LuckStoneEarningsBatch_BatchId",
                table: "LuckStoneEarnings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LuckStoneEarningsBatch",
                table: "LuckStoneEarningsBatch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LuckStoneEarnings",
                table: "LuckStoneEarnings");

            migrationBuilder.RenameTable(
                name: "LuckStoneEarningsBatch",
                newName: "ImportedEarningsBatch");

            migrationBuilder.RenameTable(
                name: "LuckStoneEarnings",
                newName: "ImportedEarnings");

            migrationBuilder.RenameIndex(
                name: "IX_LuckStoneEarnings_BatchId",
                table: "ImportedEarnings",
                newName: "IX_ImportedEarnings_BatchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImportedEarningsBatch",
                table: "ImportedEarningsBatch",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ImportedEarnings",
                table: "ImportedEarnings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ImportedEarnings_ImportedEarningsBatch_BatchId",
                table: "ImportedEarnings",
                column: "BatchId",
                principalTable: "ImportedEarningsBatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ImportedEarnings_ImportedEarningsBatch_BatchId",
                table: "ImportedEarnings");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImportedEarningsBatch",
                table: "ImportedEarningsBatch");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ImportedEarnings",
                table: "ImportedEarnings");

            migrationBuilder.RenameTable(
                name: "ImportedEarningsBatch",
                newName: "LuckStoneEarningsBatch");

            migrationBuilder.RenameTable(
                name: "ImportedEarnings",
                newName: "LuckStoneEarnings");

            migrationBuilder.RenameIndex(
                name: "IX_ImportedEarnings_BatchId",
                table: "LuckStoneEarnings",
                newName: "IX_LuckStoneEarnings_BatchId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LuckStoneEarningsBatch",
                table: "LuckStoneEarningsBatch",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LuckStoneEarnings",
                table: "LuckStoneEarnings",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LuckStoneEarnings_LuckStoneEarningsBatch_BatchId",
                table: "LuckStoneEarnings",
                column: "BatchId",
                principalTable: "LuckStoneEarningsBatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
