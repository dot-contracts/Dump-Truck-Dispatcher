SELECT * FROM abptenants where isdeleted = 0
SELECT COunt(Id), Category FROM Truck Group By Category

SELECT * FROM Truck where IsDeleted = 0 and IsActive = 1 and category =3

SELECT top (100) * FROM OrderTruck

SELECT COUNT(*), LocationId 
FROM OrderTruck Inner Join Truck on Truck.Id = OrderTruck.TruckId
Where Truck.Category in (4, 5) and OrderTruck.CreationTime > '2018-01-01' and OrderTruck.CreationTime <='2018-12-31'
Group By Truck.LocationId
 
SELECT COUNT(*) from [Order]

SELECT * FROM Truck where Category in (4, 5) and isdeleted = 0 and DefaultDriverId is null 
SELECT * FROM Driver where officeid is null

SELECT * FROM Truck where DefaultDriverId = 444 and locationid = 5

-- Set the default driver for lease hauler trucks
--Begin Transaction
--Update Truck Set DefaultDriverId = 444 where Category in (4, 5) and DefaultDriverId is null and isdeleted = 0
--commit



select * from driver where emailaddress is not null and OrderNotifyPreferredFormat in (1, 3) order by emailaddress desc