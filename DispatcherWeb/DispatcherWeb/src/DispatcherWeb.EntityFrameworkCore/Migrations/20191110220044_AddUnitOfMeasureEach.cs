using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddUnitOfMeasureEach : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
BEGIN
   IF NOT EXISTS (SELECT * FROM UnitOfMeasure 
                   WHERE Name = 'Each')
   BEGIN
       insert into UnitOfMeasure (Name, TenantId, CreationTime, IsDeleted)
       select 'Each', Id, getdate(), 0 from AbpTenants
   END
END
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
