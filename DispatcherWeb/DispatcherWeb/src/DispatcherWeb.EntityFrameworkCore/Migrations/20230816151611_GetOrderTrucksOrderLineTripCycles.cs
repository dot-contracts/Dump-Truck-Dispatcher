using DispatcherWeb.Helpers;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class GetOrderTrucksOrderLineTripCycles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = this.ReadSql();
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sqlScript = @"
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND OBJECT_ID = OBJECT_ID('dbo.sp_GetOrderTrucksOrderLineTripCycles'))
BEGIN
    DROP PROCEDURE [dbo].[sp_GetOrderTrucksOrderLineTripCycles]
END";
            migrationBuilder.Sql($"EXEC(N'{sqlScript}')");
        }
    }
}
