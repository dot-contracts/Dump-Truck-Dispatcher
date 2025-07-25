using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class AddedCreationTimeIndexToAbpUserNotifications : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                CREATE NONCLUSTERED INDEX [IX_AbpUserNotifications_UserId_CreationTime] ON [dbo].[AbpUserNotifications]
                (
                	[UserId] ASC,
                	[CreationTime] ASC
                )WITH (STATISTICS_NORECOMPUTE = OFF, DROP_EXISTING = OFF, ONLINE = OFF, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DROP INDEX [IX_AbpUserNotifications_UserId_CreationTime] ON [dbo].[AbpUserNotifications]
            ");
        }
    }
}
