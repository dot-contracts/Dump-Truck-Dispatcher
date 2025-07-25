using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedCustomerNotificationTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CustomerNotification",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Type = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_CustomerNotification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerNotification_AbpUsers_CreatorUserId",
                        column: x => x.CreatorUserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerNotificationEdition",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerNotificationId = table.Column<int>(type: "int", nullable: false),
                    EditionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotificationEdition", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerNotificationEdition_AbpEditions_EditionId",
                        column: x => x.EditionId,
                        principalTable: "AbpEditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerNotificationEdition_CustomerNotification_CustomerNotificationId",
                        column: x => x.CustomerNotificationId,
                        principalTable: "CustomerNotification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerNotificationRole",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerNotificationId = table.Column<int>(type: "int", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotificationRole", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerNotificationRole_CustomerNotification_CustomerNotificationId",
                        column: x => x.CustomerNotificationId,
                        principalTable: "CustomerNotification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CustomerNotificationTenant",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CustomerNotificationId = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerNotificationTenant", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CustomerNotificationTenant_AbpTenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "AbpTenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CustomerNotificationTenant_CustomerNotification_CustomerNotificationId",
                        column: x => x.CustomerNotificationId,
                        principalTable: "CustomerNotification",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotification_CreatorUserId",
                table: "CustomerNotification",
                column: "CreatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotificationEdition_CustomerNotificationId",
                table: "CustomerNotificationEdition",
                column: "CustomerNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotificationEdition_EditionId",
                table: "CustomerNotificationEdition",
                column: "EditionId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotificationRole_CustomerNotificationId",
                table: "CustomerNotificationRole",
                column: "CustomerNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotificationTenant_CustomerNotificationId",
                table: "CustomerNotificationTenant",
                column: "CustomerNotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerNotificationTenant_TenantId",
                table: "CustomerNotificationTenant",
                column: "TenantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerNotificationEdition");

            migrationBuilder.DropTable(
                name: "CustomerNotificationRole");

            migrationBuilder.DropTable(
                name: "CustomerNotificationTenant");

            migrationBuilder.DropTable(
                name: "CustomerNotification");
        }
    }
}
