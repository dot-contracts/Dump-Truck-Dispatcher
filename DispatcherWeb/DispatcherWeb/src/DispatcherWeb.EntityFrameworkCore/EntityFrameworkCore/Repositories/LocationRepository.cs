using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.UI;
using DispatcherWeb.Locations;
using DispatcherWeb.Orders;
using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class LocationRepository : DispatcherWebRepositoryBase<Location>, ILocationRepository,
        IAsyncEventHandler<EntityCreatingEventData<Location>>,
        IAsyncEventHandler<EntityUpdatingEventData<Location>>
    {
        public LocationRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider)
          : base(dbContextProvider)
        {
        }

        public async Task MergeLocationsAsync(List<int> recordIds, int mainRecordId, int? tenantId)
        {
            recordIds.RemoveAll(x => x == mainRecordId);

            var allRecordIds = recordIds.Union(new[] { mainRecordId }).Distinct().ToList();
            var records = await (await GetQueryAsync())
                .Include(x => x.LocationContacts)
                .Where(x => allRecordIds.Contains(x.Id))
                .ToListAsync();

            var mainRecord = records.FirstOrDefault(x => x.Id == mainRecordId);
            records.RemoveAll(x => x.Id == mainRecordId);

            if (records.Any(x => x.PredefinedLocationKind != null))
            {
                throw new UserFriendlyException("You can't merge predefined Locations");
            }

            if (mainRecord == null || !records.Any())
            {
                return;
            }

            foreach (var record in records)
            {
                foreach (var contact in record.LocationContacts.ToList())
                {
                    record.LocationContacts.Remove(contact);
                    mainRecord.LocationContacts.Add(contact);
                    contact.LocationId = mainRecordId;
                }
                record.MergedToId = mainRecordId;
            }
            var context = await GetContextAsync();

            await context.SaveChangesAsync();

            await context.MergeEntitiesAsync(nameof(OrderLine), nameof(OrderLine.LoadAtId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(OrderLine), nameof(OrderLine.DeliverToId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ReceiptLine), nameof(OrderLine.LoadAtId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ReceiptLine), nameof(OrderLine.DeliverToId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Ticket), nameof(OrderLine.LoadAtId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Ticket), nameof(OrderLine.DeliverToId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(QuoteLine), nameof(QuoteLine.LoadAtId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(QuoteLine), nameof(QuoteLine.DeliverToId), tenantId, mainRecordId, allRecordIds);

            foreach (var record in records)
            {
                await DeleteAsync(record);
            }
        }

        public async Task MergeLocationContactsAsync(List<int> recordIds, int mainRecordId, int? tenantId)
        {
            var context = await GetContextAsync();

            recordIds.RemoveAll(x => x == mainRecordId);

            var allRecordIds = recordIds.Union(new[] { mainRecordId }).Distinct().ToList();
            var records = await context.LocationContacts
                .Where(x => allRecordIds.Contains(x.Id))
                .ToListAsync();

            var mainRecord = records.FirstOrDefault(x => x.Id == mainRecordId);
            records.RemoveAll(x => x.Id == mainRecordId);

            if (mainRecord == null || !records.Any())
            {
                return;
            }

            //var recordIdsString = string.Join(",", recordIds);
            //await Context.Database.ExecuteSqlCommandAsync($"Update dbo.[Order] set LocationContactId = {mainRecordId} where LocationContactId in ({recordIdsString})");
            //await Context.Database.ExecuteSqlCommandAsync($"Update dbo.Quote set LocationContactId = {mainRecordId} where LocationContactId in ({recordIdsString})");

            records.ForEach(x => x.MergedToId = mainRecordId);
            await context.SaveChangesAsync();

            foreach (var record in records)
            {
                context.LocationContacts.Remove(record);
            }
        }

        public Task HandleEventAsync(EntityCreatingEventData<Location> eventData)
        {
            UpdateLocationDisplayName(eventData.Entity);
            return Task.CompletedTask;
        }

        public Task HandleEventAsync(EntityUpdatingEventData<Location> eventData)
        {
            UpdateLocationDisplayName(eventData.Entity);
            return Task.CompletedTask;
        }

        public static void UpdateLocationDisplayName(Location location)
        {
            location.DisplayName = Utilities.ConcatenateAddress(
                location.Name,
                location.StreetAddress,
                location.City,
                location.State
            );
        }
    }
}
