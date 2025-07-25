using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedMatchingFiltersToFewEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "UserDailyHistory",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "QuoteEmails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "OrderEmails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TenantId",
                table: "InvoiceEmails",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HostEmailTenants",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HostEmailRoles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HostEmailReceivers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "HostEmailEditions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "DismissedCustomerNotification",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomerNotificationTenant",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomerNotificationRole",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CustomerNotificationEdition",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(@"
                update target set TenantId = email.TenantId
                from InvoiceEmails target
                inner join TrackableEmails email on email.Id = target.EmailId
            ");

            migrationBuilder.Sql(@"
                update target set TenantId = email.TenantId
                from OrderEmails target
                inner join TrackableEmails email on email.Id = target.EmailId
            ");

            migrationBuilder.Sql(@"
                update target set TenantId = email.TenantId
                from QuoteEmails target
                inner join TrackableEmails email on email.Id = target.EmailId
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "UserDailyHistory");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "QuoteEmails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "OrderEmails");

            migrationBuilder.DropColumn(
                name: "TenantId",
                table: "InvoiceEmails");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HostEmailTenants");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HostEmailRoles");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HostEmailReceivers");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "HostEmailEditions");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "DismissedCustomerNotification");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomerNotificationTenant");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomerNotificationRole");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CustomerNotificationEdition");
        }
    }
}
