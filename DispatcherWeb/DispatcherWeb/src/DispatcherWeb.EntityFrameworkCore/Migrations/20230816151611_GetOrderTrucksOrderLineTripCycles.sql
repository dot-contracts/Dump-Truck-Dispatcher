CREATE OR ALTER PROCEDURE [dbo].[sp_GetOrderTrucksOrderLineJobCycles]
    @TENANT_ID AS INT, @OL_ID AS INT
AS
BEGIN
    SET NOCOUNT ON;

    WITH loads AS
    (
        select	l.Id [LoadId]
                , l.SourceLatitude
                , l.SourceLongitude
                , l.SourceDateTime
                , l.DestinationLatitude
                , l.DestinationLongitude
                , l.DestinationDateTime
                , o.Id [OrderId]
                , o.DeliveryDate
                , ol.Id [OrderLineId]
                , ol.LoadAtId
                , ol.DeliverToId
                , tk.Id [TicketId]
                , tk.Quantity [TicketQuantity]
                , tk.UnitOfMeasureId [TicketUomId]
                , truck.TruckCode
                , d.Id [DispatchId]
                , d.TruckId
                , d.DriverId

        FROM OrderLine ol
            INNER JOIN [Order] o ON ol.OrderId = o.Id
            INNER JOIN OrderLineTruck olt ON olt.OrderLineId = ol.Id
            INNER JOIN Dispatch d ON d.OrderLineTruckId = olt.Id
            INNER JOIN [Load] l ON d.Id = l.DispatchId
            INNER JOIN Truck truck ON truck.Id = d.TruckId
            LEFT JOIN Ticket tk ON tk.LoadId = l.Id

        WHERE	ol.TenantId = @TENANT_ID AND
                ol.Id = @OL_ID  AND
                ISNULL(ol.IsDeleted, 0) = 0 AND
                ISNULL(o.IsDeleted, 0) = 0 AND
                ISNULL(olt.IsDeleted, 0) = 0 AND
                ISNULL(d.IsDeleted, 0) = 0 AND
                ISNULL(l.IsDeleted, 0) = 0 AND
                ISNULL(truck.IsDeleted, 0) = 0 AND
                ISNULL(tk.IsDeleted, 0) = 0

        ORDER BY [TruckId], [LoadId] OFFSET 0 ROWS

    ) --> CTE: SELECT * FROM loads

    , LoadTimeLines AS
    (
        Select	[LoadId]
                , SourceLatitude
                , SourceLongitude
                , SourceDateTime
                , DestinationLatitude
                , DestinationLongitude
                , DestinationDateTime
                , [OrderId]
                , DeliveryDate
                , [OrderLineId]
                , LoadAtId
                , DeliverToId
                , [TicketId]
                , [TicketQuantity]
                , [TicketUomId]
                , TruckCode
                , [DispatchId]
                , TruckId
                , DriverId

                , (Select max(l3.DestinationDateTime)
                        from Dispatch d2
                            inner join Load l3 on d2.Id = l3.DispatchId
                        Where loads.DriverId = d2.DriverId
                            and loads.TruckId = d2.TruckId
                            and l3.DestinationDateTime <= loads.SourceDateTime
                            and l3.DestinationDateTime >= loads.DeliveryDate
                            AND ISNULL(l3.IsDeleted, 0) = 0
                            AND ISNULL(d2.IsDeleted, 0) = 0
                        Group by d2.DriverId, d2.TruckId) [PreviousJobEnd]

                , (Select max(StartDateTime)
                        from EmployeeTime et
                        where et.DriverId = loads.DriverId and
                            et.StartDateTime <= loads.SourceDateTime AND
                            ISNULL(et.IsDeleted, 0) = 0 ) [PreviousClockin]

                , (Select Count(StartDateTime)
                        from EmployeeTime et
                        where et.DriverId = loads.DriverId and
                            et.EndDateTime >= loads.SourceDateTime and
                            et.EndDateTime <= loads.DestinationDateTime
                            AND ISNULL(et.IsDeleted, 0) = 0)[ClockOutCount]

        FROM loads

    ) --> CTE: SELECT * FROM LoadTimeLines

    , TripSegmentsBase AS
    (
        SELECT ROW_NUMBER() OVER(ORDER BY (SELECT NULL)) [RowNo], * FROM (
            SELECT	0[TripType] -- Trip to Load

                    , [LoadId]
                    , SourceLatitude
                    , SourceLongitude
                    , [SourceDateTime]
                    , DestinationLatitude
                    , DestinationLongitude
                    , [DestinationDateTime]
                    , [OrderId]
                    , DeliveryDate
                    , [OrderLineId]

                    , LoadAtId
                    , DeliverToId
                    , [TicketId]
                    , [TicketQuantity]
                    , [TicketUomId]

                    , TruckCode
                    , [DispatchId]
                    , TruckId
                    , DriverId

                    , [PreviousJobEnd]
                    , [PreviousClockin]
                    , [ClockOutCount]
            FROM LoadTimeLines
            UNION ALL
            SELECT	1[TripType] -- Trip to Dump

                    , [LoadId]
                    , SourceLatitude
                    , SourceLongitude
                    , [SourceDateTime]
                    , DestinationLatitude
                    , DestinationLongitude
                    , [DestinationDateTime]
                    , [OrderId]
                    , DeliveryDate
                    , [OrderLineId]

                    , LoadAtId
                    , DeliverToId

                    , [TicketId]
                    , [TicketQuantity]
                    , [TicketUomId]

                    , TruckCode
                    , [DispatchId]
                    , TruckId
                    , DriverId

                    , [PreviousJobEnd]
                    , [PreviousClockin]
                    , [ClockOutCount]
            FROM LoadTimeLines
        ) a
        ORDER BY TruckId, [LoadId], [TripType] OFFSET 0 ROWS
    ) --> CTE: SELECT * FROM TripSegmentsBase

    , TripSegments AS
    (
        SELECT
            a.[LoadId]
            , a.SourceLatitude
            , a.SourceLongitude
            , a.DestinationLatitude
            , a.DestinationLongitude
            , a.[OrderId]
            , a.DeliveryDate
            , a.[OrderLineId]

            , a.LoadAtId
            , a.DeliverToId

            , a.[TicketId]
            , a.[TicketQuantity]
            , a.[TicketUomId]

            , a.TruckCode
            , a.[DispatchId]
            , a.TruckId
            , a.DriverId
            , a.TripType

            , IIF(a.TripType=0, '0-ToLoadSite', '1-ToDumpSite')[TripTypeDesc]

            , (
                CASE a.TripType
                    WHEN 0 THEN IIF(a.PreviousJobEnd IS NULL,
                                    a.PreviousClockIn,
                                    IIF(a.PreviousClockIn > a.PreviousJobEnd, a.PreviousClockIn, a.PreviousJobEnd)
                                )
                    ELSE IIF(a.ClockOutCount > 0,
                                (SELECT x.PreviousClockIn FROM TripSegmentsBase x WHERE x.RowNo = (a.RowNo+1)),
                                a.[SourceDateTime]
                            )
                END
            ) [TripStart]

            , (
                CASE a.TripType
                    WHEN 0 THEN a.[SourceDateTime]
                    ELSE a.[DestinationDateTime]
                END
            ) [TripEnd]

        FROM TripSegmentsBase a
    )

    SELECT a.*,
            loadAt.[Name] [LoadAtName],
            loadAt.[StreetAddress] [LoadAtStreetAddress],
            loadAt.[City] [LoadAtCity],
            loadAt.[State] [LoadAtState],

            deliverTo.[Name] [DeliverToName],
            deliverTo.[StreetAddress] [DeliverToStreetAddress],
            deliverTo.[City] [DeliverToCity],
            deliverTo.[State] [DeliverToState],

            tum.[Name][TicketUom],

            TRIM(TRIM(d.[FirstName]) + N' ' + d.[LastName])[DriverName]

    FROM TripSegments a
        LEFT JOIN UnitOfMeasure tum ON tum.Id = a.TicketUomId
        LEFT JOIN [Location] loadAt ON loadAt.Id = a.LoadAtId
        LEFT JOIN [Location] deliverTo ON deliverTo.Id = a.DeliverToId
        LEFT JOIN [Driver] d ON d.Id = a.DriverId

    WHERE	ISNULL(tum.IsDeleted, 0) = 0 AND
            ISNULL(loadAt.IsDeleted, 0) = 0 AND
            ISNULL(deliverTo.IsDeleted, 0) = 0 AND
            ISNULL(d.IsDeleted, 0) = 0
    /*

    DECLARE @OL_ID AS INT = 11813;
    DECLARE @TENANT_ID AS INT = 2;

    --EXEC [dbo].[sp_GetOrderTrucksOrderLineJobCycles] @TENANT_ID, @OL_ID;

    EXEC [dbo].[sp_GetOrderTrucksOrderLineJobCycles] @TENANT_ID, 11814;

    */
END;