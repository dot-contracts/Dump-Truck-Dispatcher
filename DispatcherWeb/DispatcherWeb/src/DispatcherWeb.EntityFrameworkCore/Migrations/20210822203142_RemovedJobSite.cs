using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class RemovedJobSite : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //replace Rental with FreightOnly because rental designation was deleted
            migrationBuilder.Sql(@"
update LeaseHaulerAgreementService set Designation = 1 where Designation = 4;
update OrderLine set Designation = 1 where Designation = 4;
update ReceiptLine set Designation = 1 where Designation = 4;
update ProjectService set Designation = 1 where Designation = 4;
update QuoteService set Designation = 1 where Designation = 4;
update OfficeServicePrice set Designation = 1 where Designation = 4;
");

            migrationBuilder.Sql(@"
insert into LocationCategory (TenantId, [Name], PredefinedLocationCategoryKind, CreationTime, IsDeleted)
select Id, 'Unknown Load Site', 11, getdate(), 0 from AbpTenants where IsDeleted = 0

insert into LocationCategory (TenantId, [Name], PredefinedLocationCategoryKind, CreationTime, IsDeleted)
select Id, 'Unknown Delivery Site', 12, getdate(), 0 from AbpTenants where IsDeleted = 0
");

            //add Locations with Name set to match Order/Receipt/Project/Quote JobSite field
            migrationBuilder.Sql(@"
insert into [Location] (Name, CreationTime, IsDeleted, TenantId, IsActive)
select JobSite, getDate(), 0, TenantId, 1 from (
select distinct o.JobSite, o.TenantId from [Order] o
where o.JobSite is not null and o.JobSite != '' and o.IsDeleted = 0
and not exists (select * from Location l where l.Name = o.JobSite and l.TenantId = o.TenantId and l.IsDeleted = 0)
and exists (
    select * from OrderLine ol
    left join Location loadAt on loadAt.Id = ol.LoadAtId
    left join Location deliverTo on deliverTo.Id = ol.DeliverToId
    where ol.IsDeleted = 0 and ol.OrderId = o.Id and (loadAt.PredefinedLocationKind = 1 or deliverTo.PredefinedLocationKind = 1)
    )
) b
");
            migrationBuilder.Sql(@"
insert into [Location] (Name, CreationTime, IsDeleted, TenantId, IsActive)
select JobSite, getDate(), 0, TenantId, 1 from (
select distinct o.JobSite, o.TenantId from Receipt o
where o.JobSite is not null and o.JobSite != '' and o.IsDeleted = 0
and not exists (select * from Location l where l.Name = o.JobSite and l.TenantId = o.TenantId and l.IsDeleted = 0)
and exists (
    select * from ReceiptLine ol
    left join Location loadAt on loadAt.Id = ol.LoadAtId
    left join Location deliverTo on deliverTo.Id = ol.DeliverToId
    where ol.IsDeleted = 0 and ol.ReceiptId = o.Id and (loadAt.PredefinedLocationKind = 1 or deliverTo.PredefinedLocationKind = 1)
    )
) b
");
            migrationBuilder.Sql(@"
insert into [Location] (Name, CreationTime, IsDeleted, TenantId, IsActive)
select JobSite, getDate(), 0, TenantId, 1 from (
select distinct o.JobSite, o.TenantId from Project o
where o.JobSite is not null and o.JobSite != '' and o.IsDeleted = 0
and not exists (select * from Location l where l.Name = o.JobSite and l.TenantId = o.TenantId and l.IsDeleted = 0)
and exists (
    select * from ProjectService ol
    left join Location loadAt on loadAt.Id = ol.LoadAtId
    left join Location deliverTo on deliverTo.Id = ol.DeliverToId
    where ol.IsDeleted = 0 and ol.ProjectId = o.Id and (loadAt.PredefinedLocationKind = 1 or deliverTo.PredefinedLocationKind = 1)
    )
) b
");
            migrationBuilder.Sql(@"
insert into [Location] (Name, CreationTime, IsDeleted, TenantId, IsActive)
select JobSite, getDate(), 0, TenantId, 1 from (
select distinct o.JobSite, o.TenantId from Quote o
where o.JobSite is not null and o.JobSite != '' and o.IsDeleted = 0
and not exists (select * from Location l where l.Name = o.JobSite and l.TenantId = o.TenantId and l.IsDeleted = 0)
and exists (
    select * from QuoteService ol
    left join Location loadAt on loadAt.Id = ol.LoadAtId
    left join Location deliverTo on deliverTo.Id = ol.DeliverToId
    where ol.IsDeleted = 0 and ol.QuoteId = o.Id and (loadAt.PredefinedLocationKind = 1 or deliverTo.PredefinedLocationKind = 1)
    )
) b
");

            //update OrderLine, ReceiptLine, ProjectService, QuoteService to set LoadAtId and DeliverToId to this new location id
            migrationBuilder.Sql(@"
update ol set ol.LoadAtId = jobSite.Id
from OrderLine ol
left join Location loadAt on loadAt.Id = ol.LoadAtId
--left join Location deliverTo on deliverTo.Id = ol.DeliverToId
left join [Order] o on o.Id = ol.OrderId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where ol.IsDeleted = 0 and loadAt.PredefinedLocationKind = 1 and jobSite.Id is not null
");
            migrationBuilder.Sql(@"
update ol set ol.DeliverToId = jobSite.Id
from OrderLine ol
--left join Location loadAt on loadAt.Id = ol.LoadAtId
left join Location deliverTo on deliverTo.Id = ol.DeliverToId
left join [Order] o on o.Id = ol.OrderId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where ol.IsDeleted = 0 and deliverTo.PredefinedLocationKind = 1 and jobSite.Id is not null
");

            migrationBuilder.Sql(@"
update ol set ol.LoadAtId = jobSite.Id
from ReceiptLine ol
left join Location loadAt on loadAt.Id = ol.LoadAtId
left join [Receipt] o on o.Id = ol.ReceiptId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where ol.IsDeleted = 0 and loadAt.PredefinedLocationKind = 1 and jobSite.Id is not null
");
            migrationBuilder.Sql(@"
update ol set ol.DeliverToId = jobSite.Id
from ReceiptLine ol
left join Location deliverTo on deliverTo.Id = ol.DeliverToId
left join [Receipt] o on o.Id = ol.ReceiptId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where ol.IsDeleted = 0 and deliverTo.PredefinedLocationKind = 1 and jobSite.Id is not null
");

            migrationBuilder.Sql(@"
update ol set ol.LoadAtId = jobSite.Id
from ProjectService ol
left join Location loadAt on loadAt.Id = ol.LoadAtId
left join [Project] o on o.Id = ol.ProjectId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where ol.IsDeleted = 0 and loadAt.PredefinedLocationKind = 1 and jobSite.Id is not null
");
            migrationBuilder.Sql(@"
update ol set ol.DeliverToId = jobSite.Id
from ProjectService ol
left join Location deliverTo on deliverTo.Id = ol.DeliverToId
left join [Project] o on o.Id = ol.ProjectId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where ol.IsDeleted = 0 and deliverTo.PredefinedLocationKind = 1 and jobSite.Id is not null
");

            migrationBuilder.Sql(@"
update ol set ol.LoadAtId = jobSite.Id
from QuoteService ol
left join Location loadAt on loadAt.Id = ol.LoadAtId
left join [Quote] o on o.Id = ol.QuoteId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where ol.IsDeleted = 0 and loadAt.PredefinedLocationKind = 1 and jobSite.Id is not null
");
            migrationBuilder.Sql(@"
update ol set ol.DeliverToId = jobSite.Id
from QuoteService ol
left join Location deliverTo on deliverTo.Id = ol.DeliverToId
left join [Quote] o on o.Id = ol.QuoteId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where ol.IsDeleted = 0 and deliverTo.PredefinedLocationKind = 1 and jobSite.Id is not null
");

            migrationBuilder.Sql(@"
update t set t.LoadAtId = jobSite.Id
from Ticket t
left join OrderLine ol on ol.Id = t.OrderLineId
left join Location loadAt on loadAt.Id = t.LoadAtId
--left join Location deliverTo on deliverTo.Id = t.DeliverToId
left join [Order] o on o.Id = ol.OrderId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where t.IsDeleted = 0 and ol.IsDeleted = 0 and loadAt.PredefinedLocationKind = 1 and jobSite.Id is not null
");
            migrationBuilder.Sql(@"
update t set t.DeliverToId = jobSite.Id
from Ticket t
left join OrderLine ol on ol.Id = t.OrderLineId
--left join Location loadAt on loadAt.Id = t.LoadAtId
left join Location deliverTo on deliverTo.Id = t.DeliverToId
left join [Order] o on o.Id = ol.OrderId
left join Location jobSite on jobSite.Name = o.JobSite and jobSite.TenantId = o.TenantId
where t.IsDeleted = 0 and ol.IsDeleted = 0 and deliverTo.PredefinedLocationKind = 1 and jobSite.Id is not null
");

            //remove LcoationId where not possible to match to job site
            migrationBuilder.Sql(@"
update o set o.LocationId = null
from LeaseHaulerAgreementService o
left join Location l on l.Id = o.LocationId
where l.PredefinedLocationKind = 1 
");
            //e.g. Order.JobSite is null or empty, Order is deleted, OrderLine is deleted
            migrationBuilder.Sql(@"
update ol set ol.LoadAtId = null
from OrderLine ol
left join Location loadAt on loadAt.Id = ol.LoadAtId
where loadAt.PredefinedLocationKind = 1
");
            migrationBuilder.Sql(@"
--select ol.IsDeleted, o.IsDeleted, o.JobSite  
update ol set ol.DeliverToId = null
from OrderLine ol
--left join [Order] o on o.Id = ol.OrderId
left join Location deliverTo on deliverTo.Id = ol.DeliverToId
where deliverTo.PredefinedLocationKind = 1
");

            migrationBuilder.Sql(@"
update ol set ol.LoadAtId = null
from ReceiptLine ol
left join Location loadAt on loadAt.Id = ol.LoadAtId
where loadAt.PredefinedLocationKind = 1
");
            migrationBuilder.Sql(@"
update ol set ol.DeliverToId = null
from ReceiptLine ol
left join Location deliverTo on deliverTo.Id = ol.DeliverToId
where deliverTo.PredefinedLocationKind = 1
");

            migrationBuilder.Sql(@"
update ol set ol.LoadAtId = null
from ProjectService ol
left join Location loadAt on loadAt.Id = ol.LoadAtId
where loadAt.PredefinedLocationKind = 1
");
            migrationBuilder.Sql(@"
update ol set ol.DeliverToId = null
from ProjectService ol
left join Location deliverTo on deliverTo.Id = ol.DeliverToId
where deliverTo.PredefinedLocationKind = 1
");

            migrationBuilder.Sql(@"
update ol set ol.LoadAtId = null
from QuoteService ol
left join Location loadAt on loadAt.Id = ol.LoadAtId
where loadAt.PredefinedLocationKind = 1
");
            migrationBuilder.Sql(@"
update ol set ol.DeliverToId = null
from QuoteService ol
left join Location deliverTo on deliverTo.Id = ol.DeliverToId
where deliverTo.PredefinedLocationKind = 1
");

            migrationBuilder.Sql(@"
update t set t.LoadAtId = null
--select t.OrderLineId, t.IsDeleted, ol.IsDeleted, o.IsDeleted, o.JobSite
from Ticket t
--left join OrderLine ol on ol.Id = t.OrderLineId
--left join [Order] o on o.Id = ol.OrderId
left join Location loadAt on loadAt.Id = t.LoadAtId
where loadAt.PredefinedLocationKind = 1
");
            migrationBuilder.Sql(@"
--select t.OrderLineId, t.IsDeleted, ol.IsDeleted, o.IsDeleted, o.JobSite
update t set t.DeliverToId = null
from Ticket t
--left join OrderLine ol on ol.Id = t.OrderLineId
--left join [Order] o on o.Id = ol.OrderId
left join Location deliverTo on deliverTo.Id = t.DeliverToId
where deliverTo.PredefinedLocationKind = 1
");

            //delete Job Site location
            migrationBuilder.Sql("delete from Location where PredefinedLocationKind = 1");

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
