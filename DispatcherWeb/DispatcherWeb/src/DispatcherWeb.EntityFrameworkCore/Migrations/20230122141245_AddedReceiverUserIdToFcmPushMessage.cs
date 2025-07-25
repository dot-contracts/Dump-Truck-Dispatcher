using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedReceiverUserIdToFcmPushMessage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from FcmPushMessage");

            migrationBuilder.AddColumn<int>(
                name: "ReceiverDriverId",
                table: "FcmPushMessage",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ReceiverUserId",
                table: "FcmPushMessage",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_FcmPushMessage_ReceiverDriverId",
                table: "FcmPushMessage",
                column: "ReceiverDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_FcmPushMessage_ReceiverUserId",
                table: "FcmPushMessage",
                column: "ReceiverUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_FcmPushMessage_AbpUsers_ReceiverUserId",
                table: "FcmPushMessage",
                column: "ReceiverUserId",
                principalTable: "AbpUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FcmPushMessage_Driver_ReceiverDriverId",
                table: "FcmPushMessage",
                column: "ReceiverDriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FcmPushMessage_AbpUsers_ReceiverUserId",
                table: "FcmPushMessage");

            migrationBuilder.DropForeignKey(
                name: "FK_FcmPushMessage_Driver_ReceiverDriverId",
                table: "FcmPushMessage");

            migrationBuilder.DropIndex(
                name: "IX_FcmPushMessage_ReceiverDriverId",
                table: "FcmPushMessage");

            migrationBuilder.DropIndex(
                name: "IX_FcmPushMessage_ReceiverUserId",
                table: "FcmPushMessage");

            migrationBuilder.DropColumn(
                name: "ReceiverDriverId",
                table: "FcmPushMessage");

            migrationBuilder.DropColumn(
                name: "ReceiverUserId",
                table: "FcmPushMessage");
        }
    }
}
