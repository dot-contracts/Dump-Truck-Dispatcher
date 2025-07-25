using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedSeparateTicketQuantityFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Item_ItemId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_UnitOfMeasure_UnitOfMeasureId",
                table: "Ticket");

            migrationBuilder.RenameColumn(
                name: "UnitOfMeasureId",
                table: "Ticket",
                newName: "FreightUomId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "Ticket",
                newName: "FreightItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_UnitOfMeasureId",
                table: "Ticket",
                newName: "IX_Ticket_FreightUomId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_ItemId",
                table: "Ticket",
                newName: "IX_Ticket_FreightItemId");

            migrationBuilder.AddColumn<int>(
                name: "MaterialItemId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FreightQuantity",
                table: "Ticket",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialUomId",
                table: "Ticket",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialQuantity",
                table: "Ticket",
                type: "decimal(18,4)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "NonbillableFreight",
                table: "Ticket",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "NonbillableMaterial",
                table: "Ticket",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_MaterialItemId",
                table: "Ticket",
                column: "MaterialItemId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_MaterialUomId",
                table: "Ticket",
                column: "MaterialUomId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Item_MaterialItemId",
                table: "Ticket",
                column: "MaterialItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Item_FreightItemId",
                table: "Ticket",
                column: "FreightItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_UnitOfMeasure_MaterialUomId",
                table: "Ticket",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_UnitOfMeasure_FreightUomId",
                table: "Ticket",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Item_MaterialItemId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Item_FreightItemId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_UnitOfMeasure_MaterialUomId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_UnitOfMeasure_FreightUomId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_MaterialItemId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_MaterialUomId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MaterialItemId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "FreightQuantity",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MaterialUomId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MaterialQuantity",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "NonbillableFreight",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "NonbillableMaterial",
                table: "Ticket");

            migrationBuilder.RenameColumn(
                name: "FreightUomId",
                table: "Ticket",
                newName: "UnitOfMeasureId");

            migrationBuilder.RenameColumn(
                name: "FreightItemId",
                table: "Ticket",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_FreightUomId",
                table: "Ticket",
                newName: "IX_Ticket_UnitOfMeasureId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_FreightItemId",
                table: "Ticket",
                newName: "IX_Ticket_ItemId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Item_ItemId",
                table: "Ticket",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_UnitOfMeasure_UnitOfMeasureId",
                table: "Ticket",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id");
        }
    }
}
