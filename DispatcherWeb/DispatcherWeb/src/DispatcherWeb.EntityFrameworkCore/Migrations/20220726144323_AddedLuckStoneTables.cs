using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedLuckStoneTables : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LuckStoneEarningsBatch",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_LuckStoneEarningsBatch", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LuckStoneLocation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Site = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    StreetAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ZipCode = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: true),
                    CountryCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LuckStoneLocation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LuckStoneEarnings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    TicketDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Site = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AccountNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CustomerName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    LicensePlate = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    StoneRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    NetTons = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    StoneAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerPONumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SalesTaxAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CustomerTotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductDescription = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BatchId = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_LuckStoneEarnings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LuckStoneEarnings_LuckStoneEarningsBatch_BatchId",
                        column: x => x.BatchId,
                        principalTable: "LuckStoneEarningsBatch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LuckStoneEarnings_BatchId",
                table: "LuckStoneEarnings",
                column: "BatchId");

            migrationBuilder.Sql(@"
                Insert into LuckStoneLocation
                (Site, Name, StreetAddress, City, State, ZipCode, CountryCode, Latitude, Longitude)
                values
                ('Boscobel', 'Luck Stone', '485 Quarry Drive', 'Manakin Sabot', 'VA', '23103', null, null, null),
                ('Burkeville', 'Luck Stone', '500 Ronnie Armes Way', 'Burkeville', 'VA', '23922', null, null, null),
                ('Caroline', 'Luck Stone', '19380 Richmond Turnpike', 'Milford', 'VA', '22514', null, null, null),
                ('Caroline Ecosystems', 'Luck Stone', '19380 Richmond Turnpike', 'Milford', 'VA', '22514', null, null, null),
                ('Charlottesville', 'Luck Stone', 'P.O. Box 687', 'Keswick', 'VA', '22947', null, null, null),
                ('Culpeper', 'Luck Stone', 'Germanna Highway', 'Culpeper', 'VA', '22701', null, null, null),
                ('Ecosystems Greene', 'Luck Stone', '221 Luck Stone Rd', 'Ruckersville', 'VA', '22968', null, null, null),
                ('Ecosystems South Richmond (CLOSED)', 'Luck Stone', '2100 Deepwater Terminal Rd', 'Richmond', 'VA', '23234', null, null, null),
                ('Gilmerton Yard', 'Luck Stone', '4608 Bainbridge Boulevard', 'Chesapeake', 'VA', '23320', null, null, null),
                ('Greene', 'Luck Stone', '221 Luck Stone Road', 'Ruckersville', 'VA', '22968', null, null, null),
                ('Greene Ecosystems', 'Luck Stone', '221 Luck Stone Road', 'Ruckersville', 'VA', '22968', null, null, null),
                ('Powhatan', 'Luck Stone', '1920 Anderson Highway', 'Powhatan', 'VA', '23139', null, null, null),
                ('Prince Edward', 'Luck Stone', '11779 Prince Edward Hwy', 'Farmville', 'VA', '23901', null, null, null),
                ('Rockville', 'Luck Stone', '2115 Ashland Road', 'Rockville', 'VA', '23146', null, null, null),
                ('South Richmond QUARRY', 'Luck Stone', '2100 Deepwater Terminal Rd', 'Richmond', 'VA', '23234', null, null, null),
                ('Spotsylvania', 'Luck Stone', '9100 Luck Stone Lane', 'Fredericksburg', 'VA', '22407', null, null, null)
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LuckStoneEarnings");

            migrationBuilder.DropTable(
                name: "LuckStoneLocation");

            migrationBuilder.DropTable(
                name: "LuckStoneEarningsBatch");
        }
    }
}
