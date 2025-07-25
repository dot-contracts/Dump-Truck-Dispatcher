using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedVehicleCategoryTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BedConstruction",
                table: "Truck",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CanPullTrailer",
                table: "Truck",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApportioned",
                table: "Truck",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmbedded",
                table: "Truck",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "VehicleCategoryId",
                table: "Truck",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "VehicleCategory",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    AssetType = table.Column<int>(nullable: false),
                    IsPowered = table.Column<bool>(nullable: false),
                    SortOrder = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VehicleCategory", x => x.Id);
                });



            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [dbo].[VehicleCategory] ON;

                INSERT INTO [dbo].[VehicleCategory] ([Id], [Name], [AssetType], [IsPowered], [SortOrder])
                    VALUES
                        (1, 'DumpTruck',            1, 1,    1),
                        (2, 'Trailer',              3, 0,   11),
                        (3, 'Tractor',              2, 1,    5),
                        --(4, 'LeasedDumpTruck',    1, 1,    0), 4 --> 1, IsEmbedded = 1
                        --(5, 'LeasedTractor',      2, 1,    0), 5 --> 3, IsEmbedded = 1
                        (6, 'Other',                4, 0,   20),
                        (7, 'TriaxleDumpTruck',     1, 1,    2),
                        (8, 'QuadDumpTruck',        1, 1,    3),
                        (9, 'QuintDumpTruck',       1, 1,    4),
                        (10, 'WaterTruck',          4, 1,    6),
                        (11, 'CementTruck',         4, 1,    7),
                        (12, 'ConcreteMixer',       4, 1,    8),
                        (13, 'BellyDumpTrailer',    3, 0,   12),
                        (14, 'EndDumpTrailer',      3, 0,   13),
                        (15, 'WalkingBedTrailer',   3, 0,   14),
                        (16, 'LowBoyTrailer',       3, 0,   15),
                        (17, 'FlatBedTrailer',      3, 0,   16),
                        (18, 'StoneSlinger',        3, 0,   17),
                        (19, 'Flowboy',             3, 0,   18),
                        (20, 'LiveBottomTrailer',   3, 0,   19),
                        (21, 'CentipedeDumpTruck',  1, 1,    9),
                        (22, 'TandemDumpTruck',     1, 1,    10);

                SET IDENTITY_INSERT [dbo].[VehicleCategory] OFF;
                ");

            migrationBuilder.Sql(@"Update Truck set VehicleCategoryId = Category");
            migrationBuilder.Sql(@"Update Truck set VehicleCategoryId = 1 where VehicleCategoryId < 1 or VehicleCategoryId > 22");
            migrationBuilder.Sql(@"Update Truck set VehicleCategoryId = 1, IsEmbedded = 1 where VehicleCategoryId = 4");
            migrationBuilder.Sql(@"Update Truck set VehicleCategoryId = 3, IsEmbedded = 1 where VehicleCategoryId = 5");
            migrationBuilder.Sql(@"Update Truck set IsEmbedded = 0 where LocationId is null");
            migrationBuilder.Sql(@"Update Truck set CanPullTrailer = 1 where VehicleCategoryId in (3)"); //VehicleCategories with AssetType == Tractor



            migrationBuilder.CreateIndex(
                name: "IX_Truck_VehicleCategoryId",
                table: "Truck",
                column: "VehicleCategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Truck_VehicleCategory_VehicleCategoryId",
                table: "Truck",
                column: "VehicleCategoryId",
                principalTable: "VehicleCategory",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Truck_VehicleCategory_VehicleCategoryId",
                table: "Truck");

            migrationBuilder.DropTable(
                name: "VehicleCategory");

            migrationBuilder.DropIndex(
                name: "IX_Truck_VehicleCategoryId",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "BedConstruction",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "CanPullTrailer",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "IsApportioned",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "IsEmbedded",
                table: "Truck");

            migrationBuilder.DropColumn(
                name: "VehicleCategoryId",
                table: "Truck");
        }
    }
}
