select t.id, t.TicketNumber, t.Quantity, t.TicketDate, t.TruckCode, t.ServiceId, t.UnitOfMeasureId, t.ServiceId, ol.id as olid, ol.FreightPricePerUnit, ol.FreightPricePerUnit * t.Quantity as Amount, c.Name, o.id, o.DateTime, s.Name, s1.Name, t.Quantity*ol.FreightPricePerUnit as Price
from Ticket t
	Inner join OrderLine ol on ol.id = t.Orderlineid
	inner join [order] o on o.id = ol.OrderId
	inner join Customer c on t.CustomerId = c.Id
	inner join Supplier s on s.id = t.DeliverToId
	inner join Supplier s1 on s1.id = t.LoadAtId
where t.tenantid = 16 and t.TicketDate >= '2021-02-02' and t.TicketDate < '2021-02-03' and t.isdeleted = 0 --and t.id = 4457
order by c.Name, s.Name, s1.Name
