using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    /// <inheritdoc />
    public partial class AddedStringLengthToStringColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE WorkOrderPicture SET FileName = LEFT(FileName, 100) WHERE LEN(FileName) > 100");
            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "WorkOrderPicture",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE WialonDeviceType SET Name = LEFT(Name, 127) WHERE LEN(Name) > 127");
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WialonDeviceType",
                type: "nvarchar(127)",
                maxLength: 127,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Truck SET DtdTrackerPassword = LEFT(DtdTrackerPassword, 100) WHERE LEN(DtdTrackerPassword) > 100");
            migrationBuilder.AlterColumn<string>(
                name: "DtdTrackerPassword",
                table: "Truck",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Quote SET Notes = LEFT(Notes, 4000) WHERE LEN(Notes) > 4000");
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Quote",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Quote SET Directions = LEFT(Directions, 1000) WHERE LEN(Directions) > 1000");
            migrationBuilder.AlterColumn<string>(
                name: "Directions",
                table: "Quote",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Project SET Notes = LEFT(Notes, 4000) WHERE LEN(Notes) > 4000");
            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Project",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Project SET Directions = LEFT(Directions, 1000) WHERE LEN(Directions) > 1000");
            migrationBuilder.AlterColumn<string>(
                name: "Directions",
                table: "Project",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Payment SET TransactionType = LEFT(TransactionType, 255) WHERE LEN(TransactionType) > 255");
            migrationBuilder.AlterColumn<string>(
                name: "TransactionType",
                table: "Payment",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Payment SET BatchSummaryId = LEFT(BatchSummaryId, 1000) WHERE LEN(BatchSummaryId) > 1000");
            migrationBuilder.AlterColumn<string>(
                name: "BatchSummaryId",
                table: "Payment",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE [Order] SET EncryptedInternalNotes = LEFT(EncryptedInternalNotes, 1000) WHERE LEN(EncryptedInternalNotes) > 1000");
            migrationBuilder.AlterColumn<string>(
                name: "EncryptedInternalNotes",
                table: "Order",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE LuckStoneLocation SET Site = LEFT(Site, 100) WHERE LEN(Site) > 100");
            migrationBuilder.AlterColumn<string>(
                name: "Site",
                table: "LuckStoneLocation",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Location SET PlaceId = LEFT(PlaceId, 255) WHERE LEN(PlaceId) > 255");
            migrationBuilder.AlterColumn<string>(
                name: "PlaceId",
                table: "Location",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Load SET SignatureName = LEFT(SignatureName, 100) WHERE LEN(SignatureName) > 100");
            migrationBuilder.AlterColumn<string>(
                name: "SignatureName",
                table: "Load",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Invoice SET QuickbooksInvoiceId = LEFT(QuickbooksInvoiceId, 30) WHERE LEN(QuickbooksInvoiceId) > 30");
            migrationBuilder.AlterColumn<string>(
                name: "QuickbooksInvoiceId",
                table: "Invoice",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE HostEmailRoles SET RoleName = LEFT(RoleName, 255) WHERE LEN(RoleName) > 255");
            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "HostEmailRoles",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE DriverApplicationDevice SET Useragent = LEFT(Useragent, 500) WHERE LEN(Useragent) > 500");
            migrationBuilder.AlterColumn<string>(
                name: "Useragent",
                table: "DriverApplicationDevice",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE CustomerNotificationRole SET RoleName = LEFT(RoleName, 500) WHERE LEN(RoleName) > 500");
            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "CustomerNotificationRole",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Customer SET CreditCardZipCode = LEFT(CreditCardZipCode, 50) WHERE LEN(CreditCardZipCode) > 50");
            migrationBuilder.AlterColumn<string>(
                name: "CreditCardZipCode",
                table: "Customer",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Customer SET CreditCardToken = LEFT(CreditCardToken, 500) WHERE LEN(CreditCardToken) > 500");
            migrationBuilder.AlterColumn<string>(
                name: "CreditCardToken",
                table: "Customer",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE Customer SET CreditCardStreetAddress = LEFT(CreditCardStreetAddress, 500) WHERE LEN(CreditCardStreetAddress) > 500");
            migrationBuilder.AlterColumn<string>(
                name: "CreditCardStreetAddress",
                table: "Customer",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE CannedText SET Text = LEFT(Text, 1000) WHERE LEN(Text) > 1000");
            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "CannedText",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE BackgroundJobHistory SET Details = LEFT(Details, 500) WHERE LEN(Details) > 500");
            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "BackgroundJobHistory",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppSubscriptionPaymentsExtensionData SET Value = LEFT(Value, 500) WHERE LEN(Value) > 500");
            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AppSubscriptionPaymentsExtensionData",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppSubscriptionPayments SET SuccessUrl = LEFT(SuccessUrl, 500) WHERE LEN(SuccessUrl) > 500");
            migrationBuilder.AlterColumn<string>(
                name: "SuccessUrl",
                table: "AppSubscriptionPayments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppSubscriptionPayments SET InvoiceNo = LEFT(InvoiceNo, 30) WHERE LEN(InvoiceNo) > 30");
            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "AppSubscriptionPayments",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppSubscriptionPayments SET ErrorUrl = LEFT(ErrorUrl, 500) WHERE LEN(ErrorUrl) > 500");
            migrationBuilder.AlterColumn<string>(
                name: "ErrorUrl",
                table: "AppSubscriptionPayments",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppSubscriptionPayments SET Description = LEFT(Description, 2000) WHERE LEN(Description) > 2000");
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AppSubscriptionPayments",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppInvoices SET TenantTaxNo = LEFT(TenantTaxNo, 50) WHERE LEN(TenantTaxNo) > 50");
            migrationBuilder.AlterColumn<string>(
                name: "TenantTaxNo",
                table: "AppInvoices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppInvoices SET TenantLegalName = LEFT(TenantLegalName, 255) WHERE LEN(TenantLegalName) > 255");
            migrationBuilder.AlterColumn<string>(
                name: "TenantLegalName",
                table: "AppInvoices",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppInvoices SET TenantAddress = LEFT(TenantAddress, 500) WHERE LEN(TenantAddress) > 500");
            migrationBuilder.AlterColumn<string>(
                name: "TenantAddress",
                table: "AppInvoices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppInvoices SET InvoiceNo = LEFT(InvoiceNo, 30) WHERE LEN(InvoiceNo) > 30");
            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "AppInvoices",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppFriendships SET FriendTenancyName = LEFT(FriendTenancyName, 255) WHERE LEN(FriendTenancyName) > 255");
            migrationBuilder.AlterColumn<string>(
                name: "FriendTenancyName",
                table: "AppFriendships",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AppBinaryObjects SET Description = LEFT(Description, 255) WHERE LEN(Description) > 255");
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AppBinaryObjects",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AbpUsers SET SignInToken = LEFT(SignInToken, 255) WHERE LEN(SignInToken) > 255");
            migrationBuilder.AlterColumn<string>(
                name: "SignInToken",
                table: "AbpUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.Sql("UPDATE AbpUsers SET GoogleAuthenticatorKey = LEFT(GoogleAuthenticatorKey, 255) WHERE LEN(GoogleAuthenticatorKey) > 255");
            migrationBuilder.AlterColumn<string>(
                name: "GoogleAuthenticatorKey",
                table: "AbpUsers",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "WorkOrderPicture",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "WialonDeviceType",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(127)",
                oldMaxLength: 127,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DtdTrackerPassword",
                table: "Truck",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Quote",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Directions",
                table: "Quote",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                table: "Project",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Directions",
                table: "Project",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TransactionType",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BatchSummaryId",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EncryptedInternalNotes",
                table: "Order",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Site",
                table: "LuckStoneLocation",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PlaceId",
                table: "Location",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SignatureName",
                table: "Load",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "QuickbooksInvoiceId",
                table: "Invoice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "HostEmailRoles",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Useragent",
                table: "DriverApplicationDevice",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RoleName",
                table: "CustomerNotificationRole",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreditCardZipCode",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreditCardToken",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CreditCardStreetAddress",
                table: "Customer",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Text",
                table: "CannedText",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Details",
                table: "BackgroundJobHistory",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Value",
                table: "AppSubscriptionPaymentsExtensionData",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SuccessUrl",
                table: "AppSubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "AppSubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ErrorUrl",
                table: "AppSubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AppSubscriptionPayments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantTaxNo",
                table: "AppInvoices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantLegalName",
                table: "AppInvoices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "TenantAddress",
                table: "AppInvoices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "InvoiceNo",
                table: "AppInvoices",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FriendTenancyName",
                table: "AppFriendships",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "AppBinaryObjects",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "SignInToken",
                table: "AbpUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "GoogleAuthenticatorKey",
                table: "AbpUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(255)",
                oldMaxLength: 255,
                oldNullable: true);
        }
    }
}
