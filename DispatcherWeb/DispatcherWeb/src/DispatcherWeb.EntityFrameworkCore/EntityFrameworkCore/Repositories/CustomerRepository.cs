using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using DispatcherWeb.Customers;
using DispatcherWeb.Invoices;
using DispatcherWeb.Orders;
using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class CustomerRepository : DispatcherWebRepositoryBase<Customer>, ICustomerRepository
    {
        public CustomerRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task MergeCustomersAsync(List<int> recordIds, int mainRecordId, int? tenantId)
        {
            recordIds.RemoveAll(x => x == mainRecordId);

            var allRecordIds = recordIds.Union(new[] { mainRecordId }).Distinct().ToList();
            var records = await (await GetQueryAsync())
                .Include(x => x.CustomerContacts)
                .Where(x => allRecordIds.Contains(x.Id))
                .ToListAsync();

            var mainRecord = records.FirstOrDefault(x => x.Id == mainRecordId);
            records.RemoveAll(x => x.Id == mainRecordId);

            if (mainRecord == null || !records.Any())
            {
                return;
            }

            foreach (var record in records)
            {
                foreach (var contact in record.CustomerContacts.ToList())
                {
                    record.CustomerContacts.Remove(contact);
                    mainRecord.CustomerContacts.Add(contact);
                    contact.CustomerId = mainRecordId;
                }
                record.MergedToId = mainRecordId;
            }

            var context = await GetContextAsync();

            await context.SaveChangesAsync();

            await context.MergeEntitiesAsync(nameof(Order), nameof(Order.CustomerId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Quote), nameof(Quote.CustomerId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Ticket), nameof(Ticket.CustomerId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Invoice), nameof(Invoice.CustomerId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Receipt), nameof(Receipt.CustomerId), tenantId, mainRecordId, allRecordIds);

            foreach (var record in records)
            {
                await DeleteAsync(record);
            }
        }

        public async Task MergeCustomerContactsAsync(List<int> recordIds, int mainRecordId, int? tenantId)
        {
            var context = await GetContextAsync();

            recordIds.RemoveAll(x => x == mainRecordId);

            var allRecordIds = recordIds.Union(new[] { mainRecordId }).Distinct().ToList();
            var records = await context.CustomerContacts
                .Where(x => allRecordIds.Contains(x.Id))
                .ToListAsync();

            var mainRecord = records.FirstOrDefault(x => x.Id == mainRecordId);
            records.RemoveAll(x => x.Id == mainRecordId);

            if (mainRecord == null || !records.Any())
            {
                return;
            }

            await context.MergeEntitiesAsync(nameof(Order), nameof(Order.ContactId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Quote), nameof(Quote.ContactId), tenantId, mainRecordId, allRecordIds);

            foreach (var record in records)
            {
                context.CustomerContacts.Remove(record);
            }
        }

    }
}
