using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddLeaseHaulerRequestTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeaseHaulerRequest",
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
                    Date = table.Column<DateTime>(nullable: false),
                    Shift = table.Column<byte>(nullable: true),
                    OfficeId = table.Column<int>(nullable: false),
                    LeaseHaulerId = table.Column<int>(nullable: false),
                    Sent = table.Column<DateTime>(nullable: true),
                    Available = table.Column<int>(nullable: false),
                    Approved = table.Column<int>(nullable: false),
                    Scheduled = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerRequest", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerRequest_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerRequest_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerRequest_LeaseHaulerId",
                table: "LeaseHaulerRequest",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerRequest_OfficeId",
                table: "LeaseHaulerRequest",
                column: "OfficeId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeaseHaulerRequest");
        }
    }
}
