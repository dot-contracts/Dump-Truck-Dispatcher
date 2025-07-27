select * from DriverApplicationLog where DateTime >= '2020-10-17' order by DriverId, DateTime

select da.BatchOrder, da.DateTime, d.FirstName, d.LastName, da.Level, da.Message 
from DriverApplicationLog da
		inner join Driver d on d.Id = da.DriverId
where da.tenantid = 11 and DateTime >= '2020-10-21' and (Message like 'page load %' or Message like 'sync tags before sync.register: %' or Message like 'finished uploading %')
order by DriverId, DateTime

select d.FirstName, d.LastName, da.Message , count(*)
from DriverApplicationLog da
		inner join Driver d on d.Id = da.DriverId
where da.tenantid = 11 and DateTime >= '2020-10-12' and (Message like 'page load %' or Message like 'sync tags before sync.register: %' or Message like 'finished uploading %')
Group by d.FirstName, d.LastName, da.Message
order by d.FirstName, d.LastName, da.Message

select driverid, count(*) from DriverApplicationLog where DateTime >= '2020-10-17' group by driverId
select * from driver where id = 449

select da.Id, d.FirstName, d.LastName, t.TruckCode, da.[Date] 
from driverassignment da 
				inner join Truck t on t.id = da.TruckId  
				inner join Driver d on d.Id = da.DriverId
where Date >= '2020-10-01' and da.tenantid = 11
order by driverid, Date

select di.LastModificationTime, d.FirstName, d.LastName, t.TruckCode, di.CreationTime, di.Acknowledged, di.LastModifierUserId, di.Message
from Dispatch di inner join Truck t on t.id = di.TruckId  
				inner join Driver d on d.Id = di.DriverId
where di.LastModificationTime >= '2020-11-04' and status = 7 and di.tenantid = 11
order by di.DriverId, di.CreationTime
