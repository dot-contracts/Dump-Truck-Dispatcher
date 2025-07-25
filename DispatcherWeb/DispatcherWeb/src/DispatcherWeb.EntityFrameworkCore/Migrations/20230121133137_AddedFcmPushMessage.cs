using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedFcmPushMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FcmPushMessage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    FcmRegistrationTokenId = table.Column<int>(type: "int", nullable: true),
                    JsonPayload = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    SentAtDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReceivedAtDateTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Error = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FcmPushMessage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FcmPushMessage_FcmRegistrationToken_FcmRegistrationTokenId",
                        column: x => x.FcmRegistrationTokenId,
                        principalTable: "FcmRegistrationToken",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FcmPushMessage_FcmRegistrationTokenId",
                table: "FcmPushMessage",
                column: "FcmRegistrationTokenId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FcmPushMessage");
        }
    }
}
