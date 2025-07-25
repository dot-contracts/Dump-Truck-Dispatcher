SET IDENTITY_INSERT [dbo].[ActiveReportCategory] ON;
INSERT INTO [dbo].[ActiveReportCategory] (ID, [Name], CreationTime, IsDeleted)
VALUES (1, 'Default Category', GETDATE(), 0);
SET IDENTITY_INSERT [dbo].[ActiveReportCategory] OFF;

SET IDENTITY_INSERT [dbo].[ActiveReport] ON;
INSERT INTO [dbo].[ActiveReport] (ID, [Name], [Description], [Path], CategoryId, CreationTime, IsDeleted)
VALUES (1, 'TenantStatisticsReport', 'Tenant Statistics Report', 'TenantStatisticsReport', 1, GETDATE(), 0);
SET IDENTITY_INSERT [dbo].[ActiveReport] OFF;

