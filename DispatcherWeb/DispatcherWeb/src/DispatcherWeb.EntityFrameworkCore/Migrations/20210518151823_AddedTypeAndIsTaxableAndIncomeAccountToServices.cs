using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedTypeAndIsTaxableAndIncomeAccountToServices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IncomeAccount",
                table: "Service",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsTaxable",
                table: "Service",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Service",
                nullable: true);

            migrationBuilder.Sql(@"
update [Service] set IsActive = 0, [Type] = 0
where PredefinedServiceKind is not null
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IncomeAccount",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "IsTaxable",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Service");
        }
    }
}
