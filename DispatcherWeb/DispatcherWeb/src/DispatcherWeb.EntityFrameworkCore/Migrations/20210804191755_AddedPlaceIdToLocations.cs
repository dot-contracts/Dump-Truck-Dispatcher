using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedPlaceIdToLocations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Location",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "PlaceId",
                table: "Location",
                nullable: true);

            migrationBuilder.Sql(@"
insert into LocationCategory (TenantId, [Name], PredefinedLocationCategoryKind, CreationTime, IsDeleted)
select Id, 'Project Site', 10, getdate(), 0 from AbpTenants where IsDeleted = 0
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlaceId",
                table: "Location");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Location",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
