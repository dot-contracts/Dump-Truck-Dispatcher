using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class TruncateTruckCodeInTruckTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string sql = "update Truck set TruckCode = SUBSTRING(TruckCode, 0, 25) where len(TruckCode) > 25";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
