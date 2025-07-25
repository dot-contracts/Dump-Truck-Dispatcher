using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedTruckAndDriverFKtoChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TargetDriverId",
                table: "AppChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetTrailerId",
                table: "AppChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TargetTruckId",
                table: "AppChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppChatMessages_TargetDriverId",
                table: "AppChatMessages",
                column: "TargetDriverId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatMessages_TargetTrailerId",
                table: "AppChatMessages",
                column: "TargetTrailerId");

            migrationBuilder.CreateIndex(
                name: "IX_AppChatMessages_TargetTruckId",
                table: "AppChatMessages",
                column: "TargetTruckId");

            migrationBuilder.AddForeignKey(
                name: "FK_AppChatMessages_Driver_TargetDriverId",
                table: "AppChatMessages",
                column: "TargetDriverId",
                principalTable: "Driver",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppChatMessages_Truck_TargetTrailerId",
                table: "AppChatMessages",
                column: "TargetTrailerId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AppChatMessages_Truck_TargetTruckId",
                table: "AppChatMessages",
                column: "TargetTruckId",
                principalTable: "Truck",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AppChatMessages_Driver_TargetDriverId",
                table: "AppChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_AppChatMessages_Truck_TargetTrailerId",
                table: "AppChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_AppChatMessages_Truck_TargetTruckId",
                table: "AppChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_AppChatMessages_TargetDriverId",
                table: "AppChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_AppChatMessages_TargetTrailerId",
                table: "AppChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_AppChatMessages_TargetTruckId",
                table: "AppChatMessages");

            migrationBuilder.DropColumn(
                name: "TargetDriverId",
                table: "AppChatMessages");

            migrationBuilder.DropColumn(
                name: "TargetTrailerId",
                table: "AppChatMessages");

            migrationBuilder.DropColumn(
                name: "TargetTruckId",
                table: "AppChatMessages");
        }
    }
}
