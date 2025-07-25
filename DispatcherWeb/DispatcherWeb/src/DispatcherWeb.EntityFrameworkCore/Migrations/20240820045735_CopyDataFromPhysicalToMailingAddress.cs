using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class CopyDataFromPhysicalToMailingAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE LH
                SET LH.MailingAddress1 = LH.StreetAddress1, 
                LH.MailingAddress2 = LH.StreetAddress2, 
                LH.MailingCity = City, 
                LH.MailingState = [State],
                LH.MailingCountryCode = CountryCode,
                LH.MailingZipCode = ZipCode
                FROM LeaseHauler LH");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"UPDATE LH
                SET LH.MailingAddress1 = NULL, 
                LH.MailingAddress2 = NULL, 
                LH.MailingCity = NULL, 
                LH.MailingState = NULL,
                LH.MailingCountryCode = NULL,
                LH.MailingZipCode = NULL
                FROM LeaseHauler LH");
        }
    }
}
