using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenamedServiceToItemStep1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Charge_Service_ItemId",
                table: "Charge");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingService_Service_ServiceId",
                table: "HaulingService");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingService_UnitOfMeasure_UnitOfMeasureId",
                table: "HaulingService");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingService_VehicleCategory_TruckCategoryId",
                table: "HaulingService");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingServicePrice_HaulingService_HaulingServiceId",
                table: "HaulingServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingServicePrice_PricingTier_PricingTierId",
                table: "HaulingServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Service_ItemId",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Service_MaterialItemId",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationService_Location_LocationId",
                table: "LocationService");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationService_Service_ServiceId",
                table: "LocationService");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationService_UnitOfMeasure_UnitOfMeasureId",
                table: "LocationService");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationServicePrice_LocationService_LocationServiceId",
                table: "LocationServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationServicePrice_PricingTier_PricingTierId",
                table: "LocationServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeServicePrice_Office_OfficeId",
                table: "OfficeServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeServicePrice_Service_ServiceId",
                table: "OfficeServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_FreightUomId",
                table: "OfficeServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_MaterialUomId",
                table: "OfficeServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Service_FreightItemId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Service_MaterialItemId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_Service_FreightItemId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_Service_MaterialItemId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Service_FreightItemId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Service_MaterialItemId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Service_HaulZone_HaulZoneId",
                table: "Service");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Service_ItemId",
                table: "Ticket");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.Service", //"PK_Service"
                table: "Service");

            migrationBuilder.DropPrimaryKey(
                name: "PK_dbo.OfficeServicePrice", //"PK_OfficeServicePrice",
                table: "OfficeServicePrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocationServicePrice",
                table: "LocationServicePrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_LocationService",
                table: "LocationService");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HaulingServicePrice",
                table: "HaulingServicePrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HaulingService",
                table: "HaulingService");

            migrationBuilder.RenameTable(
                name: "Service",
                newName: "Item");

            migrationBuilder.RenameTable(
                name: "OfficeServicePrice",
                newName: "OfficeItemPrice");

            migrationBuilder.RenameTable(
                name: "LocationServicePrice",
                newName: "ProductLocationPrice");

            migrationBuilder.RenameTable(
                name: "LocationService",
                newName: "ProductLocation");

            migrationBuilder.RenameTable(
                name: "HaulingServicePrice",
                newName: "HaulingCategoryPrice");

            migrationBuilder.RenameTable(
                name: "HaulingService",
                newName: "HaulingCategory");

            migrationBuilder.RenameIndex(
                name: "IX_Service_HaulZoneId",
                table: "Item",
                newName: "IX_Item_HaulZoneId");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "OfficeItemPrice",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeServicePrice_ServiceId",
                table: "OfficeItemPrice",
                newName: "IX_OfficeItemPrice_ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeServicePrice_OfficeId",
                table: "OfficeItemPrice",
                newName: "IX_OfficeItemPrice_OfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeServicePrice_MaterialUomId",
                table: "OfficeItemPrice",
                newName: "IX_OfficeItemPrice_MaterialUomId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeServicePrice_FreightUomId",
                table: "OfficeItemPrice",
                newName: "IX_OfficeItemPrice_FreightUomId");

            migrationBuilder.RenameColumn(
                name: "LocationServiceId",
                table: "ProductLocationPrice",
                newName: "ProductLocationId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationServicePrice_PricingTierId",
                table: "ProductLocationPrice",
                newName: "IX_ProductLocationPrice_PricingTierId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationServicePrice_LocationServiceId",
                table: "ProductLocationPrice",
                newName: "IX_ProductLocationPrice_ProductLocationId");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "ProductLocation",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationService_UnitOfMeasureId",
                table: "ProductLocation",
                newName: "IX_ProductLocation_UnitOfMeasureId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationService_ServiceId",
                table: "ProductLocation",
                newName: "IX_ProductLocation_ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_LocationService_LocationId",
                table: "ProductLocation",
                newName: "IX_ProductLocation_LocationId");

            migrationBuilder.RenameColumn(
                name: "HaulingServiceId",
                table: "HaulingCategoryPrice",
                newName: "HaulingCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingServicePrice_PricingTierId",
                table: "HaulingCategoryPrice",
                newName: "IX_HaulingCategoryPrice_PricingTierId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingServicePrice_HaulingServiceId",
                table: "HaulingCategoryPrice",
                newName: "IX_HaulingCategoryPrice_HaulingCategoryId");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "HaulingCategory",
                newName: "ItemId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingService_UnitOfMeasureId",
                table: "HaulingCategory",
                newName: "IX_HaulingCategory_UnitOfMeasureId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingService_TruckCategoryId",
                table: "HaulingCategory",
                newName: "IX_HaulingCategory_TruckCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingService_ServiceId",
                table: "HaulingCategory",
                newName: "IX_HaulingCategory_ItemId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Item",
                table: "Item",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfficeItemPrice",
                table: "OfficeItemPrice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductLocationPrice",
                table: "ProductLocationPrice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProductLocation",
                table: "ProductLocation",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HaulingCategoryPrice",
                table: "HaulingCategoryPrice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HaulingCategory",
                table: "HaulingCategory",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Charge_Item_ItemId",
                table: "Charge",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingCategory_Item_ItemId",
                table: "HaulingCategory",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingCategory_UnitOfMeasure_UnitOfMeasureId",
                table: "HaulingCategory",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingCategory_VehicleCategory_TruckCategoryId",
                table: "HaulingCategory",
                column: "TruckCategoryId",
                principalTable: "VehicleCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingCategoryPrice_HaulingCategory_HaulingCategoryId",
                table: "HaulingCategoryPrice",
                column: "HaulingCategoryId",
                principalTable: "HaulingCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingCategoryPrice_PricingTier_PricingTierId",
                table: "HaulingCategoryPrice",
                column: "PricingTierId",
                principalTable: "PricingTier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Item_ItemId",
                table: "InvoiceLine",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Item_MaterialItemId",
                table: "InvoiceLine",
                column: "MaterialItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Item_HaulZone_HaulZoneId",
                table: "Item",
                column: "HaulZoneId",
                principalTable: "HaulZone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeItemPrice_Item_ItemId",
                table: "OfficeItemPrice",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeItemPrice_Office_OfficeId",
                table: "OfficeItemPrice",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeItemPrice_UnitOfMeasure_FreightUomId",
                table: "OfficeItemPrice",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeItemPrice_UnitOfMeasure_MaterialUomId",
                table: "OfficeItemPrice",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Item_FreightItemId",
                table: "OrderLine",
                column: "FreightItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Item_MaterialItemId",
                table: "OrderLine",
                column: "MaterialItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductLocation_Item_ItemId",
                table: "ProductLocation",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductLocation_Location_LocationId",
                table: "ProductLocation",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductLocation_UnitOfMeasure_UnitOfMeasureId",
                table: "ProductLocation",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductLocationPrice_PricingTier_PricingTierId",
                table: "ProductLocationPrice",
                column: "PricingTierId",
                principalTable: "PricingTier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductLocationPrice_ProductLocation_ProductLocationId",
                table: "ProductLocationPrice",
                column: "ProductLocationId",
                principalTable: "ProductLocation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_Item_FreightItemId",
                table: "QuoteLine",
                column: "FreightItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteLine_Item_MaterialItemId",
                table: "QuoteLine",
                column: "MaterialItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Item_FreightItemId",
                table: "ReceiptLine",
                column: "FreightItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Item_MaterialItemId",
                table: "ReceiptLine",
                column: "MaterialItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Item_ItemId",
                table: "Ticket",
                column: "ItemId",
                principalTable: "Item",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Charge_Item_ItemId",
                table: "Charge");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingCategory_Item_ItemId",
                table: "HaulingCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingCategory_UnitOfMeasure_UnitOfMeasureId",
                table: "HaulingCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingCategory_VehicleCategory_TruckCategoryId",
                table: "HaulingCategory");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingCategoryPrice_HaulingCategory_HaulingCategoryId",
                table: "HaulingCategoryPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_HaulingCategoryPrice_PricingTier_PricingTierId",
                table: "HaulingCategoryPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Item_ItemId",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_InvoiceLine_Item_MaterialItemId",
                table: "InvoiceLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Item_HaulZone_HaulZoneId",
                table: "Item");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeItemPrice_Item_ItemId",
                table: "OfficeItemPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeItemPrice_Office_OfficeId",
                table: "OfficeItemPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeItemPrice_UnitOfMeasure_FreightUomId",
                table: "OfficeItemPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeItemPrice_UnitOfMeasure_MaterialUomId",
                table: "OfficeItemPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Item_FreightItemId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Item_MaterialItemId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductLocation_Item_ItemId",
                table: "ProductLocation");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductLocation_Location_LocationId",
                table: "ProductLocation");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductLocation_UnitOfMeasure_UnitOfMeasureId",
                table: "ProductLocation");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductLocationPrice_PricingTier_PricingTierId",
                table: "ProductLocationPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductLocationPrice_ProductLocation_ProductLocationId",
                table: "ProductLocationPrice");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_Item_FreightItemId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteLine_Item_MaterialItemId",
                table: "QuoteLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Item_FreightItemId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Item_MaterialItemId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Item_ItemId",
                table: "Ticket");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductLocationPrice",
                table: "ProductLocationPrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProductLocation",
                table: "ProductLocation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_OfficeItemPrice",
                table: "OfficeItemPrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Item",
                table: "Item");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HaulingCategoryPrice",
                table: "HaulingCategoryPrice");

            migrationBuilder.DropPrimaryKey(
                name: "PK_HaulingCategory",
                table: "HaulingCategory");

            migrationBuilder.RenameTable(
                name: "ProductLocationPrice",
                newName: "LocationServicePrice");

            migrationBuilder.RenameTable(
                name: "ProductLocation",
                newName: "LocationService");

            migrationBuilder.RenameTable(
                name: "OfficeItemPrice",
                newName: "OfficeServicePrice");

            migrationBuilder.RenameTable(
                name: "Item",
                newName: "Service");

            migrationBuilder.RenameTable(
                name: "HaulingCategoryPrice",
                newName: "HaulingServicePrice");

            migrationBuilder.RenameTable(
                name: "HaulingCategory",
                newName: "HaulingService");

            migrationBuilder.RenameColumn(
                name: "ProductLocationId",
                table: "LocationServicePrice",
                newName: "LocationServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductLocationPrice_ProductLocationId",
                table: "LocationServicePrice",
                newName: "IX_LocationServicePrice_LocationServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductLocationPrice_PricingTierId",
                table: "LocationServicePrice",
                newName: "IX_LocationServicePrice_PricingTierId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "LocationService",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductLocation_UnitOfMeasureId",
                table: "LocationService",
                newName: "IX_LocationService_UnitOfMeasureId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductLocation_LocationId",
                table: "LocationService",
                newName: "IX_LocationService_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductLocation_ItemId",
                table: "LocationService",
                newName: "IX_LocationService_ServiceId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "OfficeServicePrice",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeItemPrice_OfficeId",
                table: "OfficeServicePrice",
                newName: "IX_OfficeServicePrice_OfficeId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeItemPrice_MaterialUomId",
                table: "OfficeServicePrice",
                newName: "IX_OfficeServicePrice_MaterialUomId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeItemPrice_ItemId",
                table: "OfficeServicePrice",
                newName: "IX_OfficeServicePrice_ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_OfficeItemPrice_FreightUomId",
                table: "OfficeServicePrice",
                newName: "IX_OfficeServicePrice_FreightUomId");

            migrationBuilder.RenameIndex(
                name: "IX_Item_HaulZoneId",
                table: "Service",
                newName: "IX_Service_HaulZoneId");

            migrationBuilder.RenameColumn(
                name: "HaulingCategoryId",
                table: "HaulingServicePrice",
                newName: "HaulingServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingCategoryPrice_PricingTierId",
                table: "HaulingServicePrice",
                newName: "IX_HaulingServicePrice_PricingTierId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingCategoryPrice_HaulingCategoryId",
                table: "HaulingServicePrice",
                newName: "IX_HaulingServicePrice_HaulingServiceId");

            migrationBuilder.RenameColumn(
                name: "ItemId",
                table: "HaulingService",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingCategory_UnitOfMeasureId",
                table: "HaulingService",
                newName: "IX_HaulingService_UnitOfMeasureId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingCategory_TruckCategoryId",
                table: "HaulingService",
                newName: "IX_HaulingService_TruckCategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_HaulingCategory_ItemId",
                table: "HaulingService",
                newName: "IX_HaulingService_ServiceId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocationServicePrice",
                table: "LocationServicePrice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_LocationService",
                table: "LocationService",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_OfficeServicePrice",
                table: "OfficeServicePrice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Service",
                table: "Service",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HaulingServicePrice",
                table: "HaulingServicePrice",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_HaulingService",
                table: "HaulingService",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Charge_Service_ItemId",
                table: "Charge",
                column: "ItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingService_Service_ServiceId",
                table: "HaulingService",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingService_UnitOfMeasure_UnitOfMeasureId",
                table: "HaulingService",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingService_VehicleCategory_TruckCategoryId",
                table: "HaulingService",
                column: "TruckCategoryId",
                principalTable: "VehicleCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingServicePrice_HaulingService_HaulingServiceId",
                table: "HaulingServicePrice",
                column: "HaulingServiceId",
                principalTable: "HaulingService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_HaulingServicePrice_PricingTier_PricingTierId",
                table: "HaulingServicePrice",
                column: "PricingTierId",
                principalTable: "PricingTier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Service_ItemId",
                table: "InvoiceLine",
                column: "ItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InvoiceLine_Service_MaterialItemId",
                table: "InvoiceLine",
                column: "MaterialItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationService_Location_LocationId",
                table: "LocationService",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationService_Service_ServiceId",
                table: "LocationService",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationService_UnitOfMeasure_UnitOfMeasureId",
                table: "LocationService",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LocationServicePrice_LocationService_LocationServiceId",
                table: "LocationServicePrice",
                column: "LocationServiceId",
                principalTable: "LocationService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationServicePrice_PricingTier_PricingTierId",
                table: "LocationServicePrice",
                column: "PricingTierId",
                principalTable: "PricingTier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeServicePrice_Office_OfficeId",
                table: "OfficeServicePrice",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeServicePrice_Service_ServiceId",
                table: "OfficeServicePrice",
                column: "ServiceId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_FreightUomId",
                table: "OfficeServicePrice",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_MaterialUomId",
                table: "OfficeServicePrice",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Service_FreightItemId",
                table: "OrderLine",
                column: "FreightItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Service_MaterialItemId",
                table: "OrderLine",
                column: "MaterialItemId",
                principalTable: "Service",
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
                name: "FK_ReceiptLine_Service_FreightItemId",
                table: "ReceiptLine",
                column: "FreightItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Service_MaterialItemId",
                table: "ReceiptLine",
                column: "MaterialItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Service_HaulZone_HaulZoneId",
                table: "Service",
                column: "HaulZoneId",
                principalTable: "HaulZone",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Service_ItemId",
                table: "Ticket",
                column: "ItemId",
                principalTable: "Service",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
