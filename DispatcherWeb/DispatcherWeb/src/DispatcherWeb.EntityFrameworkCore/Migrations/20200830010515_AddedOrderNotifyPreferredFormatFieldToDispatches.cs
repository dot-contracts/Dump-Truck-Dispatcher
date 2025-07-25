using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedOrderNotifyPreferredFormatFieldToDispatches : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderNotifyPreferredFormat",
                table: "Dispatch",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"update di set di.OrderNotifyPreferredFormat = dr.OrderNotifyPreferredFormat
from Dispatch di
left join Driver dr on dr.Id = di.DriverId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNotifyPreferredFormat",
                table: "Dispatch");
        }
    }
}
