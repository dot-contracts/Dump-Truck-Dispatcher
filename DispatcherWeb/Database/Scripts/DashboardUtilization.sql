SELECT * FROM abptenants where isdeleted = 0


--Need to take into account category
Select t.TruckCode, o.DateTime
	from OrderLineTruck olt inner join OrderLine ol on ol.Id = olt.OrderLineId
	inner join [order] o on o.Id = ol.OrderId
	inner join Truck t on t.Id = olt.TruckId
where olt.tenantid = 2 
	and olt.IsDeleted = 0 
	and DateTime >= '2019-04-01' 
	and DateTime <= '2019-04-03'
	and (t.Category = 1 OR t.Category = 3)
Group by t.TruckCode, o.DateTime

--Need to take into account category
Select truckCode, Category from truck 
	where tenantid = 2 
		and isDeleted = 0 
		and (Category = 1 or Category = 3)
		and InServiceDate <= '2019-04-01'
		and (SoldDate >='2019-04-01' OR SoldDate is null)