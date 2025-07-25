using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class ReduceDirectionsFieldOfOrderTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            string sql = "update [Order] set [Directions] = SUBSTRING([Directions], 0, 1000) where len([Directions]) > 1000";
            migrationBuilder.Sql(sql);

            migrationBuilder.AlterColumn<string>(
                name: "Directions",
                table: "Order",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Directions",
                table: "Order",
                nullable: true,
                oldClrType: typeof(string),
                oldMaxLength: 1000,
                oldNullable: true);
        }
    }
}
