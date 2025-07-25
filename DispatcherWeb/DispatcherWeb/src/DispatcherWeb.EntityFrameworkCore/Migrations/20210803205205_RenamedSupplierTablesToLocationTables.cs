using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RenamedSupplierTablesToLocationTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaseHaulerAgreementService_Supplier_SupplierId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Supplier_DeliverToId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Supplier_SupplierId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectService_Supplier_DeliverToId",
                table: "ProjectService");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectService_Supplier_SupplierId",
                table: "ProjectService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Supplier_DeliverToId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Supplier_SupplierId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Supplier_DeliverToId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Supplier_SupplierId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_Supplier_SupplierCategory_CategoryId",
                table: "Supplier");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierContact_Supplier_SupplierId",
                table: "SupplierContact");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Supplier_DeliverToId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Supplier_LoadAtId",
                table: "Ticket");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_dbo.SupplierCategory",
            //    table: "SupplierCategory");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_dbo.Supplier",
            //    table: "Supplier");

            migrationBuilder.RenameColumn(
                name: "PredefinedSupplierCategoryKind",
                table: "SupplierCategory",
                newName: "PredefinedLocationCategoryKind");

            migrationBuilder.RenameColumn(
                name: "PredefinedSupplierKind",
                table: "Supplier",
                newName: "PredefinedLocationKind");

            migrationBuilder.RenameTable(
                name: "SupplierCategory",
                newName: "LocationCategory");

            migrationBuilder.RenameTable(
                name: "Supplier",
                newName: "Location");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "SupplierContact",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierContact_SupplierId",
                table: "SupplierContact",
                newName: "IX_SupplierContact_LocationId");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "ReceiptLine",
                newName: "LoadAtId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptLine_SupplierId",
                table: "ReceiptLine",
                newName: "IX_ReceiptLine_LoadAtId");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "QuoteService",
                newName: "LoadAtId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_SupplierId",
                table: "QuoteService",
                newName: "IX_QuoteService_LoadAtId");

            migrationBuilder.RenameColumn(
                name: "DeliverTo",
                table: "Quote",
                newName: "JobSite");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "ProjectService",
                newName: "LoadAtId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectService_SupplierId",
                table: "ProjectService",
                newName: "IX_ProjectService_LoadAtId");

            migrationBuilder.RenameColumn(
                name: "DeliverTo",
                table: "Project",
                newName: "JobSite");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "OrderLine",
                newName: "LoadAtId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLine_SupplierId",
                table: "OrderLine",
                newName: "IX_OrderLine_LoadAtId");

            migrationBuilder.RenameColumn(
                name: "DeliverTo",
                table: "Order",
                newName: "JobSite");

            migrationBuilder.RenameColumn(
                name: "SupplierId",
                table: "LeaseHaulerAgreementService",
                newName: "LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaseHaulerAgreementService_SupplierId",
                table: "LeaseHaulerAgreementService",
                newName: "IX_LeaseHaulerAgreementService_LocationId");

            migrationBuilder.RenameIndex(
                name: "IX_Supplier_CategoryId",
                table: "Location",
                newName: "IX_Location_CategoryId");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_LocationCategory",
            //    table: "LocationCategory",
            //    column: "Id");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_Location",
            //    table: "Location",
            //    column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseHaulerAgreementService_Location_LocationId",
                table: "LeaseHaulerAgreementService",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Location_LocationCategory_CategoryId",
                table: "Location",
                column: "CategoryId",
                principalTable: "LocationCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Location_DeliverToId",
                table: "OrderLine",
                column: "DeliverToId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Location_LoadAtId",
                table: "OrderLine",
                column: "LoadAtId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectService_Location_DeliverToId",
                table: "ProjectService",
                column: "DeliverToId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectService_Location_LoadAtId",
                table: "ProjectService",
                column: "LoadAtId",
                principalTable: "Location",
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
                name: "FK_ReceiptLine_Location_DeliverToId",
                table: "ReceiptLine",
                column: "DeliverToId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Location_LoadAtId",
                table: "ReceiptLine",
                column: "LoadAtId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierContact_Location_LocationId",
                table: "SupplierContact",
                column: "LocationId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Location_DeliverToId",
                table: "Ticket",
                column: "DeliverToId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Location_LoadAtId",
                table: "Ticket",
                column: "LoadAtId",
                principalTable: "Location",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaseHaulerAgreementService_Location_LocationId",
                table: "LeaseHaulerAgreementService");

            migrationBuilder.DropForeignKey(
                name: "FK_Location_LocationCategory_CategoryId",
                table: "Location");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Location_DeliverToId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Location_LoadAtId",
                table: "OrderLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectService_Location_DeliverToId",
                table: "ProjectService");

            migrationBuilder.DropForeignKey(
                name: "FK_ProjectService_Location_LoadAtId",
                table: "ProjectService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Location_DeliverToId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Location_LoadAtId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Location_DeliverToId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Location_LoadAtId",
                table: "ReceiptLine");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierContact_Location_LocationId",
                table: "SupplierContact");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Location_DeliverToId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_Location_LoadAtId",
                table: "Ticket");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_LocationCategory",
            //    table: "LocationCategory");

            //migrationBuilder.DropPrimaryKey(
            //    name: "PK_Location",
            //    table: "Location");

            migrationBuilder.RenameColumn(
                name: "PredefinedLocationCategoryKind",
                table: "SupplierCategory",
                newName: "PredefinedSupplierCategoryKind");

            migrationBuilder.RenameColumn(
                name: "PredefinedLocationKind",
                table: "Supplier",
                newName: "PredefinedSupplierKind");

            migrationBuilder.RenameTable(
                name: "LocationCategory",
                newName: "SupplierCategory");

            migrationBuilder.RenameTable(
                name: "Location",
                newName: "Supplier");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "SupplierContact",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_SupplierContact_LocationId",
                table: "SupplierContact",
                newName: "IX_SupplierContact_SupplierId");

            migrationBuilder.RenameColumn(
                name: "LoadAtId",
                table: "ReceiptLine",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_ReceiptLine_LoadAtId",
                table: "ReceiptLine",
                newName: "IX_ReceiptLine_SupplierId");

            migrationBuilder.RenameColumn(
                name: "LoadAtId",
                table: "QuoteService",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_QuoteService_LoadAtId",
                table: "QuoteService",
                newName: "IX_QuoteService_SupplierId");

            migrationBuilder.RenameColumn(
                name: "JobSite",
                table: "Quote",
                newName: "DeliverTo");

            migrationBuilder.RenameColumn(
                name: "LoadAtId",
                table: "ProjectService",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_ProjectService_LoadAtId",
                table: "ProjectService",
                newName: "IX_ProjectService_SupplierId");

            migrationBuilder.RenameColumn(
                name: "JobSite",
                table: "Project",
                newName: "DeliverTo");

            migrationBuilder.RenameColumn(
                name: "LoadAtId",
                table: "OrderLine",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_OrderLine_LoadAtId",
                table: "OrderLine",
                newName: "IX_OrderLine_SupplierId");

            migrationBuilder.RenameColumn(
                name: "JobSite",
                table: "Order",
                newName: "DeliverTo");

            migrationBuilder.RenameColumn(
                name: "LocationId",
                table: "LeaseHaulerAgreementService",
                newName: "SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_LeaseHaulerAgreementService_LocationId",
                table: "LeaseHaulerAgreementService",
                newName: "IX_LeaseHaulerAgreementService_SupplierId");

            migrationBuilder.RenameIndex(
                name: "IX_Location_CategoryId",
                table: "Supplier",
                newName: "IX_Supplier_CategoryId");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_dbo.SupplierCategory",
            //    table: "SupplierCategory",
            //    column: "Id");

            //migrationBuilder.AddPrimaryKey(
            //    name: "PK_dbo.Supplier",
            //    table: "Supplier",
            //    column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseHaulerAgreementService_Supplier_SupplierId",
                table: "LeaseHaulerAgreementService",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Supplier_DeliverToId",
                table: "OrderLine",
                column: "DeliverToId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Supplier_SupplierId",
                table: "OrderLine",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectService_Supplier_DeliverToId",
                table: "ProjectService",
                column: "DeliverToId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectService_Supplier_SupplierId",
                table: "ProjectService",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Supplier_DeliverToId",
                table: "QuoteService",
                column: "DeliverToId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_QuoteService_Supplier_SupplierId",
                table: "QuoteService",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Supplier_DeliverToId",
                table: "ReceiptLine",
                column: "DeliverToId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ReceiptLine_Supplier_SupplierId",
                table: "ReceiptLine",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Supplier_SupplierCategory_CategoryId",
                table: "Supplier",
                column: "CategoryId",
                principalTable: "SupplierCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierContact_Supplier_SupplierId",
                table: "SupplierContact",
                column: "SupplierId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Supplier_DeliverToId",
                table: "Ticket",
                column: "DeliverToId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_Supplier_LoadAtId",
                table: "Ticket",
                column: "LoadAtId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
