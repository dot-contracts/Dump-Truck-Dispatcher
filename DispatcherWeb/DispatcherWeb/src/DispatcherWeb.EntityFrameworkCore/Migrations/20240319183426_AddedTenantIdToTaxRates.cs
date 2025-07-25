using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedTenantIdToTaxRates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "TaxRate",
                type: "int",
                nullable: true);

            migrationBuilder.Sql(@"
                update t set TenantId = u.TenantId, IsDeleted = 0, DeleterUserId = null, DeletionTime = null
                from TaxRate t
                inner join AbpUsers u on u.Id = t.CreatorUserId
            ");

            migrationBuilder.Sql(@"
                delete
                from TaxRate
                where TenantId is null
            ");

            migrationBuilder.AlterColumn<int>(
                name: "TenantId",
                table: "TaxRate",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "TaxRate");
        }
    }
}
