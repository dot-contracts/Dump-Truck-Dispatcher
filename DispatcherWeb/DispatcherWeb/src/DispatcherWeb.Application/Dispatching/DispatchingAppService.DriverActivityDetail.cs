using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dispatching.Reports;
using Microsoft.EntityFrameworkCore;
using MigraDocCore.DocumentObjectModel;

namespace DispatcherWeb.Dispatching
{
    [AbpAuthorize]
    public partial class DispatchingAppService
    {
        [AbpAuthorize(AppPermissions.Pages_Reports_DriverActivityDetail)]
        public async Task<Document> GetDriverActivityDetailReport(GetDriverActivityDetailReportInput input)
        {
            if (input.DateBegin == null || input.DateEnd == null)
            {
                throw new UserFriendlyException("Date range is required.");
            }

            var daysInRange = (input.DateEnd - input.DateBegin)?.TotalDays + 1;

            switch (daysInRange)
            {
                case > 7:
                    throw new UserFriendlyException("This report is limited to a week. Please select a date range that doesn't exceed 7 days.");
                case > 1 and <= 7:
                    {
                        if (input.DriverId == null)
                        {
                            throw new UserFriendlyException("Driver is required");
                        }
                        break;
                    }
            }

            if (input.DriverId != null && daysInRange > 7)
            {
                throw new UserFriendlyException("Please select a date range of 7 days or less when a single driver is selected.");
            }

            if (input.DriverId == null && daysInRange > 1)
            {
                throw new UserFriendlyException("Please select a date range of 1 day when no driver is selected.");
            }

            var timezone = await GetTimezone();

            var drivers = await (await _driverRepository.GetQueryAsync())
                .Where(x => x.UserId.HasValue)
                .WhereIf(input.DriverId.HasValue, x => input.DriverId == x.Id)
                .Select(x => new
                {
                    DriverId = x.Id,
                    DriverName = x.LastName + ", " + x.FirstName,
                    UserId = x.UserId.Value,
                    CarrierName = x.LeaseHaulerDriver.LeaseHauler.Name,
                }).OrderBy(d => d.DriverName).ToListAsync();

            var startDateConverted = input.DateBegin?.ConvertTimeZoneFrom(timezone);
            var endDateConverted = input.DateEnd?.AddDays(1).ConvertTimeZoneFrom(timezone);
            var employeeTimes = await (await _employeeTimeRepository.GetQueryAsync())
                .WhereIf(input.DriverId.HasValue, x => x.DriverId == input.DriverId)
                .WhereIf(startDateConverted.HasValue, x => x.StartDateTime >= startDateConverted && x.StartDateTime < endDateConverted)
                .Select(x => new
                {
                    x.UserId,
                    x.DriverId,
                    TruckId = x.EquipmentId,
                    x.Truck.TruckCode,
                    x.StartDateTime,
                    x.EndDateTime,
                    TimeClassificationName = x.TimeClassification.Name,
                })
                .OrderBy(x => x.StartDateTime)
                .ToListAsync();

            var driverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .WhereIf(input.DriverId.HasValue, x => x.DriverId == input.DriverId)
                .WhereIf(input.DateBegin.HasValue && input.DateEnd.HasValue, x => x.Date >= input.DateBegin && x.Date <= input.DateEnd)
                .Select(x => new
                {
                    x.DriverId,
                    x.Date,
                    x.TruckId,
                    StartTimeUtc = x.StartTime,
                })
                .OrderBy(x => x.StartTimeUtc == null)
                .ThenBy(x => x.StartTimeUtc)
                .ToListAsync();

            var loads = await (await _loadRepository.GetQueryAsync())
                .WhereIf(input.DriverId.HasValue, x => x.Dispatch.DriverId == input.DriverId)
                .WhereIf(input.DateBegin.HasValue && input.DateEnd.HasValue, x => x.Dispatch.OrderLine.Order.DeliveryDate >= input.DateBegin && x.Dispatch.OrderLine.Order.DeliveryDate <= input.DateEnd)
                .Where(x => x.SourceDateTime.HasValue)
                .Select(x => new
                {
                    Date = x.Dispatch.OrderLine.Order.DeliveryDate,
                    DriverId = (int?)x.Dispatch.DriverId,
                    TruckId = (int?)x.Dispatch.TruckId,
                    TruckCode = x.Dispatch.Truck.TruckCode,
                    CustomerName = x.Dispatch.OrderLine.Order.Customer.Name,
                    LoadAtName = x.Dispatch.OrderLine.LoadAt.DisplayName,
                    DeliverToName = x.Dispatch.OrderLine.DeliverTo.DisplayName,
                    Tickets = x.Tickets.Select(t => new
                    {
                        TicketNumber = t.TicketNumber,
                        FreightQuantity = t.FreightQuantity,
                        MaterialQuantity = t.MaterialQuantity,
                        FreightUomName = t.FreightUom.Name,
                        MaterialUomName = t.MaterialUom.Name,
                        TrailerTruckCode = t.Trailer.TruckCode,
                        VehicleCategory = t.Trailer.VehicleCategory.Name,
                        TicketUomId = t.FreightUomId,
                    }).ToList(),
                    LoadTime = x.SourceDateTime,
                    DeliveryTime = x.DestinationDateTime,
                    JobNumber = x.Dispatch.OrderLine.JobNumber,
                    FreightItemName = x.Dispatch.OrderLine.FreightItem.Name,
                    MaterialItemName = x.Dispatch.OrderLine.MaterialItem.Name,
                    DispatchId = x.DispatchId,
                    OrderLineId = x.Dispatch.OrderLineId,
                    FreightQuantityOrdered = x.Dispatch.FreightQuantity,
                    MaterialQuantityOrdered = x.Dispatch.MaterialQuantity,
                    Designation = x.Dispatch.OrderLine.Designation,
                    MaterialUomId = x.Dispatch.OrderLine.MaterialUomId,
                    FreightUomId = x.Dispatch.OrderLine.FreightUomId,
                })
                .OrderBy(x => x.LoadTime)
                .ToListAsync();

            var pages = new List<DriverActivityDetailReportPageDto>();

            foreach (var employeeTime in employeeTimes)
            {
                var date = employeeTime.StartDateTime.ConvertTimeZoneTo(timezone).Date;
                var driver = drivers.FirstOrDefault(x => x.DriverId == employeeTime.DriverId);
                var page = pages.FirstOrDefault(x => x.Date == date && x.UserId == employeeTime.UserId);
                if (page == null)
                {
                    var driverId = driver?.DriverId ?? 0;
                    page = new DriverActivityDetailReportPageDto
                    {
                        Date = date,
                        DriverId = driverId,
                        DriverName = driver?.DriverName,
                        CarrierName = driver?.CarrierName,
                        UserId = employeeTime.UserId,
                        ScheduledStartTime = driverAssignments.FirstOrDefault(x => x.DriverId == driverId && x.Date == date)?.StartTimeUtc?.ConvertTimeZoneTo(timezone),
                        EmployeeTimes = new List<DriverActivityDetailReportEmployeeTimeDto>(),
                        Loads = new List<DriverActivityDetailReportLoadDto>(),
                    };
                    pages.Add(page);
                }
                page.EmployeeTimes.Add(new DriverActivityDetailReportEmployeeTimeDto
                {
                    TruckId = employeeTime.TruckId,
                    TruckCode = employeeTime.TruckCode,
                    ClockInTime = employeeTime.StartDateTime,
                    ClockOutTime = employeeTime.EndDateTime,
                    TimeClassificationName = employeeTime.TimeClassificationName,
                });
            }

            foreach (var load in loads)
            {
                var page = pages.FirstOrDefault(x => x.Date == load.Date && x.DriverId == load.DriverId);
                if (page == null)
                {
                    var driver = drivers.FirstOrDefault(x => x.DriverId == load.DriverId);
                    page = new DriverActivityDetailReportPageDto
                    {
                        Date = load.Date,
                        DriverId = load.DriverId ?? 0,
                        DriverName = driver?.DriverName,
                        CarrierName = driver?.CarrierName,
                        UserId = driver?.UserId ?? 0,
                        ScheduledStartTime = driverAssignments.FirstOrDefault(x => x.DriverId == load.DriverId && x.Date == load.Date)?.StartTimeUtc?.ConvertTimeZoneTo(timezone),
                        EmployeeTimes = new List<DriverActivityDetailReportEmployeeTimeDto>(),
                        Loads = new List<DriverActivityDetailReportLoadDto>(),
                    };
                    pages.Add(page);
                }

                foreach (var ticket in load.Tickets.DefaultIfEmpty())
                {
                    page.Loads.Add(new DriverActivityDetailReportLoadDto
                    {
                        TruckId = load.TruckId,
                        DispatchId = load.DispatchId,
                        OrderLineId = load.OrderLineId,
                        TruckCode = load.TruckCode,
                        CustomerName = load.CustomerName,
                        DeliverToName = load.DeliverToName,
                        DeliveryTime = load.DeliveryTime,
                        LoadAtName = load.LoadAtName,
                        LoadTime = load.LoadTime,
                        FreightQuantity = ticket?.FreightQuantity,
                        MaterialQuantity = ticket?.MaterialQuantity,
                        FreightUomName = ticket?.FreightUomName,
                        MaterialUomName = ticket?.MaterialUomName,
                        TrailerTruckCode = ticket?.TrailerTruckCode,
                        VehicleCategory = ticket?.VehicleCategory,
                        LoadTicket = ticket?.TicketNumber,
                        JobNumber = load.JobNumber,
                        FreightItemName = load.FreightItemName,
                        MaterialItemName = load.MaterialItemName,
                        FreightQuantityOrdered = load.FreightQuantityOrdered,
                        MaterialQuantityOrdered = load.MaterialQuantityOrdered,
                        Designation = load.Designation,
                        OrderLineMaterialUomId = load.MaterialUomId,
                        OrderLineFreightUomId = load.FreightUomId,
                        TicketUomId = ticket?.TicketUomId,
                    });
                }
            }

            pages = pages.OrderBy(p => p.Date).ThenBy(p => p.DriverName).ToList();

            foreach (var page in pages)
            {
                //page.Loads = page.Loads.OrderBy(x => x.LoadTime).ToList();

                for (int i = 0; i < page.Loads.Count - 1; i++)
                {
                    if (page.Loads[i + 1].LoadTime.HasValue && page.Loads[i].LoadTime.HasValue)
                    {
                        page.Loads[i].CycleTime = page.Loads[i + 1].LoadTime.Value - page.Loads[i].LoadTime.Value;
                    }
                }

                if (page.Loads.Any())
                {
                    var lastLoad = page.Loads.Last();
                    if (lastLoad.DeliveryTime.HasValue)
                    {
                        var employeeTime = page.EmployeeTimes
                            .Where(x => x.ClockOutTime > lastLoad.DeliveryTime)
                            //.Where(x => x.ClockInTime <= lastLoad.LoadTime && x.ClockOutTime > lastLoad.LoadTime);
                            .MinBy(x => x.ClockOutTime);
                        if (lastLoad.LoadTime.HasValue && employeeTime?.ClockOutTime.HasValue == true)
                        {
                            lastLoad.CycleTime = employeeTime.ClockOutTime.Value - lastLoad.LoadTime.Value;
                        }
                    }
                }
            }

            var reportGenerator = new DriverActivityDetailReportGenerator();
            return reportGenerator.GenerateReport(new DriverActivityDetailReportDto
            {
                Timezone = timezone,
                Pages = pages,
            });
        }
    }
}
