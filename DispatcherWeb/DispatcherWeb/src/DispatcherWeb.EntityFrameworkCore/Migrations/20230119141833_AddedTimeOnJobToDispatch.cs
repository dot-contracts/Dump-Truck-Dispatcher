using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedTimeOnJobToDispatch : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "TimeOnJob",
                table: "Dispatch",
                type: "datetime2",
                nullable: true);

            migrationBuilder.Sql(@"UPDATE d
                SET TimeOnJob = Coalesce(olt.TimeOnJob, ol.TimeOnJob)
                FROM 
                Dispatch d
                INNER JOIN OrderLineTruck olt ON d.OrderLineTruckId = olt.Id
                INNER JOIN OrderLine ol ON d.OrderLineId = ol.Id
                WHERE d.TimeOnJob is null and (olt.TimeOnJob is not null or ol.TimeOnJob is not null)"
            );
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TimeOnJob",
                table: "Dispatch");
        }
    }
}
