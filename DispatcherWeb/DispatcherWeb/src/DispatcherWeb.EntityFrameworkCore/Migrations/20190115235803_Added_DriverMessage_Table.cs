using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class Added_DriverMessage_Table : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TrackableSms",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    Sid = table.Column<string>(maxLength: 100, nullable: true),
                    Status = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackableSms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DriverMessage",
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
                    DriverId = table.Column<int>(nullable: false),
                    TimeSent = table.Column<DateTime>(nullable: false),
                    MessageType = table.Column<int>(nullable: false),
                    Subject = table.Column<string>(maxLength: 100, nullable: true),
                    Body = table.Column<string>(maxLength: 200, nullable: true),
                    TrackableEmailId = table.Column<Guid>(nullable: true),
                    TrackableSmsId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DriverMessage_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverMessage_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverMessage_TrackableEmails_TrackableEmailId",
                        column: x => x.TrackableEmailId,
                        principalTable: "TrackableEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DriverMessage_TrackableSms_TrackableSmsId",
                        column: x => x.TrackableSmsId,
                        principalTable: "TrackableSms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DriverMessage_CreatorUserId",
                table: "DriverMessage",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverMessage_DriverId",
                table: "DriverMessage",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverMessage_TrackableEmailId",
                table: "DriverMessage",
                column: "TrackableEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverMessage_TrackableSmsId",
                table: "DriverMessage",
                column: "TrackableSmsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverMessage");

            migrationBuilder.DropTable(
                name: "TrackableSms");
        }
    }
}
