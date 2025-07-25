using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class NormalizedPkNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //Rename primary keys that match "PK_dbo.*" into "PK_*" to match the EF model and most of the other existing primary keys

            migrationBuilder.Sql(@"
                -- Step 1: Retrieve the primary keys to rename
                DECLARE @RenameCommands TABLE (OldName NVARCHAR(128), NewName NVARCHAR(128), TableName NVARCHAR(128));

                INSERT INTO @RenameCommands (OldName, NewName, TableName)
                SELECT 
                    tc.CONSTRAINT_NAME COLLATE SQL_Latin1_General_CP1_CI_AS AS OldName,
                    REPLACE(tc.CONSTRAINT_NAME COLLATE SQL_Latin1_General_CP1_CI_AS, 'PK_dbo.', 'PK_') AS NewName,
                    tc.TABLE_NAME COLLATE SQL_Latin1_General_CP1_CI_AS AS TableName
                FROM 
                    INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                WHERE 
                    tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    AND tc.CONSTRAINT_NAME LIKE 'PK_dbo.%';

                -- Step 2: Generate dynamic SQL to rename the primary keys
                DECLARE @sql NVARCHAR(MAX) = '';

                SELECT @sql = @sql + 'EXEC sp_rename ''' + QUOTENAME(tc.TABLE_SCHEMA COLLATE SQL_Latin1_General_CP1_CI_AS) + '.' + QUOTENAME(tc.TABLE_NAME COLLATE SQL_Latin1_General_CP1_CI_AS) + '.' + QUOTENAME(OldName COLLATE SQL_Latin1_General_CP1_CI_AS) + ''', ''' + NewName + ''', ''INDEX'';' + CHAR(13)
                FROM @RenameCommands AS rc
                JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
                ON rc.OldName = tc.CONSTRAINT_NAME COLLATE SQL_Latin1_General_CP1_CI_AS;

                -- Step 3: Execute the dynamic SQL
                EXEC sp_executesql @sql;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
