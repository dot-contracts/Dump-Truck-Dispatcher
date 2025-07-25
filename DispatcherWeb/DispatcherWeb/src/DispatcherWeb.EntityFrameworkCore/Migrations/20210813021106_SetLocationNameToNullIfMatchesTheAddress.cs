using Microsoft.EntityFrameworkCore.Migrations;

namespace DispatcherWeb.Migrations
{
    public partial class SetLocationNameToNullIfMatchesTheAddress : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
Update Location set Name = null 
where Replace(Name, ',', '') = Replace(StreetAddress, ',', '')
or Replace(Name, ',', '') = Replace(StreetAddress, ',', '') + ' ' + Replace(City, ',', '')
or Replace(Name, ',', '') = Replace(StreetAddress, ',', '') + ' ' + Replace(City, ',', '') + ' ' + Replace(State, ',', '')
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
