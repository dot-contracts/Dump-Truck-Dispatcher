using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedQuoteServicesToQuoteLinesStep2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_QuoteService_QuoteLineId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Location_DeliverToId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Location_LoadAtId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Quote_QuoteId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Service_FreightItemId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Service_MaterialItemId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_FreightUomId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_MaterialUomId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteServiceVehicleCategory_QuoteService_QuoteLineId",
                table: "QuoteServiceVehicleCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteServiceVehicleCategory_VehicleCategory_VehicleCategoryId",
                table: "QuoteServiceVehicleCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuoteServiceVehicleCategory",
                table: "QuoteServiceVehicleCategory");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_QuoteService",
            //    table: "QuoteService");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.QuoteService",
                table: "QuoteService");

            migrationBuilder.RenameTable(
                name: "QuoteServiceVehicleCategory",
                newName: "QuoteLineVehicleCategory");

            migrationBuilder.RenameTable(
                name: "QuoteService",
                newName: "QuoteLine");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteServiceVehicleCategory_VehicleCategoryId",
                table: "QuoteLineVehicleCategory",
                newName: "IX_QuoteLineVehicleCategory_VehicleCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteServiceVehicleCategory_QuoteLineId",
                table: "QuoteLineVehicleCategory",
                newName: "IX_QuoteLineVehicleCategory_QuoteLineId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_QuoteId",
                table: "QuoteLine",
                newName: "IX_QuoteLine_QuoteId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_MaterialUomId",
                table: "QuoteLine",
                newName: "IX_QuoteLine_MaterialUomId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_MaterialItemId",
                table: "QuoteLine",
                newName: "IX_QuoteLine_MaterialItemId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_LoadAtId",
                table: "QuoteLine",
                newName: "IX_QuoteLine_LoadAtId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_FreightUomId",
                table: "QuoteLine",
                newName: "IX_QuoteLine_FreightUomId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_FreightItemId",
                table: "QuoteLine",
                newName: "IX_QuoteLine_FreightItemId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_DeliverToId",
                table: "QuoteLine",
                newName: "IX_QuoteLine_DeliverToId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuoteLineVehicleCategory",
                table: "QuoteLineVehicleCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuoteLine",
                table: "QuoteLine",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_QuoteLine_QuoteLineId",
                table: "OrderLine",
                column: "QuoteLineId",
                principalTable: "QuoteLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_Location_DeliverToId",
                table: "QuoteLine",
                column: "DeliverToId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_Location_LoadAtId",
                table: "QuoteLine",
                column: "LoadAtId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_Quote_QuoteId",
                table: "QuoteLine",
                column: "QuoteId",
                principalTable: "Quote",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_Service_FreightItemId",
                table: "QuoteLine",
                column: "FreightItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_Service_MaterialItemId",
                table: "QuoteLine",
                column: "MaterialItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_UnitOfMeasure_FreightUomId",
                table: "QuoteLine",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_UnitOfMeasure_MaterialUomId",
                table: "QuoteLine",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLineVehicleCategory_QuoteLine_QuoteLineId",
                table: "QuoteLineVehicleCategory",
                column: "QuoteLineId",
                principalTable: "QuoteLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLineVehicleCategory_VehicleCategory_VehicleCategoryId",
                table: "QuoteLineVehicleCategory",
                column: "VehicleCategoryId",
                principalTable: "VehicleCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_QuoteLine_QuoteLineId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_Location_DeliverToId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_Location_LoadAtId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_Quote_QuoteId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_Service_FreightItemId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_Service_MaterialItemId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_UnitOfMeasure_FreightUomId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_UnitOfMeasure_MaterialUomId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLineVehicleCategory_QuoteLine_QuoteLineId",
                table: "QuoteLineVehicleCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLineVehicleCategory_VehicleCategory_VehicleCategoryId",
                table: "QuoteLineVehicleCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuoteLineVehicleCategory",
                table: "QuoteLineVehicleCategory");

            migrationBuilder.DropPrimaryKey(
                name: "PK_QuoteLine",
                table: "QuoteLine");

            migrationBuilder.RenameTable(
                name: "QuoteLineVehicleCategory",
                newName: "QuoteServiceVehicleCategory");

            migrationBuilder.RenameTable(
                name: "QuoteLine",
                newName: "QuoteService");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteLineVehicleCategory_VehicleCategoryId",
                table: "QuoteServiceVehicleCategory",
                newName: "IX_QuoteServiceVehicleCategory_VehicleCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteLineVehicleCategory_QuoteLineId",
                table: "QuoteServiceVehicleCategory",
                newName: "IX_QuoteServiceVehicleCategory_QuoteLineId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteLine_QuoteId",
                table: "QuoteService",
                newName: "IX_QuoteService_QuoteId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteLine_MaterialUomId",
                table: "QuoteService",
                newName: "IX_QuoteService_MaterialUomId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteLine_MaterialItemId",
                table: "QuoteService",
                newName: "IX_QuoteService_MaterialItemId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteLine_LoadAtId",
                table: "QuoteService",
                newName: "IX_QuoteService_LoadAtId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteLine_FreightUomId",
                table: "QuoteService",
                newName: "IX_QuoteService_FreightUomId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteLine_FreightItemId",
                table: "QuoteService",
                newName: "IX_QuoteService_FreightItemId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteLine_DeliverToId",
                table: "QuoteService",
                newName: "IX_QuoteService_DeliverToId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuoteServiceVehicleCategory",
                table: "QuoteServiceVehicleCategory",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_QuoteService",
                table: "QuoteService",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_QuoteService_QuoteLineId",
                table: "OrderLine",
                column: "QuoteLineId",
                principalTable: "QuoteService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Location_DeliverToId",
                table: "QuoteService",
                column: "DeliverToId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Location_LoadAtId",
                table: "QuoteService",
                column: "LoadAtId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Quote_QuoteId",
                table: "QuoteService",
                column: "QuoteId",
                principalTable: "Quote",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Service_FreightItemId",
                table: "QuoteService",
                column: "FreightItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Service_MaterialItemId",
                table: "QuoteService",
                column: "MaterialItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_FreightUomId",
                table: "QuoteService",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_MaterialUomId",
                table: "QuoteService",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteServiceVehicleCategory_QuoteService_QuoteLineId",
                table: "QuoteServiceVehicleCategory",
                column: "QuoteLineId",
                principalTable: "QuoteService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteServiceVehicleCategory_VehicleCategory_VehicleCategoryId",
                table: "QuoteServiceVehicleCategory",
                column: "VehicleCategoryId",
                principalTable: "VehicleCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
