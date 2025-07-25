using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedProductionPayForAllDriversFromProductionPayTenants : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
	delete from EmployeeTimeClassification where IsDeleted = 1
");
            migrationBuilder.Sql(@"
Insert into EmployeeTimeClassification (DriverId, TimeClassificationId, TenantId, CreationTime, IsDeleted, PayRate, IsDefault)
	select distinct d.Id, productionPay.Id, d.TenantId, getDate(), 0, 0, 0
from Driver d
left join AbpSettings s on s.TenantId = d.TenantId and s.Name = 'App.General.AllowAddingTickets' and s.Value = 'true'
left join Job productionPay on productionPay.IsProductionBased = 1 and productionPay.TenantId = d.TenantId
left join EmployeeTimeClassification e on e.DriverId = d.Id and e.TimeClassificationId = productionPay.Id
left join AbpTenants tenant on tenant.Id = d.TenantId
where 
	s.Id is not null
	and (e.Id is null or e.IsDeleted = 1) 
	
	and tenant.IsDeleted = 0 
	and d.IsDeleted = 0
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
