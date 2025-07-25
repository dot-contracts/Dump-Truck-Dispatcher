using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedEmployeeTimeClassificationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "DefaultRate",
                table: "Job",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsProductionBased",
                table: "Job",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EmployeeTimeClassificationId",
                table: "EmployeeTime",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeTimeClassification",
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
                    DriverId = table.Column<int>(nullable: false),
                    TimeClassificationId = table.Column<int>(nullable: false),
                    IsDefault = table.Column<bool>(nullable: false),
                    PayRate = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTimeClassification", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTimeClassification_Driver_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Driver",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeTimeClassification_Job_TimeClassificationId",
                        column: x => x.TimeClassificationId,
                        principalTable: "Job",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTime_EmployeeTimeClassificationId",
                table: "EmployeeTime",
                column: "EmployeeTimeClassificationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeClassification_DriverId",
                table: "EmployeeTimeClassification",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTimeClassification_TimeClassificationId",
                table: "EmployeeTimeClassification",
                column: "TimeClassificationId");

            migrationBuilder.AddForeignKey(
                name: "FK_EmployeeTime_EmployeeTimeClassification_EmployeeTimeClassificationId",
                table: "EmployeeTime",
                column: "EmployeeTimeClassificationId",
                principalTable: "EmployeeTimeClassification",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.Sql(@"
insert into Job (TenantId, JobDescription, IsProductionBased, CreationTime, IsDeleted)
select Id, 'Production Pay', 1, getdate(), 0 from AbpTenants where IsDeleted = 0
");

            //restore deleted Drive Truck jobs
            migrationBuilder.Sql(@"
insert into Job (TenantId, JobDescription, CreationTime, IsDeleted)
select t.Id, 'Drive Truck', getdate(), 0
from AbpTenants t
left join Job c on c.JobDescription = 'Drive Truck' and c.TenantId = t.Id
where t.IsDeleted = 0 and c.Id is null
");

            migrationBuilder.Sql(@"
insert into EmployeeTimeClassification (TenantId, DriverId, TimeClassificationId, IsDefault, PayRate, CreationTime, IsDeleted)
select d.TenantId, d.Id, (select top 1 c.Id from Job c where c.TenantId = d.TenantId and c.JobDescription = 'Drive Truck'), 1, 0, getdate(), 0 
from Driver d
left join AbpTenants t on t.Id = d.TenantId
where d.IsDeleted = 0 and t.IsDeleted = 0
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EmployeeTime_EmployeeTimeClassification_EmployeeTimeClassificationId",
                table: "EmployeeTime");

            migrationBuilder.DropTable(
                name: "EmployeeTimeClassification");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeTime_EmployeeTimeClassificationId",
                table: "EmployeeTime");

            migrationBuilder.DropColumn(
                name: "DefaultRate",
                table: "Job");

            migrationBuilder.DropColumn(
                name: "IsProductionBased",
                table: "Job");

            migrationBuilder.DropColumn(
                name: "EmployeeTimeClassificationId",
                table: "EmployeeTime");
        }
    }
}
