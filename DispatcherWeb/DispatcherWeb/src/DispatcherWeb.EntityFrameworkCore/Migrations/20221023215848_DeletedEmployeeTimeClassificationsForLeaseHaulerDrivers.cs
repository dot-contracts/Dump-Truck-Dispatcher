using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class DeletedEmployeeTimeClassificationsForLeaseHaulerDrivers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                update e set IsDeleted = 1, DeletionTime = GetDate()
                from EmployeeTimeClassification e
                inner join Driver d on d.Id = e.DriverId
                inner join LeaseHaulerDriver lhd on lhd.DriverId = d.Id
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
