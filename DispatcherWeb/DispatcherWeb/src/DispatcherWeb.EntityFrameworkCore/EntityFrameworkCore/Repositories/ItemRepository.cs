using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.EntityFrameworkCore;
using DispatcherWeb.Items;
using DispatcherWeb.Orders;
using DispatcherWeb.Quotes;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.EntityFrameworkCore.Repositories
{
    public class ItemRepository : DispatcherWebRepositoryBase<Item>, IItemRepository
    {
        public ItemRepository(IDbContextProvider<DispatcherWebDbContext> dbContextProvider)
            : base(dbContextProvider)
        {
        }

        public async Task MergeItemsAsync(List<int> recordIds, int mainRecordId, int? tenantId)
        {
            recordIds.RemoveAll(x => x == mainRecordId);

            var allRecordIds = recordIds.Union(new[] { mainRecordId }).Distinct().ToList();
            var records = await (await GetQueryAsync())
                .Include(x => x.OfficeItemPrices)
                .Include(x => x.ProductLocations)
                .Include(x => x.HaulingCategories)
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
                foreach (var price in record.OfficeItemPrices.ToList())
                {
                    if (!mainRecord.OfficeItemPrices.Any(x => x.MaterialUomId == price.MaterialUomId
                                                                && x.FreightUomId == price.FreightUomId
                                                                && x.OfficeId == price.OfficeId
                                                                //&& x.DesignationId == price.DesignationId
                                                                ))
                    {
                        record.OfficeItemPrices.Remove(price);
                        mainRecord.OfficeItemPrices.Add(price);
                        price.ItemId = mainRecordId;
                    }
                }
                record.MergedToId = mainRecordId;
            }

            var context = await GetContextAsync();

            await context.SaveChangesAsync();

            await context.MergeEntitiesAsync(nameof(OrderLine), nameof(OrderLine.FreightItemId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(OrderLine), nameof(OrderLine.MaterialItemId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ReceiptLine), nameof(ReceiptLine.FreightItemId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(ReceiptLine), nameof(ReceiptLine.MaterialItemId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(QuoteLine), nameof(QuoteLine.FreightItemId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(QuoteLine), nameof(QuoteLine.MaterialItemId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Ticket), nameof(Ticket.FreightItemId), tenantId, mainRecordId, allRecordIds);
            await context.MergeEntitiesAsync(nameof(Ticket), nameof(Ticket.MaterialItemId), tenantId, mainRecordId, allRecordIds);
            //await context.MergeEntitiesAsync(nameof(OfficeItemPrice), nameof(OfficeItemPrice.ItemId), tenantId, mainRecordId, allRecordIds);


            foreach (var record in records)
            {
                foreach (var price in record.OfficeItemPrices.ToList())
                {
                    context.OfficeItemPrices.Remove(price);
                }
                context.Items.Remove(record);

                foreach (var productLocation in record.ProductLocations.ToList())
                {
                    var duplicates = mainRecord.ProductLocations
                                 .Where(ls => ls.LocationId == productLocation.LocationId && ls.UnitOfMeasureId == productLocation.UnitOfMeasureId)
                                 .ToList();

                    if (duplicates.Any())
                    {
                        context.ProductLocations.Remove(productLocation);
                    }

                    productLocation.ItemId = mainRecordId;
                    context.ProductLocations.Update(productLocation);
                }

                foreach (var haulingCategory in record.HaulingCategories.ToList())
                {
                    var duplicates = mainRecord.HaulingCategories
                                 .Where(ls => ls.TruckCategoryId == haulingCategory.TruckCategoryId && ls.UnitOfMeasureId == haulingCategory.UnitOfMeasureId)
                                 .ToList();

                    if (duplicates.Any())
                    {
                        context.HaulingCategories.Remove(haulingCategory);
                    }

                    haulingCategory.ItemId = mainRecordId;
                    context.HaulingCategories.Update(haulingCategory);
                }

            }
        }

        public async Task MigrateToSeparateMaterialAndFreightItems(List<int> tenantIds, bool separateMaterialAndFreightItems)
        {
            //the method does not accept anything other than a list of ints and a bool, so it's less likely to be vulnerable to SQL injection
#pragma warning disable EF1002 // Risk of vulnerability to SQL injection.
            var context = await GetContextAsync();
            var tenantIdsString = string.Join(", ", tenantIds);
            var tables = new[]
            {
                nameof(OrderLine),
                nameof(ReceiptLine),
                nameof(QuoteLine),
            };

            if (separateMaterialAndFreightItems)
            {
                foreach (var table in tables)
                {
                    await context.Database.ExecuteSqlRawAsync($"Update [{table}] set MaterialItemId = FreightItemId, FreightItemId = null where TenantId in ({tenantIdsString}) and Designation = {(int)DesignationEnum.MaterialOnly} and FreightItemId is not null");
                }
            }
            else
            {
                foreach (var table in tables)
                {
                    await context.Database.ExecuteSqlRawAsync($"Update [{table}] set FreightItemId = MaterialItemId, MaterialItemId = null where TenantId in ({tenantIdsString}) and Designation = {(int)DesignationEnum.MaterialOnly} and MaterialItemId is not null");
                }
            }
#pragma warning restore EF1002 // Risk of vulnerability to SQL injection.
        }
    }
}
