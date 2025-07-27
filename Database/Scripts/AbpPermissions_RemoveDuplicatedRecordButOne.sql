USE [tablenamehere]
GO

With TempDataSet
AS
(
	SELECT Name, RoleId, UserId, ROW_NUMBER() OVER (PARTITION BY [Name], [RoleId], [UserId] ORDER BY Id) as RowNum FROM AbpPermissions
)

--DELETE FROM TempDataSet WHERE RowNum > 1