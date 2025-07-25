using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class CreateIndexInOrderTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            const string sql = @"
				IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Order_DateTime_Shift_IsPending' AND object_id = OBJECT_ID('[Order]'))
				BEGIN
					CREATE NONCLUSTERED INDEX [IX_Order_DateTime_Shift_IsPending]
					ON [dbo].[Order] ([DateTime],[Shift],[IsPending])
					INCLUDE ([Id],[CustomerId],[LocationId],[IsDeleted],[TenantId])
				END";
            migrationBuilder.Sql(sql);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            const string sql = @"
				IF EXISTS(SELECT * FROM sys.indexes WHERE name = 'IX_Order_DateTime_Shift_IsPending' AND object_id = OBJECT_ID('[Order]'))
				BEGIN
					DROP INDEX [IX_Order_DateTime_Shift_IsPending] ON [Order]
				END";
            migrationBuilder.Sql(sql);
        }
    }
}
