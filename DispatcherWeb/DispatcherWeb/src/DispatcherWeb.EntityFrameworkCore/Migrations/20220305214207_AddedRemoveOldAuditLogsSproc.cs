using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedRemoveOldAuditLogsSproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS RemoveOldAuditLogs");

            migrationBuilder.Sql(@"

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE  RemoveOldAuditLogs
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FirstId int = 0;
    Set @FirstId = (SELECT top(1) Id FROM AbpAuditLogs WHERE ExecutionTime > DateAdd(Day, -60, GetDate()));
    --Print CAST(@FirstId as NVARCHAR(10))
   
    deleteMore:
    WAITFOR DELAY '00:00:01'
    DELETE TOP(1000) FROM AbpAuditLogs WHERE Id < @FirstId
    IF @@ROWCOUNT != 0
    goto deleteMore

    select @FirstId as [FirstId]
END
GO
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS RemoveOldAuditLogs");
        }
    }
}
