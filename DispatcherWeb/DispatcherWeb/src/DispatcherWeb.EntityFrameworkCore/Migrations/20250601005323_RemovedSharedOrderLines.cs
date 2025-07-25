using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RemovedSharedOrderLines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Update OrderLine set SharedDateTime = null where SharedDateTime is not null");
            migrationBuilder.Sql("DELETE FROM SharedOrderLine");
            migrationBuilder.Sql("DELETE FROM AbpFeatures where Name = 'App.JobSharing'");
            migrationBuilder.Sql("DELETE FROM AbpPermissions where Name = 'Pages.Schedule.ShareJobs'");

            migrationBuilder.DropTable(
                name: "SharedOrderLine");

            migrationBuilder.DropColumn(
                name: "SharedDateTime",
                table: "OrderLine");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SharedDateTime",
                table: "OrderLine",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "SharedOrderLine",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OfficeId = table.Column<int>(type: "int", nullable: false),
                    OrderLineId = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SharedOrderLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SharedOrderLine_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SharedOrderLine_OrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "OrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SharedOrderLine_OfficeId",
                table: "SharedOrderLine",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_SharedOrderLine_OrderLineId",
                table: "SharedOrderLine",
                column: "OrderLineId");
        }
    }
}
