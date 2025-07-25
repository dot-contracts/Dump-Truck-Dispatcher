using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class AddedRemoveOldDriverAppLogsSproc : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS RemoveOldDriverAppLogs");

            migrationBuilder.Sql(@"

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE RemoveOldDriverAppLogs
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @FirstId int = 0;
    Set @FirstId = (SELECT top(1) Id FROM DriverApplicationLog WHERE DateTime > DateAdd(Day, -14, GetDate()));
    Print CAST(@FirstId as NVARCHAR(10))
    
    deleteMore:
    WAITFOR DELAY '00:00:02'
    DELETE TOP(1000) FROM DriverApplicationLog WHERE Id < @FirstId
    IF @@ROWCOUNT != 0
    goto deleteMore

    select @FirstId as [FirstId]
END
GO

");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP PROCEDURE IF EXISTS RemoveOldDriverAppLogs");
        }
    }
}
