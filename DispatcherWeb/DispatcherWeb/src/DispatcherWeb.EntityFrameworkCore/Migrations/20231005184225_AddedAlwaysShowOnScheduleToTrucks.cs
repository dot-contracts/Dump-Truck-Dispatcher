using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedAlwaysShowOnScheduleToTrucks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AlwaysShowOnSchedule",
                table: "Truck",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                update t set AlwaysShowOnSchedule = 1
                from Truck t
                left join LeaseHaulerTruck lht on lht.TruckId = t.Id
                where lht.Id is null
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AlwaysShowOnSchedule",
                table: "Truck");
        }
    }
}
