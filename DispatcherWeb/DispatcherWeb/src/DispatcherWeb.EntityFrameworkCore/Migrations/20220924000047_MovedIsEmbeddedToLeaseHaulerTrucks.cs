using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class MovedIsEmbeddedToLeaseHaulerTrucks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmbedded",
                table: "LeaseHaulerTruck",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Order_LastModifierUserId",
                table: "Order",
                column: "LastModifierUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Order_AbpUsers_LastModifierUserId",
                table: "Order",
                column: "LastModifierUserId",
                principalTable: "AbpUsers",
                principalColumn: "Id");

            migrationBuilder.Sql(@"
                insert into LeaseHauler
                (Name, IsDeleted, CreationTime, TenantId)
                select distinct 'Unknown', 0, GetDate(), TenantId from Truck where IsEmbedded = 1
            ");

            migrationBuilder.Sql(@"
                insert into LeaseHaulerTruck
                (TruckId, TenantId, CreationTime, IsDeleted, IsEmbedded, LeaseHaulerId)
                select tr.Id TruckId, tr.TenantId, GetDate(), tr.IsDeleted, 1, lh.Id LeaseHaulerId
                from Truck tr
                inner join LeaseHauler lh on lh.TenantId = tr.TenantId
                where tr.IsEmbedded = 1 and lh.Name = 'Unknown'
            ");
            //migrationBuilder.Sql(@"
            //    update tr set LocationId = null
            //    from Truck tr
            //    where tr.IsEmbedded = 1
            //");

            migrationBuilder.Sql(@"
                insert into LeaseHaulerDriver
                (DriverId, TenantId, CreationTime, IsDeleted, LeaseHaulerId)
                select distinct dr.Id, dr.TenantId, GetDate(), dr.IsDeleted, lh.Id LeaseHaulerId
                from Driver dr
                inner join Truck tr on tr.DefaultDriverId = dr.Id
                inner join LeaseHauler lh on lh.TenantId = dr.TenantId
                where tr.IsEmbedded = 1 and lh.Name = 'Unknown'
            ");
            migrationBuilder.Sql(@"
                update dr set OfficeId = null, IsExternal = 1
                from Driver dr
                inner join Truck tr on tr.DefaultDriverId = dr.Id
                where tr.IsEmbedded = 1
            ");
            migrationBuilder.Sql(@"
                update ur set ur.RoleId = r2.Id
                from Driver dr
                inner join Truck tr on tr.DefaultDriverId = dr.Id
                inner join AbpUsers u on u.Id = dr.UserId
                inner join AbpUserRoles ur on ur.UserId = u.Id
                inner join AbpRoles r on r.Id = ur.RoleId
                inner join AbpRoles r2 on r2.TenantId = r.TenantId
                where tr.IsEmbedded = 1 and r.Name = 'Driver' and r2.Name = 'LeaseHaulerDriver'
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Order_AbpUsers_LastModifierUserId",
                table: "Order");

            migrationBuilder.DropIndex(
                name: "IX_Order_LastModifierUserId",
                table: "Order");

            migrationBuilder.DropColumn(
                name: "IsEmbedded",
                table: "LeaseHaulerTruck");
        }
    }
}
