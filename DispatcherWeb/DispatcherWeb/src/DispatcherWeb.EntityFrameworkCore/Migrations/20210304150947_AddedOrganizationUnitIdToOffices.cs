using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedOrganizationUnitIdToOffices : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "OrganizationUnitId",
                table: "Office",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Office_OrganizationUnitId",
                table: "Office",
                column: "OrganizationUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Office_AbpOrganizationUnits_OrganizationUnitId",
                table: "Office",
                column: "OrganizationUnitId",
                principalTable: "AbpOrganizationUnits",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            //            migrationBuilder.Sql(@"
            //ALTER TABLE [dbo].[AbpOrganizationUnits]
            //ADD [TempOriginalOfficeId] int NULL 
            //");

            //            migrationBuilder.Sql(@"
            //delete from [dbo].[AbpUserOrganizationUnits]
            //");

            //            migrationBuilder.Sql(@"
            //delete from [dbo].[AbpOrganizationUnits]
            //");

            //            //[Code] is not for storing OfficeId reference, but we need to populate it with a unique 0-padded value and OfficeId would work fine for that
            //            migrationBuilder.Sql(@"
            //Insert into [dbo].[AbpOrganizationUnits] (TenantId, TempOriginalOfficeId, Code, DisplayName, IsDeleted, CreationTime)
            //select TenantId, Id, RIGHT('00000'+CAST(Id AS VARCHAR(5)),5), Name, 0, getdate() from Office where IsDeleted = 0
            //");

            //            migrationBuilder.Sql(@"
            //update o set o.OrganizationUnitId = ou.Id
            //from Office o
            //inner join AbpOrganizationUnits ou on ou.TempOriginalOfficeId = o.Id
            //where o.IsDeleted = 0
            //");

            //            migrationBuilder.Sql(@"
            //Insert into [dbo].[AbpUserOrganizationUnits] (TenantId, UserId, OrganizationUnitId, IsDeleted, CreationTime)
            //select u.TenantId, u.Id, ou.Id, 0, getdate() 
            //from AbpUsers u 
            //inner join AbpOrganizationUnits ou on ou.TempOriginalOfficeId = u.OfficeId
            //where u.IsDeleted = 0 and u.OfficeId is not null
            //");

            //            migrationBuilder.Sql(@"
            //ALTER TABLE [dbo].[AbpOrganizationUnits]
            //DROP COLUMN [TempOriginalOfficeId]
            //");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Office_AbpOrganizationUnits_OrganizationUnitId",
                table: "Office");

            migrationBuilder.DropIndex(
                name: "IX_Office_OrganizationUnitId",
                table: "Office");

            migrationBuilder.DropColumn(
                name: "OrganizationUnitId",
                table: "Office");
        }
    }
}
