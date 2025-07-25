using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DispatcherWeb.Migrations
{
    public partial class UpdatedSnapshotToMatchWithDev : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropIndex(
            //    name: "IX_AbpSettings_TenantId_Name",
            //    table: "AbpSettings");

            //migrationBuilder.DropColumn(
            //    name: "LastLoginTime",
            //    table: "AbpUsers");

            //migrationBuilder.DropColumn(
            //    name: "LastLoginTime",
            //    table: "AbpUserAccounts");

            //migrationBuilder.RenameColumn(
            //    name: "PaymentId",
            //    table: "AppSubscriptionPayments",
            //    newName: "ExternalPaymentId");

            //migrationBuilder.RenameIndex(
            //    name: "IX_AppSubscriptionPayments_PaymentId_Gateway",
            //    table: "AppSubscriptionPayments",
            //    newName: "IX_AppSubscriptionPayments_ExternalPaymentId_Gateway");

            //migrationBuilder.AddColumn<string>(
            //    name: "Description",
            //    table: "AppSubscriptionPayments",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "EditionPaymentType",
            //    table: "AppSubscriptionPayments",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AddColumn<string>(
            //    name: "ErrorUrl",
            //    table: "AppSubscriptionPayments",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<bool>(
            //    name: "IsRecurring",
            //    table: "AppSubscriptionPayments",
            //    type: "bit",
            //    nullable: false,
            //    defaultValue: false);

            //migrationBuilder.AddColumn<string>(
            //    name: "SuccessUrl",
            //    table: "AppSubscriptionPayments",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "Description",
            //    table: "AppBinaryObjects",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "UserNameOrEmailAddress",
            //    table: "AbpUserLoginAttempts",
            //    type: "nvarchar(256)",
            //    maxLength: 256,
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(255)",
            //    oldMaxLength: 255,
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "ReportsLogoFileType",
            //    table: "AbpTenants",
            //    type: "nvarchar(max)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(64)",
            //    oldMaxLength: 64,
            //    oldNullable: true);

            //migrationBuilder.AddColumn<int>(
            //    name: "SubscriptionPaymentType",
            //    table: "AbpTenants",
            //    type: "int",
            //    nullable: false,
            //    defaultValue: 0);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Value",
            //    table: "AbpSettings",
            //    type: "nvarchar(max)",
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(2000)",
            //    oldMaxLength: 2000,
            //    oldNullable: true);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "ConsumedTime",
            //    table: "AbpPersistedGrants",
            //    type: "datetime2",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "Description",
            //    table: "AbpPersistedGrants",
            //    type: "nvarchar(200)",
            //    maxLength: 200,
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "SessionId",
            //    table: "AbpPersistedGrants",
            //    type: "nvarchar(100)",
            //    maxLength: 100,
            //    nullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "LanguageName",
            //    table: "AbpLanguageTexts",
            //    type: "nvarchar(128)",
            //    maxLength: 128,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(10)",
            //    oldMaxLength: 10);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Name",
            //    table: "AbpLanguages",
            //    type: "nvarchar(128)",
            //    maxLength: 128,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(10)",
            //    oldMaxLength: 10);

            //migrationBuilder.AddColumn<string>(
            //    name: "NewValueHash",
            //    table: "AbpEntityPropertyChanges",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "OriginalValueHash",
            //    table: "AbpEntityPropertyChanges",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.AddColumn<decimal>(
            //    name: "DailyPrice",
            //    table: "AbpEditions",
            //    type: "decimal(18,2)",
            //    nullable: true);

            //migrationBuilder.AddColumn<decimal>(
            //    name: "WeeklyPrice",
            //    table: "AbpEditions",
            //    type: "decimal(18,2)",
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "ExceptionMessage",
            //    table: "AbpAuditLogs",
            //    type: "nvarchar(1024)",
            //    maxLength: 1024,
            //    nullable: true);

            //migrationBuilder.AddColumn<string>(
            //    name: "ReturnValue",
            //    table: "AbpAuditLogs",
            //    type: "nvarchar(max)",
            //    nullable: true);

            //migrationBuilder.CreateTable(
            //    name: "AbpDynamicProperties",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        PropertyName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        DisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        InputType = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Permission = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        TenantId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AbpDynamicProperties", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AbpOrganizationUnitRoles",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        TenantId = table.Column<int>(type: "int", nullable: true),
            //        RoleId = table.Column<int>(type: "int", nullable: false),
            //        OrganizationUnitId = table.Column<long>(type: "bigint", nullable: false),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AbpOrganizationUnitRoles", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AbpWebhookEvents",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        WebhookName = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        TenantId = table.Column<int>(type: "int", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AbpWebhookEvents", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AbpWebhookSubscriptions",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        TenantId = table.Column<int>(type: "int", nullable: true),
            //        WebhookUri = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        Secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        IsActive = table.Column<bool>(type: "bit", nullable: false),
            //        Webhooks = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        Headers = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatorUserId = table.Column<long>(type: "bigint", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AbpWebhookSubscriptions", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AppSubscriptionPaymentsExtensionData",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        SubscriptionPaymentId = table.Column<long>(type: "bigint", nullable: false),
            //        Key = table.Column<string>(type: "nvarchar(450)", nullable: true),
            //        Value = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AppSubscriptionPaymentsExtensionData", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AppUserDelegations",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        SourceUserId = table.Column<long>(type: "bigint", nullable: false),
            //        TargetUserId = table.Column<long>(type: "bigint", nullable: false),
            //        TenantId = table.Column<int>(type: "int", nullable: true),
            //        StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
            //        LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
            //        IsDeleted = table.Column<bool>(type: "bit", nullable: false),
            //        DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
            //        DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AppUserDelegations", x => x.Id);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AbpDynamicEntityProperties",
            //    columns: table => new
            //    {
            //        Id = table.Column<int>(type: "int", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        EntityFullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
            //        DynamicPropertyId = table.Column<int>(type: "int", nullable: false),
            //        TenantId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AbpDynamicEntityProperties", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AbpDynamicEntityProperties_AbpDynamicProperties_DynamicPropertyId",
            //            column: x => x.DynamicPropertyId,
            //            principalTable: "AbpDynamicProperties",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AbpDynamicPropertyValues",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        TenantId = table.Column<int>(type: "int", nullable: true),
            //        DynamicPropertyId = table.Column<int>(type: "int", nullable: false)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AbpDynamicPropertyValues", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AbpDynamicPropertyValues_AbpDynamicProperties_DynamicPropertyId",
            //            column: x => x.DynamicPropertyId,
            //            principalTable: "AbpDynamicProperties",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AbpWebhookSendAttempts",
            //    columns: table => new
            //    {
            //        Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        WebhookEventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        WebhookSubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
            //        Response = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        ResponseStatusCode = table.Column<int>(type: "int", nullable: true),
            //        CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
            //        LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
            //        TenantId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AbpWebhookSendAttempts", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AbpWebhookSendAttempts_AbpWebhookEvents_WebhookEventId",
            //            column: x => x.WebhookEventId,
            //            principalTable: "AbpWebhookEvents",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateTable(
            //    name: "AbpDynamicEntityPropertyValues",
            //    columns: table => new
            //    {
            //        Id = table.Column<long>(type: "bigint", nullable: false)
            //            .Annotation("SqlServer:Identity", "1, 1"),
            //        Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
            //        EntityId = table.Column<string>(type: "nvarchar(max)", nullable: true),
            //        DynamicEntityPropertyId = table.Column<int>(type: "int", nullable: false),
            //        TenantId = table.Column<int>(type: "int", nullable: true)
            //    },
            //    constraints: table =>
            //    {
            //        table.PrimaryKey("PK_AbpDynamicEntityPropertyValues", x => x.Id);
            //        table.ForeignKey(
            //            name: "FK_AbpDynamicEntityPropertyValues_AbpDynamicEntityProperties_DynamicEntityPropertyId",
            //            column: x => x.DynamicEntityPropertyId,
            //            principalTable: "AbpDynamicEntityProperties",
            //            principalColumn: "Id",
            //            onDelete: ReferentialAction.Cascade);
            //    });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpUserOrganizationUnits_UserId",
            //    table: "AbpUserOrganizationUnits",
            //    column: "UserId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpUserLogins_ProviderKey_TenantId",
            //    table: "AbpUserLogins",
            //    columns: new[] { "ProviderKey", "TenantId" },
            //    unique: true,
            //    filter: "[TenantId] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpSettings_TenantId_Name_UserId",
            //    table: "AbpSettings",
            //    columns: new[] { "TenantId", "Name", "UserId" },
            //    unique: true);

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpPersistedGrants_Expiration",
            //    table: "AbpPersistedGrants",
            //    column: "Expiration");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpPersistedGrants_SubjectId_SessionId_Type",
            //    table: "AbpPersistedGrants",
            //    columns: new[] { "SubjectId", "SessionId", "Type" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpDynamicEntityProperties_DynamicPropertyId",
            //    table: "AbpDynamicEntityProperties",
            //    column: "DynamicPropertyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpDynamicEntityProperties_EntityFullName_DynamicPropertyId_TenantId",
            //    table: "AbpDynamicEntityProperties",
            //    columns: new[] { "EntityFullName", "DynamicPropertyId", "TenantId" },
            //    unique: true,
            //    filter: "[EntityFullName] IS NOT NULL AND [TenantId] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpDynamicEntityPropertyValues_DynamicEntityPropertyId",
            //    table: "AbpDynamicEntityPropertyValues",
            //    column: "DynamicEntityPropertyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpDynamicProperties_PropertyName_TenantId",
            //    table: "AbpDynamicProperties",
            //    columns: new[] { "PropertyName", "TenantId" },
            //    unique: true,
            //    filter: "[PropertyName] IS NOT NULL AND [TenantId] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpDynamicPropertyValues_DynamicPropertyId",
            //    table: "AbpDynamicPropertyValues",
            //    column: "DynamicPropertyId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpOrganizationUnitRoles_TenantId_OrganizationUnitId",
            //    table: "AbpOrganizationUnitRoles",
            //    columns: new[] { "TenantId", "OrganizationUnitId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpOrganizationUnitRoles_TenantId_RoleId",
            //    table: "AbpOrganizationUnitRoles",
            //    columns: new[] { "TenantId", "RoleId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpWebhookSendAttempts_WebhookEventId",
            //    table: "AbpWebhookSendAttempts",
            //    column: "WebhookEventId");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AppSubscriptionPaymentsExtensionData_SubscriptionPaymentId_Key_IsDeleted",
            //    table: "AppSubscriptionPaymentsExtensionData",
            //    columns: new[] { "SubscriptionPaymentId", "Key", "IsDeleted" },
            //    unique: true,
            //    filter: "[Key] IS NOT NULL");

            //migrationBuilder.CreateIndex(
            //    name: "IX_AppUserDelegations_TenantId_SourceUserId",
            //    table: "AppUserDelegations",
            //    columns: new[] { "TenantId", "SourceUserId" });

            //migrationBuilder.CreateIndex(
            //    name: "IX_AppUserDelegations_TenantId_TargetUserId",
            //    table: "AppUserDelegations",
            //    columns: new[] { "TenantId", "TargetUserId" });

            //migrationBuilder.AddForeignKey(
            //    name: "FK_AbpUserOrganizationUnits_AbpUsers_UserId",
            //    table: "AbpUserOrganizationUnits",
            //    column: "UserId",
            //    principalTable: "AbpUsers",
            //    principalColumn: "Id",
            //    onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            //migrationBuilder.DropForeignKey(
            //    name: "FK_AbpUserOrganizationUnits_AbpUsers_UserId",
            //    table: "AbpUserOrganizationUnits");

            //migrationBuilder.DropTable(
            //    name: "AbpDynamicEntityPropertyValues");

            //migrationBuilder.DropTable(
            //    name: "AbpDynamicPropertyValues");

            //migrationBuilder.DropTable(
            //    name: "AbpOrganizationUnitRoles");

            //migrationBuilder.DropTable(
            //    name: "AbpWebhookSendAttempts");

            //migrationBuilder.DropTable(
            //    name: "AbpWebhookSubscriptions");

            //migrationBuilder.DropTable(
            //    name: "AppSubscriptionPaymentsExtensionData");

            //migrationBuilder.DropTable(
            //    name: "AppUserDelegations");

            //migrationBuilder.DropTable(
            //    name: "AbpDynamicEntityProperties");

            //migrationBuilder.DropTable(
            //    name: "AbpWebhookEvents");

            //migrationBuilder.DropTable(
            //    name: "AbpDynamicProperties");

            //migrationBuilder.DropIndex(
            //    name: "IX_AbpUserOrganizationUnits_UserId",
            //    table: "AbpUserOrganizationUnits");

            //migrationBuilder.DropIndex(
            //    name: "IX_AbpUserLogins_ProviderKey_TenantId",
            //    table: "AbpUserLogins");

            //migrationBuilder.DropIndex(
            //    name: "IX_AbpSettings_TenantId_Name_UserId",
            //    table: "AbpSettings");

            //migrationBuilder.DropIndex(
            //    name: "IX_AbpPersistedGrants_Expiration",
            //    table: "AbpPersistedGrants");

            //migrationBuilder.DropIndex(
            //    name: "IX_AbpPersistedGrants_SubjectId_SessionId_Type",
            //    table: "AbpPersistedGrants");

            //migrationBuilder.DropColumn(
            //    name: "Description",
            //    table: "AppSubscriptionPayments");

            //migrationBuilder.DropColumn(
            //    name: "EditionPaymentType",
            //    table: "AppSubscriptionPayments");

            //migrationBuilder.DropColumn(
            //    name: "ErrorUrl",
            //    table: "AppSubscriptionPayments");

            //migrationBuilder.DropColumn(
            //    name: "IsRecurring",
            //    table: "AppSubscriptionPayments");

            //migrationBuilder.DropColumn(
            //    name: "SuccessUrl",
            //    table: "AppSubscriptionPayments");

            //migrationBuilder.DropColumn(
            //    name: "Description",
            //    table: "AppBinaryObjects");

            //migrationBuilder.DropColumn(
            //    name: "SubscriptionPaymentType",
            //    table: "AbpTenants");

            //migrationBuilder.DropColumn(
            //    name: "ConsumedTime",
            //    table: "AbpPersistedGrants");

            //migrationBuilder.DropColumn(
            //    name: "Description",
            //    table: "AbpPersistedGrants");

            //migrationBuilder.DropColumn(
            //    name: "SessionId",
            //    table: "AbpPersistedGrants");

            //migrationBuilder.DropColumn(
            //    name: "NewValueHash",
            //    table: "AbpEntityPropertyChanges");

            //migrationBuilder.DropColumn(
            //    name: "OriginalValueHash",
            //    table: "AbpEntityPropertyChanges");

            //migrationBuilder.DropColumn(
            //    name: "DailyPrice",
            //    table: "AbpEditions");

            //migrationBuilder.DropColumn(
            //    name: "WeeklyPrice",
            //    table: "AbpEditions");

            //migrationBuilder.DropColumn(
            //    name: "ExceptionMessage",
            //    table: "AbpAuditLogs");

            //migrationBuilder.DropColumn(
            //    name: "ReturnValue",
            //    table: "AbpAuditLogs");

            //migrationBuilder.RenameColumn(
            //    name: "ExternalPaymentId",
            //    table: "AppSubscriptionPayments",
            //    newName: "PaymentId");

            //migrationBuilder.RenameIndex(
            //    name: "IX_AppSubscriptionPayments_ExternalPaymentId_Gateway",
            //    table: "AppSubscriptionPayments",
            //    newName: "IX_AppSubscriptionPayments_PaymentId_Gateway");

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "LastLoginTime",
            //    table: "AbpUsers",
            //    type: "datetime2",
            //    nullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "UserNameOrEmailAddress",
            //    table: "AbpUserLoginAttempts",
            //    type: "nvarchar(255)",
            //    maxLength: 255,
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(256)",
            //    oldMaxLength: 256,
            //    oldNullable: true);

            //migrationBuilder.AddColumn<DateTime>(
            //    name: "LastLoginTime",
            //    table: "AbpUserAccounts",
            //    type: "datetime2",
            //    nullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "ReportsLogoFileType",
            //    table: "AbpTenants",
            //    type: "nvarchar(64)",
            //    maxLength: 64,
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Value",
            //    table: "AbpSettings",
            //    type: "nvarchar(2000)",
            //    maxLength: 2000,
            //    nullable: true,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(max)",
            //    oldNullable: true);

            //migrationBuilder.AlterColumn<string>(
            //    name: "LanguageName",
            //    table: "AbpLanguageTexts",
            //    type: "nvarchar(10)",
            //    maxLength: 10,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(128)",
            //    oldMaxLength: 128);

            //migrationBuilder.AlterColumn<string>(
            //    name: "Name",
            //    table: "AbpLanguages",
            //    type: "nvarchar(10)",
            //    maxLength: 10,
            //    nullable: false,
            //    oldClrType: typeof(string),
            //    oldType: "nvarchar(128)",
            //    oldMaxLength: 128);

            //migrationBuilder.CreateIndex(
            //    name: "IX_AbpSettings_TenantId_Name",
            //    table: "AbpSettings",
            //    columns: new[] { "TenantId", "Name" });
        }
    }
}
