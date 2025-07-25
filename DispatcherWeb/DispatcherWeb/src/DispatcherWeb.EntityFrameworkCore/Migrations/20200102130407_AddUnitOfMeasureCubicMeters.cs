using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddUnitOfMeasureCubicMeters : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
BEGIN
   IF NOT EXISTS (SELECT * FROM UnitOfMeasure 
                   WHERE Name = 'Cubic Meters')
   BEGIN
       insert into UnitOfMeasure (Name, TenantId, CreationTime, IsDeleted)
       select 'Cubic Meters', Id, getdate(), 0 from AbpTenants
   END
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
