using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class RenamedJobToTimeClassificationStep2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //This is supposed to be empty. We are updating the reference in the model snapshot from DispatcherWeb.Jobs.Job to DispatcherWeb.TimeClassifications.TimeClassification
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
