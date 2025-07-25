using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Extensions;
using DispatcherWeb.Authorization;
using DispatcherWeb.Common.Dto;
using DispatcherWeb.Dispatching.Dto;
using DispatcherWeb.Infrastructure.Extensions;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Dispatching
{
    [AbpAuthorize]
    public partial class DispatchingAppService
    {
        //This method gets called when dispatcher clicks Acknowledge/Load/Complete button on the first active dispatch for the driver
        //Previously, it was also used by an old Driver App
        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task<DispatchInfoBaseDto> GetDispatchInfo(GetDispatchInfoInput input)
        {
            var dispatch = await (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Id == input.Id)
                .FirstOrDefaultAsync();
            if (dispatch == null)
            {
                return new DriverInfoNotFoundDto();
            }

            if (!dispatch.Status.IsIn(DispatchStatus.Completed, DispatchStatus.Canceled))
            {
                var openDispatchId = await GetFirstOpenDispatchId(dispatch.DriverId);
                if (openDispatchId != null && openDispatchId != input.Id)
                {
                    return new DispatchInfoErrorAndRedirect
                    {
                        Message = L("CompleteExistingDispatchesBeforeFutureDispatchesWithUrl"),
                        UrlText = L("CapitalClickHere"),
                        RedirectUrl = GetAcknowledgementUrl(openDispatchId.Value),
                    };
                }
            }

            var driverInfoQuery = (await _dispatchRepository.GetQueryAsync())
                .Where(d => d.Id == input.Id)
                .Select(d => new
                {
                    d.Status,
                    CustomerName = d.OrderLine.Order.Customer.Name,
                    d.OrderLine.Order.DeliveryDate,
                    d.OrderLine.Order.Shift,
                    d.OrderLine.Order.ChargeTo,
                    d.OrderLine.Designation,
                    Item = d.OrderLine.FreightItem.Name,
                    d.MaterialQuantity,
                    d.FreightQuantity,
                    LoadAtName = d.OrderLine.LoadAt.Name,
                    LoadAtAddress = d.OrderLine.LoadAt == null ? null : new LocationAddressDto
                    {
                        StreetAddress = d.OrderLine.LoadAt.StreetAddress,
                        City = d.OrderLine.LoadAt.City,
                        State = d.OrderLine.LoadAt.State,
                        ZipCode = d.OrderLine.LoadAt.ZipCode,
                        CountryCode = d.OrderLine.LoadAt.CountryCode,
                    },
                    DeliverToName = d.OrderLine.DeliverTo.Name,
                    DeliverToAddress = d.OrderLine.DeliverTo == null ? null : new LocationAddressDto
                    {
                        StreetAddress = d.OrderLine.DeliverTo.StreetAddress,
                        City = d.OrderLine.DeliverTo.City,
                        State = d.OrderLine.DeliverTo.State,
                        ZipCode = d.OrderLine.DeliverTo.ZipCode,
                        CountryCode = d.OrderLine.DeliverTo.CountryCode,
                    },
                    LastLoad = d.Loads
                        .OrderByDescending(l => l.Id)
                        .Select(l => new DispatchCompleteInfoLoadDto
                        {
                            Id = l.Id,
                            SignatureId = l.SignatureId,
                            SourceDateTime = l.SourceDateTime,
                            LastTicket = l.Tickets
                                .OrderByDescending(t => t.Id)
                                .Select(t => new DispatchCompleteInfoTicketDto
                                {
                                    TicketNumber = t.TicketNumber,
                                    FreightQuantity = t.FreightQuantity,
                                    MaterialQuantity = t.MaterialQuantity,
                                    MaterialItemId = t.MaterialItemId,
                                    MaterialItemName = t.MaterialItem.Name,
                                    LoadCount = t.LoadCount,
                                    TicketPhotoId = t.TicketPhotoId,
                                })
                                .FirstOrDefault(),
                        })
                        .FirstOrDefault(),
                    RequireTicket = d.OrderLine.RequireTicket,
                    MaterialUomName = d.OrderLine.MaterialUom.Name,
                    FreightUomName = d.OrderLine.FreightUom.Name,
                    d.Note,
                    d.IsMultipleLoads,
                    d.WasMultipleLoads,
                });

            DispatchInfoDto driverInfoDto;

            switch (dispatch.Status)
            {
                case DispatchStatus.Created:
                case DispatchStatus.Sent:
                case DispatchStatus.Acknowledged:
                case DispatchStatus.Loaded when input.EditTicket:
                    var dto = await driverInfoQuery.Select(di => new DispatchLoadInfoDto
                    {
                        CustomerName = di.CustomerName,
                        Date = di.DeliveryDate,
                        Shift = di.Shift,
                        Item = di.Item,
                        Designation = di.Designation,
                        LoadAtName = di.LoadAtName,
                        LoadAt = di.LoadAtAddress,
                        ChargeTo = di.ChargeTo,
                        MaterialUomName = di.MaterialUomName,
                        FreightUomName = di.FreightUomName,
                        MaterialQuantity = di.MaterialQuantity,
                        FreightQuantity = di.FreightQuantity,
                        Note = di.Note,
                        IsMultipleLoads = di.IsMultipleLoads,
                        WasMultipleLoads = di.WasMultipleLoads,
                        LastLoad = di.LastLoad,
                        RequireTicket = di.RequireTicket,
                    })
                    .FirstAsync();

                    dto.VisibleTicketControls = await _ticketQuantityHelper.GetVisibleTicketControls(dispatch.OrderLineId);

                    var requiredTicketEntry = await SettingManager.GetRequiredTicketEntry();
                    dto.RequireTicket = IsTicketRequired(dto.RequireTicket, requiredTicketEntry);

                    driverInfoDto = dto;
                    break;
                case DispatchStatus.Loaded:
                    driverInfoDto = await driverInfoQuery.Select(di => new DispatchDestinationInfoDto
                    {
                        CustomerName = di.CustomerName,
                        DeliverToName = di.DeliverToName,
                        DeliverTo = di.DeliverToAddress,
                        Date = di.DeliveryDate,
                        Shift = di.Shift,
                        Note = di.Note,
                        SignatureId = di.LastLoad != null ? di.LastLoad.SignatureId : (Guid?)null,
                    })
                    .FirstAsync();
                    break;
                case DispatchStatus.Completed:
                    driverInfoDto = new DispatchInfoCompletedDto();
                    break;
                case DispatchStatus.Canceled:
                    driverInfoDto = new DispatchInfoCanceledDto();
                    break;
                default:
                    throw new ApplicationException($"Unexpected dispatch status: {dispatch.Status}");
            }
            driverInfoDto.DispatchStatus = dispatch.Status;
            driverInfoDto.DispatchId = dispatch.Id;
            driverInfoDto.Guid = dispatch.Guid;
            driverInfoDto.TenantId = dispatch.TenantId;
            driverInfoDto.IsMultipleLoads = dispatch.IsMultipleLoads;
            driverInfoDto.WasMultipleLoads = dispatch.WasMultipleLoads;

            return driverInfoDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task AcknowledgeDispatch(AcknowledgeDispatchInput input)
        {
            await AcknowledgeDispatchInternal(input);
        }

        //This gets called when the dispatcher clicks Loaded on a specific dispatch.
        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task LoadDispatch(LoadDispatchInput input)
        {
            await LoadDispatchInternal(input);
            await ModifyDispatchTicketInternal(input);
        }

        private string GetAcknowledgementUrl(int dispatchId)
        {
            string siteUrl = _webUrlService.GetSiteRootAddress();
            return $"{siteUrl}app/acknowledge/{dispatchId}";
        }

        //this is accessible from both Truck Dispatch List and Dispatcher driver app
        [AbpAuthorize(AppPermissions.Pages_Dispatches_Edit)]
        public async Task<CompleteDispatchResult> CompleteDispatch(CompleteDispatchDto input)
        {
            return await CompleteDispatchInternal(input);
        }
    }
}
