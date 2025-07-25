INSERT INTO dbo.AbpRoles 
(DisplayName, IsStatic, IsDefault, TenantId, Name, IsDeleted, DeleterUserId, DeletionTime, LastModificationTime, LastModifierUserId, CreationTime, CreatorUserId, ConcurrencyStamp, NormalizedName) 
SELECT 'Customer' [DisplayName]
		, 1 [IsStatic]
		, 0 [IsDefault]
		, t1.Id [TenantId]
		, 'Customer' [Name]
		, 0 [IsDeleted]
		, NULL [DeleterUserId]
		, NULL [DeletionTime]
		, NULL [LastModificationTime]
		, NULL [LastModifierUserId]
		, GETDATE() [CreationTime]
		, NULL [CreatorUserId]
		, NEWID() [ConcurrencyStamp]
		, 'CUSTOMER'[NormalizedName]
FROM AbpTenants t1
WHERE NOT EXISTS(SELECT Id FROM dbo.AbpRoles WHERE [Name]='Customer' AND TenantId=t1.Id);
