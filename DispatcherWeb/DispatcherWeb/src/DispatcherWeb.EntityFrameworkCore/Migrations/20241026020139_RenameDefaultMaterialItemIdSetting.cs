using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class RenameDefaultMaterialItemIdSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE [AbpSettings]
                SET [Name] = 'App.DispatchingAndMessaging.DefaultMaterialItemId'
                WHERE [Name] = 'App.DispatchingAndMessaging.DefaultServiceId'"
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
