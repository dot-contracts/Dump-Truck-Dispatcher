using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToNet9SetCompatLevelAndFixAssemblyName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER DATABASE CURRENT SET COMPATIBILITY_LEVEL = 150;
            ", suppressTransaction: true);

            migrationBuilder.Sql(@"
                UPDATE AbpNotifications
                SET DataTypeName = REPLACE(DataTypeName, ', Abp, Version', ', Dtd.Abp, Version')
                WHERE DataTypeName LIKE '%, Abp, Version%';
            ");
            migrationBuilder.Sql(@"
                UPDATE AbpTenantNotifications
                SET DataTypeName = REPLACE(DataTypeName, ', Abp, Version', ', Dtd.Abp, Version')
                WHERE DataTypeName LIKE '%, Abp, Version%';
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
