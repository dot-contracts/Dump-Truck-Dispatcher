using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddReceipts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FreightQuantity",
                table: "Ticket",
                type: "decimal(18, 4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FreightUomId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialQuantity",
                table: "Ticket",
                type: "decimal(18, 4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MaterialUomId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptLineId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OfficeId",
                table: "OrderPayment",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReceiptId",
                table: "OrderPayment",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "OrderLine",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddColumn<decimal>(
                name: "FreightQuantity",
                table: "OrderLine",
                type: "decimal(18, 4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FreightUomId",
                table: "OrderLine",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MaterialQuantity",
                table: "OrderLine",
                type: "decimal(18, 4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaterialUomId",
                table: "OrderLine",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Receipt",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    OrderId = table.Column<int>(nullable: false),
                    IsFreightTotalOverridden = table.Column<bool>(nullable: false),
                    IsMaterialTotalOverridden = table.Column<bool>(nullable: false),
                    DeliveryDate = table.Column<DateTime>(nullable: false),
                    ReceiptDate = table.Column<DateTime>(nullable: false),
                    Shift = table.Column<byte>(nullable: true),
                    OfficeId = table.Column<int>(nullable: false),
                    CustomerId = table.Column<int>(nullable: false),
                    QuoteId = table.Column<int>(nullable: true),
                    JobNumber = table.Column<string>(nullable: true),
                    PoNumber = table.Column<string>(nullable: true),
                    DeliverTo = table.Column<string>(nullable: true),
                    SalesTaxRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    FreightTotal = table.Column<decimal>(nullable: false),
                    MaterialTotal = table.Column<decimal>(nullable: false),
                    FuelSurcharge = table.Column<decimal>(nullable: false),
                    FuelSurchargeRate = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    SalesTax = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(19, 4)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Receipt_Customer_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customer",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipt_Office_OfficeId",
                        column: x => x.OfficeId,
                        principalTable: "Office",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipt_Order_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Order",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipt_Quote_QuoteId",
                        column: x => x.QuoteId,
                        principalTable: "Quote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptLine",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    ReceiptId = table.Column<int>(nullable: false),
                    OrderLineId = table.Column<int>(nullable: true),
                    LineNumber = table.Column<int>(nullable: false),
                    SupplierId = table.Column<int>(nullable: true),
                    ServiceId = table.Column<int>(nullable: false),
                    Designation = table.Column<int>(nullable: false),
                    MaterialUomId = table.Column<int>(nullable: true),
                    MaterialRate = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    MaterialQuantity = table.Column<decimal>(type: "decimal(18, 4)", nullable: true),
                    MaterialAmount = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    FreightUomId = table.Column<int>(nullable: true),
                    FreightRate = table.Column<decimal>(type: "decimal(18, 4)", nullable: false),
                    FreightQuantity = table.Column<decimal>(type: "decimal(18, 4)", nullable: true),
                    FreightAmount = table.Column<decimal>(type: "decimal(19, 4)", nullable: false),
                    IsMaterialAmountOverridden = table.Column<bool>(nullable: false),
                    IsFreightAmountOverridden = table.Column<bool>(nullable: false),
                    IsMaterialRateOverridden = table.Column<bool>(nullable: false),
                    IsFreightRateOverridden = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptLine_UnitOfMeasure_FreightUomId",
                        column: x => x.FreightUomId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptLine_UnitOfMeasure_MaterialUomId",
                        column: x => x.MaterialUomId,
                        principalTable: "UnitOfMeasure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptLine_OrderLine_OrderLineId",
                        column: x => x.OrderLineId,
                        principalTable: "OrderLine",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptLine_Receipt_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptLine_Service_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Service",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ReceiptLine_Supplier_SupplierId",
                        column: x => x.SupplierId,
                        principalTable: "Supplier",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.Sql(@"update Ticket set FreightQuantity = Quantity, FreightUomId = UnitOfMeasureId, MaterialQuantity = Quantity, MaterialUomId = UnitOfMeasureId");
            migrationBuilder.Sql(@"update OrderLine set FreightQuantity = Quantity, FreightUomId = UnitOfMeasureId, MaterialQuantity = Quantity, MaterialUomId = UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_FreightUomId",
                table: "Ticket",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_MaterialUomId",
                table: "Ticket",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_ReceiptLineId",
                table: "Ticket",
                column: "ReceiptLineId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayment_OfficeId",
                table: "OrderPayment",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderPayment_ReceiptId",
                table: "OrderPayment",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_FreightUomId",
                table: "OrderLine",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_MaterialUomId",
                table: "OrderLine",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_CustomerId",
                table: "Receipt",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_OfficeId",
                table: "Receipt",
                column: "OfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_OrderId",
                table: "Receipt",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipt_QuoteId",
                table: "Receipt",
                column: "QuoteId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptLine_FreightUomId",
                table: "ReceiptLine",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptLine_MaterialUomId",
                table: "ReceiptLine",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptLine_OrderLineId",
                table: "ReceiptLine",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptLine_ReceiptId",
                table: "ReceiptLine",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptLine_ServiceId",
                table: "ReceiptLine",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptLine_SupplierId",
                table: "ReceiptLine",
                column: "SupplierId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_UnitOfMeasure_FreightUomId",
                table: "OrderLine",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_UnitOfMeasure_MaterialUomId",
                table: "OrderLine",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPayment_Office_OfficeId",
                table: "OrderPayment",
                column: "OfficeId",
                principalTable: "Office",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderPayment_Receipt_ReceiptId",
                table: "OrderPayment",
                column: "ReceiptId",
                principalTable: "Receipt",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_UnitOfMeasure_FreightUomId",
                table: "Ticket",
                column: "FreightUomId",
                principalTable: "UnitOfMeasure",
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
                name: "FK_Ticket_ReceiptLine_ReceiptLineId",
                table: "Ticket",
                column: "ReceiptLineId",
                principalTable: "ReceiptLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_UnitOfMeasure_FreightUomId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_UnitOfMeasure_MaterialUomId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderPayment_Office_OfficeId",
                table: "OrderPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderPayment_Receipt_ReceiptId",
                table: "OrderPayment");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_UnitOfMeasure_FreightUomId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_UnitOfMeasure_MaterialUomId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_ReceiptLine_ReceiptLineId",
                table: "Ticket");

            migrationBuilder.DropTable(
                name: "ReceiptLine");

            migrationBuilder.DropTable(
                name: "Receipt");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_FreightUomId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_MaterialUomId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_ReceiptLineId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_OrderPayment_OfficeId",
                table: "OrderPayment");

            migrationBuilder.DropIndex(
                name: "IX_OrderPayment_ReceiptId",
                table: "OrderPayment");

            migrationBuilder.DropIndex(
                name: "IX_OrderLine_FreightUomId",
                table: "OrderLine");

            migrationBuilder.DropIndex(
                name: "IX_OrderLine_MaterialUomId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "FreightQuantity",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "FreightUomId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MaterialQuantity",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "MaterialUomId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "ReceiptLineId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "OfficeId",
                table: "OrderPayment");

            migrationBuilder.DropColumn(
                name: "ReceiptId",
                table: "OrderPayment");

            migrationBuilder.DropColumn(
                name: "FreightQuantity",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "FreightUomId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "MaterialQuantity",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "MaterialUomId",
                table: "OrderLine");

            migrationBuilder.AlterColumn<int>(
                name: "UnitOfMeasureId",
                table: "OrderLine",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);
        }
    }
}
