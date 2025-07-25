using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedDeliverToIdToQuoteAndReceiptAndProjectLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliverToId",
                table: "ReceiptLine",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliverToId",
                table: "QuoteService",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DeliverToId",
                table: "ProjectService",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptLine_DeliverToId",
                table: "ReceiptLine",
                column: "DeliverToId");

            migrationBuilder.CreateIndex(
                name: "IX_QuoteService_DeliverToId",
                table: "QuoteService",
                column: "DeliverToId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectService_DeliverToId",
                table: "ProjectService",
                column: "DeliverToId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProjectService_Supplier_DeliverToId",
                table: "ProjectService",
                column: "DeliverToId",
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
                name: "FK_ReceiptLine_Supplier_DeliverToId",
                table: "ReceiptLine",
                column: "DeliverToId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);


            migrationBuilder.Sql(@"update ReceiptLine set DeliverToId = SupplierId where Designation in (5, 6, 7)");
            migrationBuilder.Sql(@"update ol set ol.SupplierId = s.Id
from ReceiptLine ol
left join Supplier s on s.TenantId = ol.TenantId
where ol.Designation in (5, 6, 7) and s.PredefinedSupplierKind = 1");
            migrationBuilder.Sql(@"update ol set ol.DeliverToId = s.Id
from ReceiptLine ol
left join Supplier s on s.TenantId = ol.TenantId
where ol.Designation not in (5, 6, 7) and s.PredefinedSupplierKind = 1");


            migrationBuilder.Sql(@"update QuoteService set DeliverToId = SupplierId where Designation in (5, 6, 7)");
            migrationBuilder.Sql(@"update ol set ol.SupplierId = s.Id
from QuoteService ol
left join Supplier s on s.TenantId = ol.TenantId
where ol.Designation in (5, 6, 7) and s.PredefinedSupplierKind = 1");
            migrationBuilder.Sql(@"update ol set ol.DeliverToId = s.Id
from QuoteService ol
left join Supplier s on s.TenantId = ol.TenantId
where ol.Designation not in (5, 6, 7) and s.PredefinedSupplierKind = 1");


            migrationBuilder.Sql(@"update ProjectService set DeliverToId = SupplierId where Designation in (5, 6, 7)");
            migrationBuilder.Sql(@"update ol set ol.SupplierId = s.Id
from ProjectService ol
left join Supplier s on s.TenantId = ol.TenantId
where ol.Designation in (5, 6, 7) and s.PredefinedSupplierKind = 1");
            migrationBuilder.Sql(@"update ol set ol.DeliverToId = s.Id
from ProjectService ol
left join Supplier s on s.TenantId = ol.TenantId
where ol.Designation not in (5, 6, 7) and s.PredefinedSupplierKind = 1");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProjectService_Supplier_DeliverToId",
                table: "ProjectService");

            migrationBuilder.DropForeignKey(
                name: "FK_QuoteService_Supplier_DeliverToId",
                table: "QuoteService");

            migrationBuilder.DropForeignKey(
                name: "FK_ReceiptLine_Supplier_DeliverToId",
                table: "ReceiptLine");

            migrationBuilder.DropIndex(
                name: "IX_ReceiptLine_DeliverToId",
                table: "ReceiptLine");

            migrationBuilder.DropIndex(
                name: "IX_QuoteService_DeliverToId",
                table: "QuoteService");

            migrationBuilder.DropIndex(
                name: "IX_ProjectService_DeliverToId",
                table: "ProjectService");

            migrationBuilder.DropColumn(
                name: "DeliverToId",
                table: "ReceiptLine");

            migrationBuilder.DropColumn(
                name: "DeliverToId",
                table: "QuoteService");

            migrationBuilder.DropColumn(
                name: "DeliverToId",
                table: "ProjectService");
        }
    }
}
