using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Charges.Dto;
using DispatcherWeb.Orders;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Charges
{
    [AbpAuthorize]
    public class ChargeAppService : DispatcherWebAppServiceBase, IChargeAppService
    {
        private readonly IRepository<Charge> _chargeRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;

        public ChargeAppService(
            IRepository<Charge> chargeRepository,
            IRepository<OrderLine> orderLineRepository
        )
        {
            _chargeRepository = chargeRepository;
            _orderLineRepository = orderLineRepository;
        }

        [AbpAuthorize(AppPermissions.Pages_Charges)]
        public async Task<List<ChargeEditDto>> GetChargesForOrderLine(GetChargesForOrderLineInput input)
        {
            var items = await (await _chargeRepository.GetQueryAsync())
                .Where(x => x.OrderLineId == input.OrderLineId)
                .Select(x => new ChargeEditDto
                {
                    Id = x.Id,
                    ItemId = x.ItemId,
                    ItemName = x.Item.Name,
                    UnitOfMeasureId = x.UnitOfMeasureId,
                    UnitOfMeasureName = x.UnitOfMeasure.Name,
                    Description = x.Description,
                    Rate = x.Rate,
                    Quantity = x.Quantity,
                    UseMaterialQuantity = x.UseMaterialQuantity,
                    ChargeAmount = x.ChargeAmount,
                    OrderLineId = x.OrderLineId,
                    HasInvoiceLines = x.InvoiceLines.Any(),
                    IsBilled = x.IsBilled,
                    ChargeDate = x.ChargeDate,
                }).ToListAsync();

            return items;
        }

        [AbpAuthorize(AppPermissions.Pages_Charges)]
        public async Task<ChargeEditDto> EditCharge(ChargeEditDto model)
        {
            if (model.Id != 0)
            {
                if (await IsChargeReadonly(model.Id))
                {
                    throw new UserFriendlyException(L("CannotEditBilledOrInvoicedCharge"));
                }
            }

            var entity = model.Id == 0 ? new Charge() : await _chargeRepository.GetAsync(model.Id);

            if (entity.UseMaterialQuantity)
            {
                model.Quantity = null;
            }

            entity.ItemId = model.ItemId;
            entity.UnitOfMeasureId = model.UnitOfMeasureId;
            entity.Description = model.Description;
            entity.Rate = model.Rate;
            entity.Quantity = model.Quantity;
            entity.UseMaterialQuantity = model.UseMaterialQuantity;
            entity.ChargeAmount = model.Rate * (model.Quantity ?? 0);
            entity.OrderLineId = model.OrderLineId;
            entity.IsBilled = model.IsBilled;
            if (model.Id == 0)
            {
                var orderDetails = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.Id == model.OrderLineId)
                    .Select(x => new
                    {
                        x.Order.DeliveryDate,
                    }).FirstOrDefaultAsync();

                if (orderDetails == null)
                {
                    throw new UserFriendlyException("Order wasn't found. Please reload the page and try again.");
                }

                entity.ChargeDate = orderDetails.DeliveryDate;
                model.ChargeDate = entity.ChargeDate;
            }

            if (entity.Id == 0)
            {
                model.Id = await _chargeRepository.InsertAndGetIdAsync(entity);
            }

            return model;
        }

        [AbpAuthorize(AppPermissions.Pages_Charges)]
        public async Task DeleteCharge(EntityDto model)
        {
            if (await IsChargeReadonly(model.Id))
            {
                throw new UserFriendlyException(L("CannotDeleteBilledOrInvoicedCharge"));
            }

            await _chargeRepository.DeleteAsync(model.Id);
        }

        private async Task<bool> IsChargeReadonly(int id)
        {
            var charge = await (await _chargeRepository.GetQueryAsync())
                .Where(x => x.Id == id)
                .Select(x => new
                {
                    IsBilled = x.IsBilled,
                    HasInvoiceLines = x.InvoiceLines.Any(),
                }).FirstOrDefaultAsync();

            if (charge == null)
            {
                throw new UserFriendlyException("Charge wasn't found. Please reload the page and try again.");
            }

            return charge.IsBilled
                   || charge.HasInvoiceLines;
        }

        [AbpAuthorize(AppPermissions.Pages_Charges)]
        public async Task<ChargeOrderLineDetailsDto> GetChargeOrderLineDetails(GetChargeOrderLineDetailsInput input)
        {
            var orderLine = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == input.OrderLineId)
                .Select(x => new ChargeOrderLineDetailsDto
                {
                    DeliveryDate = x.Order.DeliveryDate,
                    CustomerName = x.Order.Customer.Name,
                    ItemName = x.FreightItem.Name,
                    MaterialItemName = x.MaterialItem.Name,
                    LoadAtName = x.LoadAt.DisplayName,
                    DeliverToName = x.DeliverTo.DisplayName,
                }).FirstOrDefaultAsync();

            if (orderLine == null)
            {
                throw new UserFriendlyException("Order line wasn't found. Please reload the page and try again.");
            }

            return orderLine;
        }
    }
}
