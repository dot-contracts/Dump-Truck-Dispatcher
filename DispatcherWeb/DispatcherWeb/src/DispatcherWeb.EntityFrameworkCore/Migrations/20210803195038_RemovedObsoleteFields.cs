using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedObsoleteFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"Update Customer set CreditCardFirstName = null, CreditCardLastName = null;
Update Driver set PayMethod = null, PayRate = null;
Update EmployeeTime set PayRate = null;
Update LeaseHaulerAgreementService set UnitOfMeasureId = null;
Update [Order] set StartTime = null, SupplierId = null, LoadAt = null, FuelSurchargeRate = 0, FuelSurcharge = 0, CreditCardToken = null, CreditCardFirstName = null, CreditCardLastName = null, CreditCardStreetAddress = null, CreditCardZipCode = null, AuthorizationDateTime = null, AuthorizationAmount = null, AuthorizationTransactionId = null, AuthorizationCaptureDateTime = null, AuthorizationCaptureAmount = null, AuthorizationCaptureSettlementAmount = null, AuthorizationCaptureTransactionId = null, AuthorizationCaptureResponse = null;
Update OrderLeaseHauler set UnitOfMeasureId = null;
Update OrderLine set Quantity = null, UnitOfMeasureId = null;
Update Receipt set FuelSurcharge = 0, FuelSurchargeRate = 0;
Update Ticket set MaterialQuantity = 0, FreightQuantity = 0, MaterialUomId = null, FreightUomId = null, TimeClassificationId = null, DriverPayRate = null;
Update Project set CustomerId = null, ContactId = null, SupplierId = null, LoadAt = null;
Update ProjectService set UnitOfMeasureId = null, Quantity = null;
Update Quote set SupplierId = null, LoadAt = null;
Update QuoteService set UnitOfMeasureId = null, Quantity = null;
Update OfficeServicePrice set UnitOfMeasureId = null;
Update PreventiveMaintenance set CompletedDate = null, CompletedMileage = null, CompletedHour = null;
");

            migrationBuilder.DropForeignKey(
                name: "FK_LeaseHaulerAgreementService_UnitOfMeasure_UnitOfMeasureId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_UnitOfMeasureId",
                table: "OfficeServicePrice");

            migrationBuilder.DropForeignKey(
                name: "FK_Order_Supplier_SupplierId",
                table: "Order");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLeaseHauler_UnitOfMeasure_UnitOfMeasureId",
                table: "OrderLeaseHauler");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_UnitOfMeasure_UnitOfMeasureId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_CustomerContact_ContactId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Customer_CustomerId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Supplier_SupplierId",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectService_UnitOfMeasure_UnitOfMeasureId",
                table: "ProjectService");

            migrationBuilder.DropForeignKey(
                name: "FK_Quote_Supplier_SupplierId",
                table: "Quote");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_UnitOfMeasureId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_UnitOfMeasure_FreightUomId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_UnitOfMeasure_MaterialUomId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Job_TimeClassificationId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_FreightUomId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_MaterialUomId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_TimeClassificationId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_QuoteService_UnitOfMeasureId",
                table: "QuoteService");

            migrationBuilder.DropIndex(
                name: "IX_Quote_SupplierId",
                table: "Quote");

            migrationBuilder.DropIndex(
                name: "IX_ProjectService_UnitOfMeasureId",
                table: "ProjectService");

            migrationBuilder.DropIndex(
                name: "IX_Project_ContactId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_CustomerId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_SupplierId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_OrderLine_UnitOfMeasureId",
                table: "OrderLine");

            migrationBuilder.DropIndex(
                name: "IX_OrderLeaseHauler_UnitOfMeasureId",
                table: "OrderLeaseHauler");

            migrationBuilder.DropIndex(
                name: "IX_Order_SupplierId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_OfficeServicePrice_UnitOfMeasureId",
                table: "OfficeServicePrice");

            migrationBuilder.DropIndex(
                name: "IX_LeaseHaulerAgreementService_UnitOfMeasureId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropColumn(
                name: "DriverPayRate",
                table: "Ticket");

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
                name: "TimeClassificationId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "FuelSurcharge",
                table: "Receipt");

            migrationBuilder.DropColumn(
                name: "FuelSurchargeRate",
                table: "Receipt");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "LoadAt",
                table: "Quote");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Quote");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "ProjectService");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "ProjectService");

            migrationBuilder.DropColumn(
                name: "ContactId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "LoadAt",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "CompletedDate",
                table: "PreventiveMaintenance");

            migrationBuilder.DropColumn(
                name: "CompletedHour",
                table: "PreventiveMaintenance");

            migrationBuilder.DropColumn(
                name: "CompletedMileage",
                table: "PreventiveMaintenance");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "OrderLeaseHauler");

            migrationBuilder.DropColumn(
                name: "AuthorizationAmount",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "AuthorizationCaptureAmount",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "AuthorizationCaptureDateTime",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "AuthorizationCaptureResponse",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "AuthorizationCaptureSettlementAmount",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "AuthorizationCaptureTransactionId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "AuthorizationDateTime",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "AuthorizationTransactionId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "CreditCardFirstName",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "CreditCardLastName",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "CreditCardStreetAddress",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "CreditCardToken",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "CreditCardZipCode",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "FuelSurcharge",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "FuelSurchargeRate",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "LoadAt",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "SupplierId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "OfficeServicePrice");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasureId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropColumn(
                name: "PayRate",
                table: "EmployeeTime");

            migrationBuilder.DropColumn(
                name: "PayMethod",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "PayRate",
                table: "Driver");

            migrationBuilder.DropColumn(
                name: "CreditCardFirstName",
                table: "Customer");

            migrationBuilder.DropColumn(
                name: "CreditCardLastName",
                table: "Customer");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DriverPayRate",
                table: "Ticket",
                nullable: true);

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
                name: "TimeClassificationId",
                table: "Ticket",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelSurcharge",
                table: "Receipt",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelSurchargeRate",
                table: "Receipt",
                type: "decimal(19, 4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "QuoteService",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "QuoteService",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoadAt",
                table: "Quote",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Quote",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "ProjectService",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "ProjectService",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ContactId",
                table: "Project",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CustomerId",
                table: "Project",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LoadAt",
                table: "Project",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Project",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedDate",
                table: "PreventiveMaintenance",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompletedHour",
                table: "PreventiveMaintenance",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompletedMileage",
                table: "PreventiveMaintenance",
                type: "decimal(19, 1)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Quantity",
                table: "OrderLine",
                type: "decimal(18, 4)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "OrderLine",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "OrderLeaseHauler",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AuthorizationAmount",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AuthorizationCaptureAmount",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuthorizationCaptureDateTime",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationCaptureResponse",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AuthorizationCaptureSettlementAmount",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationCaptureTransactionId",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AuthorizationDateTime",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AuthorizationTransactionId",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardFirstName",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardLastName",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardStreetAddress",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardToken",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardZipCode",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelSurcharge",
                table: "Order",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "FuelSurchargeRate",
                table: "Order",
                type: "decimal(19, 4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "LoadAt",
                table: "Order",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupplierId",
                table: "Order",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "OfficeServicePrice",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnitOfMeasureId",
                table: "LeaseHaulerAgreementService",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PayRate",
                table: "EmployeeTime",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PayMethod",
                table: "Driver",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PayRate",
                table: "Driver",
                type: "money",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardFirstName",
                table: "Customer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreditCardLastName",
                table: "Customer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_FreightUomId",
                table: "Ticket",
                column: "FreightUomId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_MaterialUomId",
                table: "Ticket",
                column: "MaterialUomId");

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_TimeClassificationId",
                table: "Ticket",
                column: "TimeClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteService_UnitOfMeasureId",
                table: "QuoteService",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Quote_SupplierId",
                table: "Quote",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_UnitOfMeasureId",
                table: "ProjectService",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_ContactId",
                table: "Project",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_CustomerId",
                table: "Project",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Project_SupplierId",
                table: "Project",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_UnitOfMeasureId",
                table: "OrderLine",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_OrderLeaseHauler_UnitOfMeasureId",
                table: "OrderLeaseHauler",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_Order_SupplierId",
                table: "Order",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_OfficeServicePrice_UnitOfMeasureId",
                table: "OfficeServicePrice",
                column: "UnitOfMeasureId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerAgreementService_UnitOfMeasureId",
                table: "LeaseHaulerAgreementService",
                column: "UnitOfMeasureId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseHaulerAgreementService_UnitOfMeasure_UnitOfMeasureId",
                table: "LeaseHaulerAgreementService",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OfficeServicePrice_UnitOfMeasure_UnitOfMeasureId",
                table: "OfficeServicePrice",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Order_Supplier_SupplierId",
                table: "Order",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLeaseHauler_UnitOfMeasure_UnitOfMeasureId",
                table: "OrderLeaseHauler",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_UnitOfMeasure_UnitOfMeasureId",
                table: "OrderLine",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_CustomerContact_ContactId",
                table: "Project",
                column: "ContactId",
                principalTable: "CustomerContact",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Customer_CustomerId",
                table: "Project",
                column: "CustomerId",
                principalTable: "Customer",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Supplier_SupplierId",
                table: "Project",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectService_UnitOfMeasure_UnitOfMeasureId",
                table: "ProjectService",
                column: "UnitOfMeasureId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Quote_Supplier_SupplierId",
                table: "Quote",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_UnitOfMeasure_UnitOfMeasureId",
                table: "QuoteService",
                column: "UnitOfMeasureId",
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

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_UnitOfMeasure_MaterialUomId",
                table: "Ticket",
                column: "MaterialUomId",
                principalTable: "UnitOfMeasure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Job_TimeClassificationId",
                table: "Ticket",
                column: "TimeClassificationId",
                principalTable: "Job",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
