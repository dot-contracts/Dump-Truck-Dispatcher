using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class ChangedUOMToNotNullableAndRenamedHaulZonIdToHaulZoneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Service_HaulZone_HaulZonId",
                table: "Service");

            migrationBuilder.RenameColumn(
                name: "HaulZonId",
                table: "Service",
                newName: "HaulZoneId");

            migrationBuilder.RenameIndex(
                name: "IX_Service_HaulZonId",
                table: "Service",
                newName: "IX_Service_HaulZoneId");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerUnit",
                table: "HaulingServicePrice",
                type: "decimal(19,4)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)");

            migrationBuilder.AddForeignKey(
                name: "FK_Service_HaulZone_HaulZoneId",
                table: "Service",
                column: "HaulZoneId",
                principalTable: "HaulZone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Service_HaulZone_HaulZoneId",
                table: "Service");

            migrationBuilder.RenameColumn(
                name: "HaulZoneId",
                table: "Service",
                newName: "HaulZonId");

            migrationBuilder.RenameIndex(
                name: "IX_Service_HaulZoneId",
                table: "Service",
                newName: "IX_Service_HaulZonId");

            migrationBuilder.AlterColumn<decimal>(
                name: "PricePerUnit",
                table: "HaulingServicePrice",
                type: "decimal(19,4)",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,4)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Service_HaulZone_HaulZonId",
                table: "Service",
                column: "HaulZonId",
                principalTable: "HaulZone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
