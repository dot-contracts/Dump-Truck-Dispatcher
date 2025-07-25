using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class DeletedHaulZoneIdFromItemAndUpdatedHaulZoneFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Item_HaulZone_HaulZoneId",
                table: "Item");

            migrationBuilder.DropIndex(
                name: "IX_Item_HaulZoneId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "HaulZoneId",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "Maximum",
                table: "HaulZone");

            migrationBuilder.RenameColumn(
                name: "Minimum",
                table: "HaulZone",
                newName: "Quantity");

            migrationBuilder.AddColumn<bool>(
                name: "UseZoneBasedRates",
                table: "Item",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "BillRatePerTon",
                table: "HaulZone",
                type: "decimal(19,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MinPerLoad",
                table: "HaulZone",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PayRatePerTon",
                table: "HaulZone",
                type: "decimal(19,4)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UseZoneBasedRates",
                table: "Item");

            migrationBuilder.DropColumn(
                name: "BillRatePerTon",
                table: "HaulZone");

            migrationBuilder.DropColumn(
                name: "MinPerLoad",
                table: "HaulZone");

            migrationBuilder.DropColumn(
                name: "PayRatePerTon",
                table: "HaulZone");

            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "HaulZone",
                newName: "Minimum");

            migrationBuilder.AddColumn<int>(
                name: "HaulZoneId",
                table: "Item",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Maximum",
                table: "HaulZone",
                type: "real",
                nullable: false,
                defaultValue: 0f);

            migrationBuilder.CreateIndex(
                name: "IX_Item_HaulZoneId",
                table: "Item",
                column: "HaulZoneId");

            migrationBuilder.AddForeignKey(
                name: "FK_Item_HaulZone_HaulZoneId",
                table: "Item",
                column: "HaulZoneId",
                principalTable: "HaulZone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
