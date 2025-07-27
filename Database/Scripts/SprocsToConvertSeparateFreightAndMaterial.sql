CREATE OR ALTER PROCEDURE UpdateDesignations 
	@TenantId INT 
AS 
BEGIN 
---- Convert all designations to either 1, 2, or 3 for quotes. We need to prevent anything other than these 3 to show 
---- when separate freight and materials feature is enabled
---- First do quotelines

	BEGIN TRANSACTION;

	BEGIN TRY
		---- BackhaulFreightOnly to FreightOnly
		Update QuoteLine set Designation = 1 where Designation = 5 and QuoteLine.TenantId = @TenantId;

		---- Disposal
		Update QuoteLine Set Designation = 2 where Designation = 6 and (FreightRate = 0 or FreightRate is null) and QuoteLine.TenantId = @TenantId;
		Update QuoteLine Set Designation = 3 where Designation = 6 and FreightRate > 0 and FreightRate is not null and QuoteLine.TenantId = @TenantId;

		----BackhaulFreightAndMaterial to FreightAndMaterial
		Update QuoteLine set Designation = 3 where Designation = 9 and QuoteLine.TenantId = @TenantId;

		----BackhaulFreightAndDisposal to FreightAndMaterial
		Update QuoteLine set Designation = 3 where Designation = 7 and QuoteLine.TenantId = @TenantId;

		----StraightHaulFreightAndDisposal to FreightAndMaterial
		Update QuoteLine set Designation = 3 where Designation = 8 and QuoteLine.TenantId = @TenantId;

		--Repeat for orderlines
		---- BackhaulFreightOnly to FreightOnly
		Update OrderLine set Designation = 1 where Designation = 5 and OrderLine.TenantId = @TenantId;

		---- Disposal
		Update OrderLine Set Designation = 2 where Designation = 6 and (FreightPricePerUnit = 0 or FreightPricePerUnit is null) and OrderLine.TenantId = @TenantId;
		Update OrderLine Set Designation = 3 where Designation = 6 and FreightPricePerUnit > 0 and FreightPricePerUnit is not null and OrderLine.TenantId = @TenantId;

		----BackhaulFreightAndMaterial to FreightAndMaterial
		Update OrderLine set Designation = 3 where Designation = 9 and OrderLine.TenantId = @TenantId;

		----BackhaulFreightAndDisposal to FreightAndMaterial
		Update OrderLine set Designation = 3 where Designation = 7 and OrderLine.TenantId = @TenantId;

		----StraightHaulFreightAndDisposal to FreightAndMaterial
		Update OrderLine set Designation = 3 where Designation = 8 and OrderLine.TenantId = @TenantId;

		-- Repeat for receiptlines
		---- BackhaulFreightOnly to FreightOnly
		Update ReceiptLine set Designation = 1 where Designation = 5 and ReceiptLine.TenantId = @TenantId;

		---- Disposal
		Update ReceiptLine Set Designation = 2 where Designation = 6 and (FreightRate = 0 or FreightRate is null) and ReceiptLine.TenantId = @TenantId;
		Update ReceiptLine Set Designation = 3 where Designation = 6 and FreightRate > 0 and FreightRate is not null and ReceiptLine.TenantId = @TenantId;

		----BackhaulFreightAndMaterial to FreightAndMaterial
		Update ReceiptLine set Designation = 3 where Designation = 9 and ReceiptLine.TenantId = @TenantId;

		----BackhaulFreightAndDisposal to FreightAndMaterial
		Update ReceiptLine set Designation = 3 where Designation = 7 and ReceiptLine.TenantId = @TenantId;

		----StraightHaulFreightAndDisposal to FreightAndMaterial
		Update ReceiptLine set Designation = 3 where Designation = 8 and ReceiptLine.TenantId = @TenantId;
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH;

END;
GO

--These items have a type of service. Since they are on a freight and material order or quote, they should be of type non-inventory product.
CREATE OR ALTER PROCEDURE UpdateItemsToMaterialType 
	@TenantId INT 
AS 
BEGIN 
	BEGIN TRANSACTION;

	BEGIN TRY
		UPDATE Item
		SET [TYPE] = 4 
		WHERE Id in (SELECT  i.Id
					FROM OrderLine ol 
						inner join Item i on i.id = ol.FreightItemId
					WHERE i.Type = 1 and ol.Designation = 3 and ol.TenantId = @TenantId
					GROUP BY i.Id);

		UPDATE Item
		SET [TYPE] = 4 
		WHERE Id in (SELECT  i.Id
					FROM QuoteLine ql 
						inner join Item i on i.id = ql.FreightItemId
					WHERE i.Type = 1 and ql.Designation = 3 and ql.TenantId = @TenantId
					GROUP BY i.Id);
		
		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;

		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE UpdateQuoteLineItems 
	@TenantId INT,
	@HaulingServiceId INT
AS 
BEGIN 
	BEGIN TRANSACTION;

	BEGIN TRY

		-- Update rows with material only designation. First copy the FreightItemId to the MaterialItemId and then set the FeightItemId to null
		UPDATE QuoteLine 
		SET MaterialItemId = FreightItemId
		WHERE Designation = 2 and MaterialItemId is null and FreightItemId is not null and TenantId = @TenantId;

		UPDATE QuoteLine 
		SET FreightItemId = NULL
		WHERE Designation = 2 and FreightItemId is not null and TenantId = @TenantId;


		-- Update rows with freight only or freight and material designations. If the freight isn't a service, copy the FreightItemId to the MaterialItemId 
		-- and set the freightItemId to the DTDHauling item.
		UPDATE QuoteLine 
		SET MaterialItemId = FreightItemId
		WHERE Id in (SELECT ql.Id 
					FROM QuoteLine ql
						Inner join Item i on i.ID = ql.FreightItemId
					WHERE ql.Designation != 2 and i.Type != 1 and ql.FreightItemId is not null and ql.MaterialItemId is null and ql.TenantId = @TenantId);

		UPDATE QuoteLine 
		SET FreightItemId = @HaulingServiceId
		WHERE Id in (SELECT ql.Id 
					FROM QuoteLine ql
						Inner join Item i on i.ID = ql.FreightItemId
					WHERE ql.Designation != 2 and i.Type != 1 and ql.FreightItemId is not null and ql.TenantId = @TenantId);


		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH;
END;
GO

CREATE OR ALTER PROCEDURE UpdateOrderLineItems 
	@TenantId INT,
	@HaulingServiceId INT

AS 
BEGIN 
	BEGIN TRANSACTION;

	BEGIN TRY

		-- Update rows with material only designation. First copy the FreightItemId to the MaterialItemId and then set the FeightItemId to null
		UPDATE OrderLine 
		SET MaterialItemId = FreightItemId
		WHERE Designation = 2 and MaterialItemId is null and FreightItemId is not null and TenantId = @TenantId;

		UPDATE OrderLine 
		SET FreightItemId = NULL
		WHERE Designation = 2 and FreightItemId is not null and TenantId = @TenantId;


		-- Update rows with freight only or freight and material designations. If the freight isn't a service, copy the FreightItemId to the MaterialItemId 
		-- and set the freightItemId to the DTDHauling item.
		UPDATE OrderLine 
		SET MaterialItemId = FreightItemId
		WHERE Id in (SELECT ol.Id 
					FROM OrderLine ol
						Inner join Item i on i.ID = ol.FreightItemId
					WHERE ol.Designation != 2 and i.Type != 1 and ol.FreightItemId is not null and ol.MaterialItemId is null and ol.TenantId = @TenantId);

		UPDATE OrderLine 
		SET FreightItemId = @HaulingServiceId
		WHERE Id in (SELECT ol.Id 
					FROM OrderLine ol
						Inner join Item i on i.ID = ol.FreightItemId
					WHERE ol.Designation != 2 and i.Type != 1 and ol.FreightItemId is not null and ol.TenantId = @TenantId);


		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH;

END;
GO

CREATE OR ALTER PROCEDURE UpdateReceiptLineItems 
	@TenantId INT,
	@HaulingServiceId INT
AS 
BEGIN 
	BEGIN TRANSACTION;

	BEGIN TRY

		-- Update rows with material only designation. First copy the FreightItemId to the MaterialItemId and then set the FeightItemId to null
		UPDATE ReceiptLine 
		SET MaterialItemId = FreightItemId
		WHERE Designation = 2 and MaterialItemId is null and FreightItemId is not null and TenantId = @TenantId;

		UPDATE ReceiptLine 
		SET FreightItemId = NULL
		WHERE Designation = 2 and FreightItemId is not null and TenantId = @TenantId;

		-- Update rows with freight only or freight and material designations. If the freight isn't a service, copy the FreightItemId to the MaterialItemId 
		-- and set the freightItemId to the DTDHauling item.
		UPDATE ReceiptLine 
		SET MaterialItemId = FreightItemId
		WHERE Id in (SELECT rl.Id 
					FROM ReceiptLine rl
						Inner join Item i on i.ID = rl.FreightItemId
					WHERE rl.Designation != 2 and i.Type != 1 and rl.FreightItemId is not null and rl.MaterialItemId is null and rl.TenantId = @TenantId);

		UPDATE ReceiptLine 
		SET FreightItemId = @HaulingServiceId
		WHERE Id in (SELECT rl.Id 
					FROM ReceiptLine rl
						Inner join Item i on i.ID = rl.FreightItemId
					WHERE rl.Designation != 2 and i.Type != 1 and rl.FreightItemId is not null and rl.TenantId = @TenantId);

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH;

END;
GO

CREATE OR ALTER PROCEDURE UpdateTickets 
	@TenantId INT,
	@HaulingServiceId INT
AS 
BEGIN 
	BEGIN TRANSACTION;

	BEGIN TRY

		-- Update tickets with material only designation. First copy the FreightItemId to the MaterialItemId and then set the FeightItemId to null
		UPDATE Ticket 
		SET MaterialItemId = FreightItemId, NonbillableFreight = 1, NonbillableMaterial = 0, MaterialQuantity = Quantity, FreightQuantity = 0
		WHERE Id in (	SELECT t.Id 
						FROM Ticket t
							inner join OrderLine ol on ol.Id = t.OrderLineId
						WHERE ol.Designation = 2 and t.MaterialItemId is null and t.FreightItemId is not null and t.TenantId = @TenantId);

		UPDATE Ticket 
		SET FreightItemId = NULL
		WHERE Id in (	SELECT t.Id 
						FROM Ticket t
							inner join OrderLine ol on ol.Id = t.OrderLineId
						WHERE ol.Designation = 2 and t.FreightItemId is not null and t.TenantId = @TenantId);


		-- Update rows with freight only designations. 
		-- First update the nonbillable values
		UPDATE Ticket 
		SET NonbillableFreight = 0, NonbillableMaterial = 1, MaterialQuantity = Quantity, FreightQuantity = Quantity
		WHERE Id in (SELECT t.Id 
					FROM Ticket t
						inner join OrderLine ol on ol.Id = t.OrderLineId
					WHERE ol.Designation = 1 and t.TenantId = @TenantId);		
		
		--If the freight isn't a service, copy the FreightItemId to the MaterialItemId 
		-- and set the freightItemId to the DTDHauling item.
		UPDATE Ticket 
		SET MaterialItemId = FreightItemId
		WHERE Id in (SELECT t.Id 
					FROM Ticket t
						Inner join Item i on i.ID = t.FreightItemId
						inner join OrderLine ol on ol.Id = t.OrderLineId
					WHERE ol.Designation = 1 and i.Type != 1 and t.FreightItemId is not null and t.MaterialItemId is null and t.TenantId = @TenantId);

		UPDATE Ticket
		SET FreightItemId = @HaulingServiceId
		WHERE Id in (SELECT t.Id 
					FROM Ticket t
						Inner join Item i on i.ID = t.FreightItemId
						inner join OrderLine ol on ol.Id = t.OrderLineId
					WHERE ol.Designation = 1 and i.Type != 1 and t.FreightItemId is not null and t.TenantId = @TenantId);


		-- Update rows with freight and material designations. 
		UPDATE Ticket 
		SET NonbillableFreight = 0, NonbillableMaterial = 0, MaterialQuantity = Quantity, FreightQuantity = Quantity
		WHERE Id in (SELECT t.Id 
					FROM Ticket t
						inner join OrderLine ol on ol.Id = t.OrderLineId
					WHERE ol.Designation = 3 and t.TenantId = @TenantId);		
		
		--If the freight isn't a service, copy the FreightItemId to the MaterialItemId 
		-- and set the freightItemId to the DTDHauling item.
		UPDATE Ticket 
		SET MaterialItemId = FreightItemId
		WHERE Id in (SELECT t.Id 
					FROM Ticket t
						Inner join Item i on i.ID = t.FreightItemId
						inner join OrderLine ol on ol.Id = t.OrderLineId
					WHERE ol.Designation = 3 and i.Type != 1 and t.FreightItemId is not null and t.MaterialItemId is null and t.TenantId = @TenantId);

		UPDATE Ticket
		SET FreightItemId = @HaulingServiceId
		WHERE Id in (SELECT t.Id 
					FROM Ticket t
						Inner join Item i on i.ID = t.FreightItemId
						inner join OrderLine ol on ol.Id = t.OrderLineId
					WHERE ol.Designation = 3 and i.Type != 1 and t.FreightItemId is not null and t.TenantId = @TenantId);
		
		-- Update the remaining historical tickets with no OrderLineId
		UPDATE Ticket
		SET MaterialQuantity = Quantity, FreightQuantity = Quantity
		WHERE (MaterialItemId is null or FreightItemId is null) and OrderLineId is null and TenantId = @TenantId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH;

END;
GO


CREATE OR ALTER PROCEDURE UpdateInvoiceLines 
	@TenantId INT,
	@HaulingServiceId INT
AS 
BEGIN 
	BEGIN TRANSACTION;

	BEGIN TRY

		UPDATE InvoiceLine
		SET IsMaterialTaxable = IsFreightTaxable, FreightQuantity = Quantity, MaterialQuantity = Quantity
		WHERE TenantId = @TenantId;

		COMMIT TRANSACTION;
	END TRY
	BEGIN CATCH
		ROLLBACK TRANSACTION;
		THROW;
	END CATCH;

END;
GO

CREATE OR ALTER PROCEDURE ConvertDataToSeparateFreightAndMaterialItems 
	@TenantId INT 
AS 
BEGIN 
	--Fix the designations
	EXEC UpdateDesignations @TenantId;
	
	-- Add a DTDHauling service
	INSERT INTO [dbo].[Item]
			   ([Name]
			   ,[IsDeleted]
			   ,[DeleterUserId]
			   ,[DeletionTime]
			   ,[LastModificationTime]
			   ,[LastModifierUserId]
			   ,[CreationTime]
			   ,[CreatorUserId]
			   ,[Description]
			   ,[IsActive]
			   ,[TenantId]
			   ,[IncomeAccount]
			   ,[IsTaxable]
			   ,[Type]
			   ,[IsInQuickBooks]
			   ,[MergedToId]
			   ,[ExpenseAccount]
			   ,[UseZoneBasedRates])
		 VALUES
			   ('DTDHauling'
			   ,0
			   , NULL
			   , NULL
			   ,GetDate()
			   ,1
			   ,GetDate()
			   ,1
			   ,'Added in converting to separate freight and material items'
			   ,1
			   ,@TenantId
			   ,''
			   ,0
			   ,1
			   ,0
			   ,NULL
			   ,''
			   ,0);

	DECLARE @HaulingServiceId INT;
	SET @HaulingServiceId = SCOPE_Identity();

	-- Fix the items on quotes and orders that are set as a service and the orderline, receiptline or quoteline has a freight and material designation
	EXEC UpdateItemsToMaterialType @TenantId;

	-- Fix the QuoteLines
	EXEC UpdateQuoteLineItems @TenantId, @HaulingServiceId

	-- Fix the OrderLines
	EXEC UpdateOrderLineItems @TenantId, @HaulingServiceId

	-- Fix the ReceiptLines
	EXEC UpdateReceiptLineItems @TenantId, @HaulingServiceId

	-- Fix the Tickets
	EXEC UpdateTickets @TenantId, @HaulingServiceId

	EXEC UpdateInvoiceLines @TenantId, @HaulingServiceId

END;
GO
