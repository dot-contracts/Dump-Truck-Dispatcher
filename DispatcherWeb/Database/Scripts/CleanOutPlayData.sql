select * from AbpTenants where isdeleted = 0

BEGIN TRY
	Begin Transaction
	-- Specify the tenant
	DECLARE @Tenant INT;
	SET @Tenant = 5;

	-- Delete pay statements for tenant
	Delete PayStatementTime where TenantId = @Tenant
	Delete PayStatementTicket where TenantId = @Tenant
	Delete PayStatementDriverDateConflict where TenantId = @Tenant
	Delete PayStatementDetail where TenantId = @Tenant
	Delete PayStatement where TenantId = @Tenant

	-- Delete invoices for tenant
	Delete InvoiceBatch where TenantId = @Tenant
	Delete InvoiceEmails where InvoiceId in (Select Id from Invoice where tenantId = @Tenant)
	Delete InvoiceLine where TenantId = @Tenant
	Delete InvoiceUploadBatch where TenantId = @Tenant
	Delete Invoice where TenantId = @Tenant

	-- Delete time entries for tenant
	Delete EmployeeTimePayStatementTime where EmployeeTimeId in (Select id from EmployeeTime where TenantId = @Tenant)
	Delete EmployeeTime where TenantId = @Tenant

	-- Delete dispatches and loads for tenant
	Delete [Load] where TenantId = @Tenant
	Delete Dispatch where TenantId = @Tenant

	-- Delete tickets for tenant
	Delete TicketSettlements where TicketId in (Select id from Ticket where TenantId = @Tenant)
	Delete ReceiptLine where TenantId = @Tenant
	Delete Ticket where TenantId = @Tenant

	-- Delete LeaseHaulerRequest and loads for tenant
	Delete LeaseHaulerRequest where TenantId = @Tenant

	-- Delete Billed and payments
	Delete BilledOrder where OrderId in (Select id from [Order]  where TenantId = @Tenant)
	Delete OrderPayment where TenantId = @Tenant

	-- Delete receipts for tenant
	Delete Receipt where TenantId = @Tenant

	-- Delete Orders and loads for tenant
	Delete OrderLineTruck where TenantId = @Tenant
	Delete OrderTruck where TenantId = @Tenant
	Delete OrderLineOfficeAmount where TenantId = @Tenant
	Delete OrderLine where TenantId = @Tenant
	Delete OrderLeaseHauler where TenantId = @Tenant
	Delete OrderEmails where OrderId in (Select id from [Order]  where TenantId = @Tenant)
	Delete [Order] where TenantId = @Tenant

	Commit Transaction
END TRY
Begin Catch
	Rollback Transaction
	SELECT
		ERROR_NUMBER() AS ErrorNumber,
		ERROR_STATE() AS ErrorState,
		ERROR_SEVERITY() AS ErrorSeverity,
		ERROR_PROCEDURE() AS ErrorProcedure,
		ERROR_LINE() AS ErrorLine,
		ERROR_MESSAGE() AS ErrorMessage;
End Catch
