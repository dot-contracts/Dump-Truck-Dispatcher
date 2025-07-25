using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedCounterSalesPermissionToUsersWithMatchingSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                insert into AbpPermissions
                (TenantId, Name, IsGranted, CreationTime, UserId, Discriminator)
                select
                TenantId, 'Pages.CounterSales', 1, GetDate(), UserId, 'UserPermissionSetting'
                from AbpSettings where Name = 'App.DispatchingAndMessaging.AllowCounterSalesForUser' and Value = 'true'
                and TenantId is not null and UserId is not null
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
