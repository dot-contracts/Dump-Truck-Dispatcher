using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class PopulatedMissingUomBaseId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE u
                SET u.UnitOfMeasureBaseId = b.Id
                FROM UnitOfMeasure u
                INNER JOIN UnitOfMeasureBase b ON u.Name = b.Name
                WHERE u.UnitOfMeasureBaseId IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
