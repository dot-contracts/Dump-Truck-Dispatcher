using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedLeaseHaulerPortalTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NumberTrucksRequested",
                table: "LeaseHaulerRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderLineId",
                table: "LeaseHaulerRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "LeaseHaulerRequest",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AllowPortalAccess",
                table: "LeaseHaulerContact",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "LeaseHaulerUser",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    LeaseHaulerId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeaseHaulerUser", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerUser_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LeaseHaulerUser_LeaseHauler_LeaseHaulerId",
                        column: x => x.LeaseHaulerId,
                        principalTable: "LeaseHauler",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OneTimeLogin",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    IsExpired = table.Column<bool>(type: "bit", nullable: false),
                    ExpiryTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimeLogin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneTimeLogin_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RequestedLeaseHaulerTruck",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    LeaseHaulerRequestId = table.Column<int>(type: "int", nullable: false),
                    TruckId = table.Column<int>(type: "int", nullable: false),
                    DriverId = table.Column<int>(type: "int", nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestedLeaseHaulerTruck", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestedLeaseHaulerTruck_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestedLeaseHaulerTruck_LeaseHaulerRequest_LeaseHaulerRequestId",
                        column: x => x.LeaseHaulerRequestId,
                        principalTable: "LeaseHaulerRequest",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestedLeaseHaulerTruck_Truck_TruckId",
                        column: x => x.TruckId,
                        principalTable: "Truck",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerRequest_OrderLineId",
                table: "LeaseHaulerRequest",
                column: "OrderLineId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerUser_LeaseHaulerId",
                table: "LeaseHaulerUser",
                column: "LeaseHaulerId");

            migrationBuilder.CreateIndex(
                name: "IX_LeaseHaulerUser_UserId",
                table: "LeaseHaulerUser",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneTimeLogin_UserId",
                table: "OneTimeLogin",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestedLeaseHaulerTruck_DriverId",
                table: "RequestedLeaseHaulerTruck",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestedLeaseHaulerTruck_LeaseHaulerRequestId",
                table: "RequestedLeaseHaulerTruck",
                column: "LeaseHaulerRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestedLeaseHaulerTruck_TruckId",
                table: "RequestedLeaseHaulerTruck",
                column: "TruckId");

            migrationBuilder.AddForeignKey(
                name: "FK_LeaseHaulerRequest_OrderLine_OrderLineId",
                table: "LeaseHaulerRequest",
                column: "OrderLineId",
                principalTable: "OrderLine",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
                INSERT INTO dbo.AbpRoles
                (DisplayName, IsStatic, IsDefault, TenantId, Name, IsDeleted, DeleterUserId, DeletionTime, LastModificationTime, LastModifierUserId, CreationTime, CreatorUserId, ConcurrencyStamp, NormalizedName)
                SELECT 'LeaseHaulerAdministrator' [DisplayName]
                        , 1 [IsStatic]
                        , 0 [IsDefault]
                        , t1.Id [TenantId]
                        , 'LeaseHaulerAdministrator' [Name]
                        , 0 [IsDeleted]
                        , NULL [DeleterUserId]
                        , NULL [DeletionTime]
                        , NULL [LastModificationTime]
                        , NULL [LastModifierUserId]
                        , GETDATE() [CreationTime]
                        , NULL [CreatorUserId]
                        , NEWID() [ConcurrencyStamp]
                        , 'LEASEHAULERADMINISTRATOR' [NormalizedName]
                FROM AbpTenants t1
                WHERE NOT EXISTS(SELECT Id FROM dbo.AbpRoles WHERE [Name]='LeaseHaulerAdministrator' AND TenantId=t1.Id)
            ");

            migrationBuilder.Sql(@"
                INSERT INTO dbo.AbpRoles
                (DisplayName, IsStatic, IsDefault, TenantId, Name, IsDeleted, DeleterUserId, DeletionTime, LastModificationTime, LastModifierUserId, CreationTime, CreatorUserId, ConcurrencyStamp, NormalizedName)
                SELECT 'LeaseHaulerDispatcher' [DisplayName]
                        , 1 [IsStatic]
                        , 0 [IsDefault]
                        , t1.Id [TenantId]
                        , 'LeaseHaulerDispatcher' [Name]
                        , 0 [IsDeleted]
                        , NULL [DeleterUserId]
                        , NULL [DeletionTime]
                        , NULL [LastModificationTime]
                        , NULL [LastModifierUserId]
                        , GETDATE() [CreationTime]
                        , NULL [CreatorUserId]
                        , NEWID() [ConcurrencyStamp]
                        , 'LEASEHAULERDISPATCHER' [NormalizedName]
                FROM AbpTenants t1
                WHERE NOT EXISTS(SELECT Id FROM dbo.AbpRoles WHERE [Name]='LeaseHaulerDispatcher' AND TenantId=t1.Id)
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LeaseHaulerRequest_OrderLine_OrderLineId",
                table: "LeaseHaulerRequest");

            migrationBuilder.DropTable(
                name: "LeaseHaulerUser");

            migrationBuilder.DropTable(
                name: "OneTimeLogin");

            migrationBuilder.DropTable(
                name: "RequestedLeaseHaulerTruck");

            migrationBuilder.DropIndex(
                name: "IX_LeaseHaulerRequest_OrderLineId",
                table: "LeaseHaulerRequest");

            migrationBuilder.DropColumn(
                name: "NumberTrucksRequested",
                table: "LeaseHaulerRequest");

            migrationBuilder.DropColumn(
                name: "OrderLineId",
                table: "LeaseHaulerRequest");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "LeaseHaulerRequest");

            migrationBuilder.DropColumn(
                name: "AllowPortalAccess",
                table: "LeaseHaulerContact");
        }
    }
}
