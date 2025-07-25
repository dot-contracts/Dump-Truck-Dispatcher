using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedQuoteServiceIdToOrderLine : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "QuoteServiceId",
                table: "OrderLine",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderLine_QuoteServiceId",
                table: "OrderLine",
                column: "QuoteServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderLine_QuoteService_QuoteServiceId",
                table: "OrderLine",
                column: "QuoteServiceId",
                principalTable: "QuoteService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                update ol set QuoteServiceId = qs.Id
                from OrderLine ol
                inner join AbpTenants t on t.Id = ol.TenantId
                inner join [Order] o on o.Id = ol.OrderId
                inner join [Quote] q on q.Id = o.QuoteId
                left join [QuoteService] qs on qs.QuoteId = o.QuoteId
                and qs.ServiceId = ol.ServiceId
                and qs.Designation = ol.Designation
                and qs.LoadAtId = ol.LoadAtId
                and qs.DeliverToId = ol.DeliverToId
                where
                ol.IsDeleted = 0
                and o.IsDeleted = 0
                and q.IsDeleted = 0
                and (qs.IsDeleted = 0 or qs.IsDeleted is null)
                and t.IsActive = 1
                and t.IsDeleted = 0
                and qs.Id is not null
                and ol.QuoteServiceId is null
            ");

            migrationBuilder.Sql(@"
                update ol set QuoteServiceId = qs.Id
                from OrderLine ol
                inner join AbpTenants t on t.Id = ol.TenantId
                inner join [Order] o on o.Id = ol.OrderId
                inner join [Quote] q on q.Id = o.QuoteId
                left join [QuoteService] qs on qs.QuoteId = o.QuoteId
                and qs.ServiceId = ol.ServiceId
                and qs.Designation = ol.Designation
                and qs.LoadAtId = ol.LoadAtId
                where
                ol.IsDeleted = 0
                and o.IsDeleted = 0
                and q.IsDeleted = 0
                and (qs.IsDeleted = 0 or qs.IsDeleted is null)
                and t.IsActive = 1
                and t.IsDeleted = 0
                and qs.Id is not null
                and ol.QuoteServiceId is null
            ");

            migrationBuilder.Sql(@"
                update ol set QuoteServiceId = qs.Id
                from OrderLine ol
                inner join AbpTenants t on t.Id = ol.TenantId
                inner join [Order] o on o.Id = ol.OrderId
                inner join [Quote] q on q.Id = o.QuoteId
                left join [QuoteService] qs on qs.QuoteId = o.QuoteId
                and qs.ServiceId = ol.ServiceId
                and qs.Designation = ol.Designation
                and qs.DeliverToId = ol.DeliverToId
                where
                ol.IsDeleted = 0
                and o.IsDeleted = 0
                and q.IsDeleted = 0
                and (qs.IsDeleted = 0 or qs.IsDeleted is null)
                and t.IsActive = 1
                and t.IsDeleted = 0
                and qs.Id is not null
                and ol.QuoteServiceId is null
            ");

            migrationBuilder.Sql(@"
                update ol set QuoteServiceId = qs.Id
                from OrderLine ol
                inner join AbpTenants t on t.Id = ol.TenantId
                inner join [Order] o on o.Id = ol.OrderId
                inner join [Quote] q on q.Id = o.QuoteId
                left join [QuoteService] qs on qs.QuoteId = o.QuoteId
                and qs.ServiceId = ol.ServiceId
                and qs.Designation = ol.Designation
                where
                ol.IsDeleted = 0
                and o.IsDeleted = 0
                and q.IsDeleted = 0
                and (qs.IsDeleted = 0 or qs.IsDeleted is null)
                and t.IsActive = 1
                and t.IsDeleted = 0
                and qs.Id is not null
                and ol.QuoteServiceId is null
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderLine_QuoteService_QuoteServiceId",
                table: "OrderLine");

            migrationBuilder.DropIndex(
                name: "IX_OrderLine_QuoteServiceId",
                table: "OrderLine");

            migrationBuilder.DropColumn(
                name: "QuoteServiceId",
                table: "OrderLine");
        }
    }
}
