using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Runtime.Session;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.JobSummary.Dto;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.JobSummary
{
    [AbpAuthorize]
    public class JobSummaryAppService : DispatcherWebAppServiceBase, IJobSummaryAppService
    {
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IJobSummaryRepository _jobSummaryRepository;

        public JobSummaryAppService(
            IRepository<OrderLine> orderLineRepository,
            IJobSummaryRepository jobSummaryRepository
        )
        {
            _orderLineRepository = orderLineRepository;
            _jobSummaryRepository = jobSummaryRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_ViewJobSummary)]
        public async Task<JobSummaryHeaderDetailsDto> GetJobSummaryHeaderDetails(int orderLineId)
        {
            var result = await (await _orderLineRepository.GetQueryAsync())
                .Where(ol => ol.Id == orderLineId)
                .Select(ol => new JobSummaryHeaderDetailsDto
                {
                    OrderLineId = ol.Id,
                    Customer = ol.Order.Customer.Name,
                    JobNumber = ol.JobNumber,
                    NumberOfTrucks = ol.NumberOfTrucks,
                    DeliveryDate = ol.Order.DeliveryDate,
                    MaterialQuantity = ol.MaterialQuantity,
                    FreightQuantity = ol.FreightQuantity,
                    MaterialUomName = ol.MaterialUom.Name,
                    FreightUomName = ol.FreightUom.Name,
                    Designation = ol.Designation,
                    Item = ol.FreightItem.Name,
                    LoadAt = ol.LoadAt.DisplayName,
                    DeliverTo = ol.DeliverTo.DisplayName,
                    IsComplete = ol.IsComplete,
                    Dispatches = ol.Dispatches.Select(d => new JobSummaryDispatchDto
                    {
                        Status = d.Status,
                    }).ToList(),
                }).FirstAsync();

            result.JobStatus = GetJobStatus(result);

            return result;
        }

        private static JobStatus GetJobStatus(JobSummaryHeaderDetailsDto orderLine)
        {
            var dispatches = orderLine.Dispatches;

            var isCompleted = orderLine.IsComplete
                || dispatches.All(d => Dispatch.ClosedDispatchStatuses.Contains(d.Status))
                && dispatches.Any(d => d.Status == DispatchStatus.Completed);

            var isInProgress = dispatches.Any(d => d.Status.IsIn(DispatchStatus.Acknowledged, DispatchStatus.Loaded, DispatchStatus.Completed));

            if (isCompleted)
            {
                return JobStatus.Completed;
            }

            if (isInProgress)
            {
                return JobStatus.InProgress;
            }

            return JobStatus.Scheduled;
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_ViewJobSummary)]
        public async Task<OrderTrucksDto> GetJobSummaryLoads(int orderLineId)
        {
            var orderTrucksDto = new OrderTrucksDto();
            var orderTrucksLoadCycles = await _jobSummaryRepository.GetOrderTrucksLoadJobTripCycles(await AbpSession.GetTenantIdAsync(), orderLineId);
            if (!orderTrucksLoadCycles.Any(x => x.TripStart.HasValue))
            {
                return orderTrucksDto;
            }

            var earliest = orderTrucksLoadCycles.Where(p => p.TripStart.HasValue).Min(p => p.TripStart.Value);
            var latest = orderTrucksLoadCycles.Where(p => p.TripEnd.HasValue).Max(p => p.TripEnd) ?? earliest;
            var minutes = latest.Subtract(earliest).TotalMinutes * .25;
            orderTrucksDto.Earliest = earliest.AddMinutes(-1 * minutes);
            orderTrucksDto.Latest = latest.AddMinutes(minutes);

            var groupedLoads = orderTrucksLoadCycles.GroupBy(p => new { p.TruckId }).ToList();

            groupedLoads.ForEach(cycles =>
            {
                var orderTruckDto = new OrderTruckDto
                {
                    TruckId = cycles.Key.TruckId,
                    TruckCode = cycles.First().TruckCode,
                    LoadCount = cycles.Count(c => c.TripType == TruckTripTypes.ToLoadSite),
                    Quantity = cycles.Where(c => c.TripType == TruckTripTypes.ToLoadSite).Sum(c => c.TicketQuantity),
                    UnitOfMeasure = cycles.First().TicketUom,
                };

                foreach (var cycle in cycles.Where(x => x.TripStart.HasValue).OrderBy(p => p.LoadId))
                {
                    var tripCycle = new TripCycleDto
                    {
                        LoadId = cycle.LoadId,
                        DriverId = cycle.DriverId,
                        DriverName = cycle.DriverName,
                        Location = cycle.LocationNameFormatted,
                        Label = $"#{orderTruckDto.TripCycles.Count + 1} {(cycle.TripType == TruckTripTypes.ToLoadSite ? "Load at" : "Deliver to")} {cycle.LocationNameFormatted}",
                        TruckTripType = cycle.TripType,
                        StartDateTime = cycle.TripStart,
                        EndDateTime = cycle.TripEnd,
                        SegmentHoverText = cycle.TripDuration.Format(),
                        SourceLatitude = cycle.SourceLatitude,
                        SourceLongitude = cycle.SourceLongitude,
                        TicketId = cycle.TicketId,
                        Quantity = cycle.TicketQuantity,
                    };

                    orderTruckDto.TripCycles.Add(tripCycle);
                }

                orderTrucksDto.OrderTrucks.Add(orderTruckDto);
            });

            return orderTrucksDto;
        }
    }
}
