using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddUnitOfMeasureMiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
BEGIN
   IF NOT EXISTS (SELECT * FROM UnitOfMeasure 
                   WHERE Name = 'Miles')
   BEGIN
       insert into UnitOfMeasure (Name, TenantId, CreationTime, IsDeleted)
       select 'Miles', Id, getdate(), 0 from AbpTenants
   END
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
