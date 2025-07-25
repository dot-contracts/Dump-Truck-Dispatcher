using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RemovedTrailerAssignmentTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Truck_Truck_DefaultTrailerId",
                table: "Truck");

            migrationBuilder.Sql("Delete from TrailerAssignment");
            migrationBuilder.DropTable(
                name: "TrailerAssignment");

            migrationBuilder.RenameColumn(
                name: "DefaultTrailerId",
                table: "Truck",
                newName: "CurrentTrailerId");

            migrationBuilder.RenameIndex(
                name: "IX_Truck_DefaultTrailerId",
                table: "Truck",
                newName: "IX_Truck_CurrentTrailerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Truck_Truck_CurrentTrailerId",
                table: "Truck",
                column: "CurrentTrailerId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Truck_Truck_CurrentTrailerId",
                table: "Truck");

            migrationBuilder.RenameColumn(
                name: "CurrentTrailerId",
                table: "Truck",
                newName: "DefaultTrailerId");

            migrationBuilder.RenameIndex(
                name: "IX_Truck_CurrentTrailerId",
                table: "Truck",
                newName: "IX_Truck_DefaultTrailerId");

            migrationBuilder.CreateTable(
                name: "TrailerAssignment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficeId = table.Column<int>(type: "int", nullable: true),
                    TractorId = table.Column<int>(type: "int", nullable: false),
                    TrailerId = table.Column<int>(type: "int", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    Shift = table.Column<byte>(type: "tinyint", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrailerAssignment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrailerAssignment_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrailerAssignment_Truck_TractorId",
                        column: x => x.TractorId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TrailerAssignment_Truck_TrailerId",
                        column: x => x.TrailerId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrailerAssignment_OfficeId",
                table: "TrailerAssignment",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrailerAssignment_TractorId",
                table: "TrailerAssignment",
                column: "TractorId");

            migrationBuilder.CreateIndex(
                name: "IX_TrailerAssignment_TrailerId",
                table: "TrailerAssignment",
                column: "TrailerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Truck_Truck_DefaultTrailerId",
                table: "Truck",
                column: "DefaultTrailerId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
