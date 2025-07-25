using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Extensions;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Items;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    public class ImportItemsAppService : ImportDataBaseAppService<ItemImportRow>, IImportItemsAppService
    {
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<OfficeItemPrice> _officeItemPriceRepository;
        private readonly IRepository<UnitOfMeasure> _uomRepository;
        private Dictionary<string, int> _uomCache = null;
        private int? _officeId = null;

        public ImportItemsAppService(
            IRepository<Item> itemRepository,
            IRepository<OfficeItemPrice> officeItemPriceRepository,
            IRepository<UnitOfMeasure> uomRepository
        )
        {
            _itemRepository = itemRepository;
            _officeItemPriceRepository = officeItemPriceRepository;
            _uomRepository = uomRepository;
        }

        protected override async Task<bool> CacheResourcesBeforeImportAsync(IImportReader reader)
        {
            _uomCache = await (await _uomRepository.GetQueryAsync())
                .Select(x => new { x.Id, x.Name })
                .ToDictionaryAsync(x => x.Name, x => x.Id);

            _officeId = await OfficeResolver.GetOfficeIdAsync(_userId.ToString());
            if (_officeId == null)
            {
                _result.NotFoundOffices.Add(_userId.ToString());
                return false;
            }

            return await base.CacheResourcesBeforeImportAsync(reader);
        }

        protected override async Task<bool> ImportRowAsync(ItemImportRow row)
        {
            var itemType = ParseItemType(row);
            if (itemType == null)
            {
                return false;
            }

            var item = await (await _itemRepository.GetQueryAsync())
                .Include(x => x.OfficeItemPrices)
                .Where(x => x.Name == row.Name)
                .FirstOrDefaultAsync();

            if (item == null)
            {
                item = new Item
                {
                    Name = row.Name,
                };
                await _itemRepository.InsertAsync(item);
            }

            item.IsActive = row.IsActive;
            item.Description = row.Description;
            item.Type = itemType;
            item.IsTaxable = row.IsTaxable;
            item.IncomeAccount = row.IncomeAccount;
            item.IsInQuickBooks = true;

            if (row.Price != null && _officeId.HasValue && ItemTypeHasPricing(itemType))
            {
                var uomId = ParseUomId(row);
                OfficeItemPrice officeItemPrice = null;
                if (item.Id > 0)
                {
                    switch (itemType)
                    {
                        case ItemType.Service:
                        case ItemType.OtherCharge:
                            officeItemPrice = item?.OfficeItemPrices.FirstOrDefault(x => x.OfficeId == _officeId.Value && x.FreightUomId == uomId && x.Designation == DesignationEnum.FreightOnly);
                            break;
                        case ItemType.InventoryPart:
                        case ItemType.NonInventoryPart:
                            officeItemPrice = item?.OfficeItemPrices.FirstOrDefault(x => x.OfficeId == _officeId.Value && x.MaterialUomId == uomId && x.Designation == DesignationEnum.MaterialOnly);
                            break;
                        case ItemType.Discount:
                        case ItemType.Payment:
                        case ItemType.SalesTaxItem:
                            break;
                    }
                }
                if (officeItemPrice == null)
                {
                    officeItemPrice = new OfficeItemPrice
                    {
                        Item = item,
                        OfficeId = _officeId.Value,
                    };
                    await _officeItemPriceRepository.InsertAsync(officeItemPrice);
                }

                switch (itemType)
                {
                    case ItemType.Service:
                    case ItemType.OtherCharge:
                        officeItemPrice.FreightRate = row.Price;
                        officeItemPrice.Designation = DesignationEnum.FreightOnly;
                        officeItemPrice.FreightUomId = uomId;
                        break;
                    case ItemType.InventoryPart:
                    case ItemType.NonInventoryPart:
                        officeItemPrice.PricePerUnit = row.Price;
                        officeItemPrice.Designation = DesignationEnum.MaterialOnly;
                        officeItemPrice.MaterialUomId = uomId;
                        break;
                    case ItemType.Discount:
                    case ItemType.Payment:
                    case ItemType.SalesTaxItem:
                        break;
                }
            }

            return true;
        }

        private bool ItemTypeHasPricing(ItemType? itemType)
        {
            switch (itemType)
            {
                default:
                case ItemType.Discount:
                case ItemType.Payment:
                case ItemType.SalesTaxItem:
                    return false;
                case ItemType.Service:
                case ItemType.OtherCharge:
                case ItemType.InventoryPart:
                case ItemType.NonInventoryPart:
                    return true;
            }
        }

        private ItemType? ParseItemType(ItemImportRow row)
        {
            var typeString = row.Type;

            if (Utilities.TryGetEnumFromDisplayName<ItemType>(typeString, out var type))
            {
                return type;
            }

            row.AddParseErrorIfNotExist("Type", row.Type, typeof(string));
            return null;
        }

        private int? ParseUomId(ItemImportRow row)
        {
            if (_uomCache?.Keys.Any() != true)
            {
                return null;
            }

            if (!row.Uom.IsNullOrEmpty())
            {
                foreach (var uom in _uomCache.Keys)
                {
                    if (row.Uom.ToLower().Contains(uom.ToLower().TrimEnd('s')))
                    {
                        return _uomCache[uom];
                    }
                }
                row.AddParseErrorIfNotExist("UOM", row.Uom, typeof(string));
            }

            return _uomCache.First().Value;
        }

        protected override bool IsRowEmpty(ItemImportRow row)
        {
            return row.Name.IsNullOrEmpty();
        }
    }
}
