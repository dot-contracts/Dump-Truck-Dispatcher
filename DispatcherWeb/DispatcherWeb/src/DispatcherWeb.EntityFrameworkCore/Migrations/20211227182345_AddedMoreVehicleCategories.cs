using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedMoreVehicleCategories : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET IDENTITY_INSERT [dbo].[VehicleCategory] ON;

                INSERT INTO [dbo].[VehicleCategory] ([Id], [Name], [AssetType], [IsPowered], [SortOrder])
                    VALUES
                        (23, 'Hopper Bottom Trailer',          3, 0, 18),
                        (24, 'Liquid Asphalt Tanker Truck',    4, 1, 13),
                        (25, 'Refrigerated Trailer',           3, 0, 24),
                        (26, 'Super Tag Dump Truck',           1, 1, 6),
                        (27, 'Super 10 Dump Truck',            1, 1, 7);

                SET IDENTITY_INSERT [dbo].[VehicleCategory] OFF;

                update VehicleCategory set Name = 'Dump Truck',           SortOrder = 1  where Id = 1;
                update VehicleCategory set Name = 'Centipede Dump Truck', SortOrder = 2  where Id = 21;
                update VehicleCategory set Name = 'Triaxle Dump Truck',   SortOrder = 3  where Id = 7;
                update VehicleCategory set Name = 'Quad Dump Truck',      SortOrder = 4  where Id = 8;
                update VehicleCategory set Name = 'Quint Dump Truck',     SortOrder = 5  where Id = 9;
                update VehicleCategory set Name = 'Super Tag Dump Truck', SortOrder = 6  where Id = 26;
                update VehicleCategory set Name = 'Super 10 Dump Truck',  SortOrder = 7  where Id = 27;
                update VehicleCategory set Name = 'Tandem Dump Truck',    SortOrder = 8  where Id = 22;
                update VehicleCategory set Name = 'Tractor',              SortOrder = 9  where Id = 3;
                update VehicleCategory set Name = 'Cement Truck',         SortOrder = 10 where Id = 11;
                update VehicleCategory set Name = 'Concrete Mixer',       SortOrder = 11 where Id = 12;
                update VehicleCategory set Name = 'Water Truck',          SortOrder = 12 where Id = 10;
                update VehicleCategory set Name = 'Liquid Asphalt Tanker Truck',   SortOrder = 13 where Id = 24;
                update VehicleCategory set Name = 'Trailer',              SortOrder = 14 where Id = 2;
                update VehicleCategory set Name = 'Belly Dump Trailer',   SortOrder = 15 where Id = 13;
                update VehicleCategory set Name = 'End Dump Trailer',     SortOrder = 16 where Id = 14;
                update VehicleCategory set Name = 'Flat Bed Trailer',     SortOrder = 17 where Id = 17;
                update VehicleCategory set Name = 'Hopper Bottom Trailer', SortOrder = 18 where Id = 23;
                update VehicleCategory set Name = 'Live Bottom Trailer',  SortOrder = 19 where Id = 20;
                update VehicleCategory set Name = 'Lowboy Trailer',       SortOrder = 20 where Id = 16;
                update VehicleCategory set Name = 'Walking Bed Trailer',  SortOrder = 21 where Id = 15;
                update VehicleCategory set Name = 'Flowboy',              SortOrder = 22 where Id = 19;
                update VehicleCategory set Name = 'Stone Slinger',        SortOrder = 23 where Id = 18;
                update VehicleCategory set Name = 'Refrigerated Trailer', SortOrder = 24 where Id = 25;
                update VehicleCategory set Name = 'Other', SortOrder = 25 where Id = 6;
                ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
