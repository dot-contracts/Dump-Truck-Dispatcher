using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedOrderLineStaggeredTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("Delete from OrderLineStaggeredTime");

            migrationBuilder.DropTable(
                name: "OrderLineStaggeredTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrderLineStaggeredTime",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LineNumber = table.Column<int>(nullable: false),
                    OrderLineId = table.Column<int>(nullable: false),
                    TenantId = table.Column<int>(nullable: false),
                    TimeOnJob = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrderLineStaggeredTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrderLineStaggeredTime_OrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "OrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrderLineStaggeredTime_OrderLineId",
                table: "OrderLineStaggeredTime",
                column: "OrderLineId");
        }
    }
}
