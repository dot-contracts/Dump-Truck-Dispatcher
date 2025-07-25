using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedHostEmailTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HostEmails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ActiveFilter = table.Column<bool>(type: "bit", nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ProcessedAtDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostEmails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostEmails_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HostEmailEditions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostEmailId = table.Column<int>(type: "int", nullable: false),
                    EditionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostEmailEditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostEmailEditions_AbpEditions_EditionId",
                        column: x => x.EditionId,
                        principalTable: "AbpEditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HostEmailEditions_HostEmails_HostEmailId",
                        column: x => x.HostEmailId,
                        principalTable: "HostEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HostEmailReceivers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostEmailId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    TrackableEmailId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostEmailReceivers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostEmailReceivers_AbpTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "AbpTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HostEmailReceivers_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HostEmailReceivers_HostEmails_HostEmailId",
                        column: x => x.HostEmailId,
                        principalTable: "HostEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HostEmailReceivers_TrackableEmails_TrackableEmailId",
                        column: x => x.TrackableEmailId,
                        principalTable: "TrackableEmails",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "HostEmailRoles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostEmailId = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostEmailRoles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostEmailRoles_HostEmails_HostEmailId",
                        column: x => x.HostEmailId,
                        principalTable: "HostEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HostEmailTenants",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostEmailId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostEmailTenants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostEmailTenants_AbpTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "AbpTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HostEmailTenants_HostEmails_HostEmailId",
                        column: x => x.HostEmailId,
                        principalTable: "HostEmails",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HostEmailEditions_EditionId",
                table: "HostEmailEditions",
                column: "EditionId");

            migrationBuilder.CreateIndex(
                name: "IX_HostEmailEditions_HostEmailId",
                table: "HostEmailEditions",
                column: "HostEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_HostEmailReceivers_HostEmailId",
                table: "HostEmailReceivers",
                column: "HostEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_HostEmailReceivers_TenantId",
                table: "HostEmailReceivers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_HostEmailReceivers_TrackableEmailId",
                table: "HostEmailReceivers",
                column: "TrackableEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_HostEmailReceivers_UserId",
                table: "HostEmailReceivers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HostEmailRoles_HostEmailId",
                table: "HostEmailRoles",
                column: "HostEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_HostEmails_CreatorUserId",
                table: "HostEmails",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_HostEmailTenants_HostEmailId",
                table: "HostEmailTenants",
                column: "HostEmailId");

            migrationBuilder.CreateIndex(
                name: "IX_HostEmailTenants_TenantId",
                table: "HostEmailTenants",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HostEmailEditions");

            migrationBuilder.DropTable(
                name: "HostEmailReceivers");

            migrationBuilder.DropTable(
                name: "HostEmailRoles");

            migrationBuilder.DropTable(
                name: "HostEmailTenants");

            migrationBuilder.DropTable(
                name: "HostEmails");
        }
    }
}
