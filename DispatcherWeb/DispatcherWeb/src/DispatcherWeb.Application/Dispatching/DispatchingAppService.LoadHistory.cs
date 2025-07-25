using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Linq.Extensions;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Dispatching
{
    [AbpAuthorize]
    public partial class DispatchingAppService
    {
        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        [HttpPost]
        public async Task<PagedResultDto<DispatchListDto>> GetDispatchPagedList(GetDispatchPagedListInput input)
        {
            var query = await GetFilteredDispatchQueryAsync(input);

            var totalCount = await query.CountAsync();

            var rawItems = await query
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            var items = GetDispatchDtoList(rawItems);

            return new PagedResultDto<DispatchListDto>(totalCount, items);
        }

        private async Task<IQueryable<RawDispatchDto>> GetFilteredDispatchQueryAsync(IGetDispatchListFilter input)
        {
            var filteredQuery = (await _dispatchRepository.GetQueryAsync())
                    .Where(d => d.Status != DispatchStatus.Canceled || d.Loads.Any())
                    .WhereIf(input.OfficeId.HasValue, d => d.OrderLine.Order.OfficeId == input.OfficeId.Value)
                    .WhereIf(input.DateBegin.HasValue, d => d.OrderLine.Order.DeliveryDate >= input.DateBegin.Value)
                    .WhereIf(input.DateEnd.HasValue, d => d.OrderLine.Order.DeliveryDate < input.DateEnd.Value.AddDays(1))
                    .WhereIf(!input.TruckIds.IsNullOrEmpty(), d => input.TruckIds.Contains(d.TruckId))
                    .WhereIf(!input.DriverIds.IsNullOrEmpty(), d => input.DriverIds.Contains(d.DriverId))
                    .WhereIf(!input.Statuses.IsNullOrEmpty(), d => input.Statuses.Contains(d.Status))
                    .WhereIf(input.CustomerId.HasValue, d => d.OrderLine.Order.CustomerId == input.CustomerId.Value)
                    .WhereIf(input.OrderLineId.HasValue, d => d.OrderLineId == input.OrderLineId);

            var query = filteredQuery.ToRawDispatchDto();

            return query
                .WhereIf(input.MissingTickets, d => d.Status == DispatchStatus.Completed && (d.FilledTicketCount == null || d.FilledTicketCount == 0));

        }

        [RemoteService(false)]
        public static IQueryable<RawDispatchDto> ToRawDispatchDto(IQueryable<Dispatch> query)
        {
            return
                from d in query
                from l in d.Loads.DefaultIfEmpty()
                select new RawDispatchDto
                {
                    Id = d.Id,
                    DriverId = d.DriverId,
                    TruckCode = d.Truck.TruckCode,
                    DriverLastFirstName = d.Driver.LastName + ", " + d.Driver.FirstName,
                    Sent = d.Sent,
                    Acknowledged = d.Acknowledged,
                    Loaded = l.SourceDateTime,
                    Delivered = l.DestinationDateTime,
                    Status = d.Status,
                    CustomerName = d.OrderLine.Order.Customer.Name,
                    QuoteName = d.OrderLine.Order.Quote.Name,
                    JobNumber = d.OrderLine.JobNumber,
                    LoadAtName = d.OrderLine.LoadAt.DisplayName,
                    DeliverToName = d.OrderLine.DeliverTo.DisplayName,
                    Item = d.OrderLine.FreightItem.Name,
                    Guid = d.Guid,
                    IsMultipleLoads = d.IsMultipleLoads,
                    FilledTicketCount = l.Tickets.Count(t => (t.MaterialQuantity ?? 0) > 0),
                };
        }

        private List<DispatchListDto> GetDispatchDtoList(List<RawDispatchDto> rawItems)
        {
            return rawItems.Select(d => new DispatchListDto
            {
                Id = d.Id,
                TruckCode = d.TruckCode,
                DriverLastFirstName = d.DriverLastFirstName,
                Sent = d.Sent,
                Acknowledged = d.Acknowledged,
                Loaded = d.Loaded,
                Delivered = d.Delivered,
                Status = d.Status.GetDisplayName(),
                DispatchStatus = d.Status,
                CustomerName = d.CustomerName,
                QuoteName = d.QuoteName,
                JobNumber = d.JobNumber,
                LoadAtName = d.LoadAtName,
                DeliverToName = d.DeliverToName,
                Item = d.Item,
                Cancelable = d.Status != DispatchStatus.Completed && d.Status != DispatchStatus.Canceled,
                Guid = d.Guid,
                IsMultipleLoads = d.IsMultipleLoads,
            }).ToList();
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches)]
        [HttpPost]
        public async Task<FileDto> GetDispatchesToCsv(GetDispatchesToCsvInput input)
        {
            var timezone = await GetTimezone();
            var query = await GetFilteredDispatchQueryAsync(input);

            var rawItems = await query
                .OrderBy(input.Sorting)
                .ToListAsync();
            var items = GetDispatchDtoList(rawItems);
            if (items.Count == 0)
            {
                throw new UserFriendlyException("There is no data to export!");
            }
            Debug.Assert(AbpSession.UserId != null, "AbpSession.UserId != null");
            items.ForEach(item =>
            {
                item.Sent = item.Sent?.ConvertTimeZoneTo(timezone);
                item.Acknowledged = item.Acknowledged?.ConvertTimeZoneTo(timezone);
                item.Loaded = item.Loaded?.ConvertTimeZoneTo(timezone);
                item.Delivered = item.Delivered?.ConvertTimeZoneTo(timezone);
            });

            return await _dispatchListCsvExporter.ExportToFileAsync(items);
        }
    }
}
