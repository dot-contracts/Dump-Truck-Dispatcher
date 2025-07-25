using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedJobTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Job",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    LastModificationTime = table.Column<DateTime>(nullable: true),
                    LastModifierUserId = table.Column<long>(nullable: true),
                    IsDeleted = table.Column<bool>(nullable: false),
                    DeleterUserId = table.Column<long>(nullable: true),
                    DeletionTime = table.Column<DateTime>(nullable: true),
                    TenantId = table.Column<int>(nullable: false),
                    JobDescription = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Job", x => x.Id);
                });


            migrationBuilder.Sql(@"
insert into Job (TenantId, JobDescription, CreationTime, IsDeleted)
select Id, 'Drive Truck', getdate(), 0 from AbpTenants where IsDeleted = 0
");

            migrationBuilder.Sql(@"
update et set JobId = j.Id
from EmployeeTime et
left join Job j on j.TenantId = et.TenantId
where j.Id is not null
");

            migrationBuilder.Sql(@"
insert into AbpSettings (TenantId, Name, [Value], CreationTime)
select t.Id, 'App.General.TimeTrackingDefaultJobId', j.Id, getDate()
from AbpTenants t 
inner join Job j on j.TenantId = t.Id
");

            migrationBuilder.Sql(@"
insert into Job (TenantId, JobDescription, CreationTime, IsDeleted)
select Id, 'Training', getdate(), 0 from AbpTenants where IsDeleted = 0
");

            migrationBuilder.Sql(@"
insert into Job (TenantId, JobDescription, CreationTime, IsDeleted)
select Id, 'Vacation', getdate(), 0 from AbpTenants where IsDeleted = 0
");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTime_JobId",
                table: "EmployeeTime",
                column: "JobId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_Job_JobId",
                table: "EmployeeTime",
                column: "JobId",
                principalTable: "Job",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_Job_JobId",
                table: "EmployeeTime");

            migrationBuilder.DropTable(
                name: "Job");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTime_JobId",
                table: "EmployeeTime");
        }
    }
}
