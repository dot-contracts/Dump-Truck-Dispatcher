using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedDeliverToIdToOrderLines : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeliverToId",
                table: "OrderLine",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_DeliverToId",
                table: "OrderLine",
                column: "DeliverToId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_Supplier_DeliverToId",
                table: "OrderLine",
                column: "DeliverToId",
                principalTable: "Supplier",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"Update Supplier set Name = 'Job Site' where PredefinedSupplierKind = 1");

            migrationBuilder.Sql(@"update OrderLine set DeliverToId = SupplierId where Designation in (5, 6, 7)");
            migrationBuilder.Sql(@"update ol set ol.SupplierId = s.Id
from OrderLine ol
left join Supplier s on s.TenantId = ol.TenantId
where ol.Designation in (5, 6, 7) and s.PredefinedSupplierKind = 1");

            migrationBuilder.Sql(@"update ol set ol.DeliverToId = s.Id
from OrderLine ol
left join Supplier s on s.TenantId = ol.TenantId
where ol.Designation not in (5, 6, 7) and s.PredefinedSupplierKind = 1");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_Supplier_DeliverToId",
                table: "OrderLine");

            migrationBuilder.DropIndex(
                name: "IX_OrderLine_DeliverToId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "DeliverToId",
                table: "OrderLine");
        }
    }
}
