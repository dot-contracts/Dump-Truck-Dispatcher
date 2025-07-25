using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Net.Mail;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.Net.Mail;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.Emailing;
using DispatcherWeb.Features;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Quotes.Dto;
using DispatcherWeb.Storage;
using DispatcherWeb.Tickets;
using DispatcherWeb.Url;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Quotes
{
    [AbpAuthorize]
    public class QuoteAppService : DispatcherWebAppServiceBase, IQuoteAppService
    {
        private readonly IRepository<Quote> _quoteRepository;
        private readonly IRepository<QuoteLine> _quoteLineRepository;
        private readonly IRepository<QuoteHistoryRecord> _quoteHistoryRepository;
        private readonly IRepository<QuoteFieldDiff> _quoteFieldDiffRepository;
        private readonly IRepository<QuoteEmail> _quoteEmailRepository;
        private readonly IRepository<QuoteLineVehicleCategory> _quoteLineVehicleCategoryRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<User, long> _userRepository;
        private readonly IRepository<FuelSurchargeCalculation> _fuelSurchargeCalculationRepository;
        private readonly IRepository<TrackableEmail, Guid> _trackableEmailRepository;
        private readonly IRepository<TrackableEmailEvent> _trackableEmailEventRepository;
        private readonly IRepository<TrackableEmailReceiver> _trackableEmailReceiverRepository;
        private readonly IBinaryObjectManager _binaryObjectManager;
        private readonly IAppFolders _appFolders;
        private readonly IEmailSender _emailSender;
        private readonly ITrackableEmailSender _trackableEmailSender;
        private readonly IWebUrlService _webUrlService;
        private readonly ISingleOfficeAppService _singleOfficeService;
        private readonly ILogoProvider _logoProvider;

        public QuoteAppService(
            IRepository<Quote> quoteRepository,
            IRepository<QuoteLine> quoteLineRepository,
            IRepository<QuoteHistoryRecord> quoteHistoryRepository,
            IRepository<QuoteFieldDiff> quoteFieldDiffRepository,
            IRepository<QuoteEmail> quoteEmailRepository,
            IRepository<QuoteLineVehicleCategory> quoteLineVehicleCategoryRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<User, long> userRepository,
            IRepository<FuelSurchargeCalculation> fuelSurchargeCalculationRepository,
            IRepository<TrackableEmail, Guid> trackableEmailRepository,
            IRepository<TrackableEmailEvent> trackableEmailEventRepository,
            IRepository<TrackableEmailReceiver> trackableEmailReceiverRepository,
            IBinaryObjectManager binaryObjectManager,
            IAppFolders appFolders,
            IEmailSender emailSender,
            ITrackableEmailSender trackableEmailSender,
            IWebUrlService webUrlService,
            ISingleOfficeAppService singleOfficeService,
            ILogoProvider logoProvider
            )
        {
            _quoteRepository = quoteRepository;
            _quoteLineRepository = quoteLineRepository;
            _quoteHistoryRepository = quoteHistoryRepository;
            _quoteFieldDiffRepository = quoteFieldDiffRepository;
            _quoteEmailRepository = quoteEmailRepository;
            _quoteLineVehicleCategoryRepository = quoteLineVehicleCategoryRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _userRepository = userRepository;
            _fuelSurchargeCalculationRepository = fuelSurchargeCalculationRepository;
            _trackableEmailRepository = trackableEmailRepository;
            _trackableEmailEventRepository = trackableEmailEventRepository;
            _trackableEmailReceiverRepository = trackableEmailReceiverRepository;
            _binaryObjectManager = binaryObjectManager;
            _appFolders = appFolders;
            _emailSender = emailSender;
            _trackableEmailSender = trackableEmailSender;
            _webUrlService = webUrlService;
            _singleOfficeService = singleOfficeService;
            _logoProvider = logoProvider;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<PagedResultDto<QuoteDto>> GetQuotes(GetQuotesInput input)
        {
            var query = (await _quoteRepository.GetQueryAsync())
                .WhereIf(input.QuoteId.HasValue,
                    x => x.Id == input.QuoteId)
                .WhereIf(input.CustomerId.HasValue,
                    x => x.CustomerId == input.CustomerId)
                .WhereIf(input.SalesPersonId.HasValue,
                    x => x.SalesPersonId == input.SalesPersonId)
                .WhereIf(input.Status >= 0,
                    x => x.Status == input.Status)
                .WhereIf(!string.IsNullOrEmpty(input.Misc),
                    x => x.Name.Contains(input.Misc)
                         || x.Description.Contains(input.Misc)
                         || x.Directions.Contains(input.Misc)
                     );

            var totalCount = await query.CountAsync();

            var items = await query
                .Select(x => new QuoteDto
                {
                    Id = x.Id,
                    QuoteName = x.Name,
                    Description = x.Description,
                    CustomerName = x.Customer.Name,
                    QuoteDate = x.ProposalDate,
                    ContactName = x.Contact.Name,
                    SalesPersonName = x.SalesPerson.Name + " " + x.SalesPerson.Surname,
                    PONumber = x.PONumber,
                    EmailDeliveryStatuses = x.QuoteEmails.Select(y => y.Email.CalculatedDeliveryStatus).ToList(),
                    Status = x.Status,
                })
                .OrderBy(input.Sorting)
                .PageBy(input)
                .ToListAsync();

            return new PagedResultDto<QuoteDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Orders_View, AppPermissions.Pages_Quotes_View)]
        public async Task<ListResultDto<SelectListDto>> GetQuotesForCustomer(GetQuotesForCustomerInput input)
        {
            if (input.Id == null)
            {
                return new ListResultDto<SelectListDto>();
            }
            var quotes = await (await _quoteRepository.GetQueryAsync())
                .Where(x => x.CustomerId == input.Id)
                .WhereIf(input.HideInactive, x => x.Status != QuoteStatus.Inactive)
                .OrderBy(x => x.Name)
                .Select(x => new SelectListDto<QuoteSelectListInfoDto>
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                    Item = new QuoteSelectListInfoDto
                    {
                        ContactId = x.ContactId,
                        Directions = x.Directions,
                        PONumber = x.PONumber,
                        CustomerIsTaxExempt = x.Customer.IsTaxExempt,
                        QuoteIsTaxExempt = x.IsTaxExempt,
                        SpectrumNumber = x.SpectrumNumber,
                        Status = x.Status,
                        CustomerId = x.CustomerId,
                        OfficeId = x.OfficeId,
                        OfficeName = x.Office.Name,
                        ChargeTo = x.ChargeTo,
                        FuelSurchargeCalculationId = x.FuelSurchargeCalculationId,
                        FuelSurchargeCalculationName = x.FuelSurchargeCalculation.Name,
                        BaseFuelCost = x.BaseFuelCost,
                        CanChangeBaseFuelCost = x.FuelSurchargeCalculation.CanChangeBaseFuelCost,
                    },
                })
                .ToListAsync();
            return new ListResultDto<SelectListDto>(quotes);
        }

        [AbpAuthorize(AppPermissions.Pages_Misc_SelectLists_QuoteSalesreps)]
        public async Task<PagedResultDto<SelectListDto>> GetQuoteSalesrepSelectList(GetSelectListInput input)
        {
            var query = (await _quoteRepository.GetQueryAsync())
                .Select(x => x.SalesPerson)
                .Distinct()
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name + " " + x.Surname,
                });

            return await query.GetSelectListResult(input);
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<QuoteEditDto> GetQuoteForEdit(NullableIdDto input)
        {
            QuoteEditDto quoteEditDto;

            if (input.Id.HasValue)
            {
                quoteEditDto = await (await _quoteRepository.GetQueryAsync())
                    .Select(quote => new QuoteEditDto
                    {
                        Id = quote.Id,
                        Name = quote.Name,
                        CustomerId = quote.CustomerId,
                        CustomerName = quote.Customer.Name,
                        OfficeId = quote.OfficeId,
                        OfficeName = quote.Office.Name,
                        ContactId = quote.ContactId,
                        ContactName = quote.Contact.Name,
                        Description = quote.Description,
                        ProposalDate = quote.ProposalDate,
                        ProposalExpiryDate = quote.ProposalExpiryDate,
                        InactivationDate = quote.InactivationDate,
                        Status = quote.Status,
                        SalesPersonId = quote.SalesPersonId,
                        SalesPersonName = quote.SalesPerson.Name + " " + quote.SalesPerson.Surname,
                        PONumber = quote.PONumber,
                        IsTaxExempt = quote.IsTaxExempt,
                        SpectrumNumber = quote.SpectrumNumber,
                        BaseFuelCost = quote.BaseFuelCost,
                        FuelSurchargeCalculationId = quote.FuelSurchargeCalculationId,
                        FuelSurchargeCalculationName = quote.FuelSurchargeCalculation.Name,
                        CanChangeBaseFuelCost = quote.FuelSurchargeCalculation.CanChangeBaseFuelCost,
                        Directions = quote.Directions,
                        Notes = quote.Notes,
                        ChargeTo = quote.ChargeTo,
                        HasOrders = quote.Orders.Any(),
                    })
                    .FirstAsync(x => x.Id == input.Id.Value);
            }
            else
            {
                quoteEditDto = new QuoteEditDto
                {
                    OfficeId = Session.OfficeId,
                    OfficeName = Session.OfficeName,
                    Notes = await SettingManager.GetSettingValueAsync(AppSettings.Quote.DefaultNotes),
                };

                var today = await GetToday();
                quoteEditDto.ProposalDate = today;
                quoteEditDto.ProposalExpiryDate = today.AddDays(30);

                quoteEditDto.FuelSurchargeCalculationId = await SettingManager.GetDefaultFuelSurchargeCalculationId();
                if (quoteEditDto.FuelSurchargeCalculationId > 0)
                {
                    var fuelSurchargeCalculation = await (await _fuelSurchargeCalculationRepository.GetQueryAsync())
                        .Where(x => x.Id == quoteEditDto.FuelSurchargeCalculationId)
                        .Select(x => new
                        {
                            x.Name,
                            x.CanChangeBaseFuelCost,
                            x.BaseFuelCost,
                        })
                        .FirstOrDefaultAsync();

                    quoteEditDto.FuelSurchargeCalculationName = fuelSurchargeCalculation.Name;
                    quoteEditDto.CanChangeBaseFuelCost = fuelSurchargeCalculation.CanChangeBaseFuelCost;
                    quoteEditDto.BaseFuelCost = fuelSurchargeCalculation.BaseFuelCost;
                }
            }
            await _singleOfficeService.FillSingleOffice(quoteEditDto);

            if (!quoteEditDto.SalesPersonId.HasValue)
            {
                var userFullName = await (await UserManager.GetQueryAsync())
                    .Where(x => x.Id == AbpSession.UserId).Select(x => x.Name + " " + x.Surname)
                    .FirstOrDefaultAsync();
                quoteEditDto.SalesPersonId = AbpSession.UserId;
                quoteEditDto.SalesPersonName = userFullName;
            }

            return quoteEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task<int> EditQuote(QuoteEditDto model)
        {
            var quote = model.Id.HasValue ? await _quoteRepository.GetAsync(model.Id.Value) : new Quote();

            var fieldDiffs = new List<QuoteFieldDiff>();

            if (quote.Name != model.Name)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Name, quote.Name, model.Name));
                }
                quote.Name = model.Name;
            }

            if (quote.CustomerId != model.CustomerId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Customer, quote.CustomerId, model.CustomerId));
                }
                quote.CustomerId = model.CustomerId;
            }

            if (quote.ContactId != model.ContactId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Contact, quote.ContactId, model.ContactId));
                }
                quote.ContactId = model.ContactId;
            }

            if (quote.OfficeId != model.OfficeId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Office, quote.OfficeId, model.OfficeId));
                }
                quote.OfficeId = model.OfficeId;
            }

            if (quote.Description != model.Description)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Description, quote.Description, model.Description));
                }
                quote.Description = model.Description;
            }

            if (quote.ProposalDate?.Date != model.ProposalDate?.Date)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.ProposalDate, quote.ProposalDate?.ToShortDateString(), model.ProposalDate?.ToShortDateString()));
                }
                quote.ProposalDate = model.ProposalDate?.Date;
            }

            if (quote.ProposalExpiryDate?.Date != model.ProposalExpiryDate?.Date)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.ProposalExpiryDate, quote.ProposalExpiryDate?.ToShortDateString(), model.ProposalExpiryDate?.ToShortDateString()));
                }
                quote.ProposalExpiryDate = model.ProposalExpiryDate?.Date;
            }

            if (quote.InactivationDate?.Date != model.InactivationDate?.Date)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.InactivationDate, quote.InactivationDate?.ToShortDateString(), model.InactivationDate?.ToShortDateString()));
                }
                quote.InactivationDate = model.InactivationDate?.Date;
            }

            if (quote.Status != model.Status)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Status, (int)quote.Status, quote.Status.GetDisplayName(), (int)model.Status, model.Status.GetDisplayName()));
                }
                quote.Status = model.Status;
            }

            if (quote.SalesPersonId != model.SalesPersonId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.SalesPerson, (int?)quote.SalesPersonId, (int?)model.SalesPersonId));
                }
                quote.SalesPersonId = model.SalesPersonId;
            }

            if (quote.PONumber != model.PONumber)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.PoNumber, quote.PONumber, model.PONumber));
                }
                quote.PONumber = model.PONumber;
            }

            if (quote.IsTaxExempt != model.IsTaxExempt)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.IsTaxExempt, quote.IsTaxExempt.ToString(), model.IsTaxExempt.ToString()));
                }
                quote.IsTaxExempt = model.IsTaxExempt;
            }

            if (quote.SpectrumNumber != model.SpectrumNumber)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.SpectrumNumber, quote.SpectrumNumber, model.SpectrumNumber));
                }
                quote.SpectrumNumber = model.SpectrumNumber;
            }

            if (quote.BaseFuelCost != model.BaseFuelCost)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.BaseFuelCost, quote.BaseFuelCost?.ToString(), model.BaseFuelCost?.ToString()));
                }
                quote.BaseFuelCost = model.BaseFuelCost;
            }

            if (quote.FuelSurchargeCalculationId != model.FuelSurchargeCalculationId)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.FuelSurchargeCalculation, (int?)quote.FuelSurchargeCalculationId, (int?)model.FuelSurchargeCalculationId));
                }
                quote.FuelSurchargeCalculationId = model.FuelSurchargeCalculationId;
            }

            if (quote.Directions != model.Directions)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Directions, quote.Directions, model.Directions));
                }
                quote.Directions = model.Directions;
            }

            if (quote.Notes != model.Notes)
            {
                if (quote.CaptureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.Notes, quote.Notes, model.Notes));
                }
                quote.Notes = model.Notes;
            }

            if (Session.OfficeCopyChargeTo)
            {
                if (quote.ChargeTo != model.ChargeTo)
                {
                    if (quote.CaptureHistory)
                    {
                        fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.ChargeTo, quote.ChargeTo, model.ChargeTo));
                    }
                    quote.ChargeTo = model.ChargeTo;
                }
            }

            await UpdateDiffDisplayValues(quote.Id, false, fieldDiffs);

            if (quote.Id == 0)
            {
                await _quoteRepository.InsertAsync(quote);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            await UpdateDiffDisplayValues(quote.Id, true, fieldDiffs);

            await InsertQuoteHistory(fieldDiffs, QuoteChangeType.QuoteBodyEdited, quote.Id, quote.CreatorUserId);

            return quote.Id;
        }

        private async Task InsertQuoteHistory(List<QuoteFieldDiff> fieldDiffs, QuoteChangeType changeType, int quoteId, long? creatorId = null)
        {
            if (fieldDiffs.Any())
            {
                var quoteHistory = new QuoteHistoryRecord
                {
                    DateTime = Clock.Now,
                    ChangeType = changeType,
                    QuoteId = quoteId,
                };
                await _quoteHistoryRepository.InsertAsync(quoteHistory);

                foreach (var fieldDiff in fieldDiffs)
                {
                    fieldDiff.QuoteHistoryRecord = quoteHistory;
                    await _quoteFieldDiffRepository.InsertAsync(fieldDiff);
                }

                await CurrentUnitOfWork.SaveChangesAsync();

                await SendQuoteChangedEmail(quoteId, quoteHistory.Id, creatorId);
            }
        }

        private async Task SendQuoteChangedEmail(int quoteId, int quoteHistoryId, long? creatorId = null)
        {
            creatorId ??= await (await _quoteRepository.GetQueryAsync()).Where(x => x.Id == quoteId).Select(x => x.CreatorUserId).FirstOrDefaultAsync();
            if (Session.UserId != creatorId)
            {
                var creatorEmail = await (await _userRepository.GetQueryAsync()).Where(x => x.Id == creatorId).Select(x => x.EmailAddress).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(creatorEmail))
                {
                    return;
                }

                var changedByUser = await (await _userRepository.GetQueryAsync())
                   .Where(x => x.Id == Session.UserId)
                   .Select(x => new
                   {
                       FirstName = x.Name,
                       LastName = x.Surname,
                       PhoneNumber = x.PhoneNumber,
                   })
                   .FirstAsync();

                var quote = await (await _quoteRepository.GetQueryAsync())
                    .Where(x => x.Id == quoteId)
                    .Select(x => new
                    {
                        CustomerName = x.Customer.Name,
                    })
                    .FirstAsync();

                var siteUrl = _webUrlService.GetSiteRootAddress();

                var subject = await SettingManager.GetSettingValueAsync(AppSettings.Quote.ChangedNotificationEmail.SubjectTemplate);
                subject = subject
                    .Replace("{Quote.Id}", quoteId.ToString());

                var body = await SettingManager.GetSettingValueAsync(AppSettings.Quote.ChangedNotificationEmail.BodyTemplate);
                body = body
                    .Replace("{Quote.Id}", quoteId.ToString())
                    .Replace("{Quote.Url}", siteUrl + "app/quotes?id=" + quoteId)
                    .Replace("{QuoteHistory.Url}", siteUrl + "app/QuoteHistory/Index/" + quoteHistoryId)
                    .Replace("{Customer.Name}", quote.CustomerName)
                    .Replace("{ChangedByUser.FirstName}", changedByUser.FirstName)
                    .Replace("{ChangedByUser.LastName}", changedByUser.LastName)
                    .Replace("{ChangedByUser.PhoneNumber}", changedByUser.PhoneNumber);


                var message = new MailMessage
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false,
                };
                message.To.Add(creatorEmail);
                await _emailSender.SendAsync(message);
            }
        }

        private async Task UpdateDiffDisplayValues(int recordId, bool updateNewValues, List<QuoteFieldDiff> diffs)
        {
            diffs.RemoveAll(x => (x.OldDisplayValue != null || x.NewDisplayValue != null)
                            && ((x.OldDisplayValue ?? string.Empty) == (x.NewDisplayValue ?? string.Empty)));

            var quoteFieldsWithDisplayValue = new[]
            {
                QuoteFieldEnum.Customer,
                QuoteFieldEnum.Contact,
                QuoteFieldEnum.SalesPerson,
                QuoteFieldEnum.FuelSurchargeCalculation,
            };

            if (diffs.Any(x => quoteFieldsWithDisplayValue.Contains(x.Field)))
            {
                var quoteDisplayValues = await (await _quoteRepository.GetQueryAsync())
                    .Where(x => x.Id == recordId)
                    .Select(x => new
                    {
                        CustomerName = x.Customer.Name,
                        ContactName = x.Contact.Name,
                        SalesPersonName = x.SalesPerson.Name + " " + x.SalesPerson.Surname,
                        FuelSurchargeCalculationName = x.FuelSurchargeCalculation.Name,
                    })
                    .FirstOrDefaultAsync();

                if (updateNewValues)
                {
                    diffs.Where(x => x.Field == QuoteFieldEnum.Customer).ToList().ForEach(x => x.NewDisplayValue = quoteDisplayValues?.CustomerName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.Contact).ToList().ForEach(x => x.NewDisplayValue = quoteDisplayValues?.ContactName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.SalesPerson).ToList().ForEach(x => x.NewDisplayValue = quoteDisplayValues?.SalesPersonName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.FuelSurchargeCalculation).ToList().ForEach(x => x.NewDisplayValue = quoteDisplayValues?.FuelSurchargeCalculationName);
                }
                else
                {
                    diffs.Where(x => x.Field == QuoteFieldEnum.Customer).ToList().ForEach(x => x.OldDisplayValue = quoteDisplayValues?.CustomerName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.Contact).ToList().ForEach(x => x.OldDisplayValue = quoteDisplayValues?.ContactName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.SalesPerson).ToList().ForEach(x => x.OldDisplayValue = quoteDisplayValues?.SalesPersonName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.FuelSurchargeCalculation).ToList().ForEach(x => x.OldDisplayValue = quoteDisplayValues?.FuelSurchargeCalculationName);
                }
            }

            var quoteLineFieldsWithDisplayValue = new[]
            {
                QuoteFieldEnum.LineItemFreightItem,
                QuoteFieldEnum.LineItemLoadAt,
                QuoteFieldEnum.LineItemDeliverTo,
                QuoteFieldEnum.LineItemMaterialUom,
                QuoteFieldEnum.LineItemFreightUom,
            };

            if (diffs.Any(x => quoteLineFieldsWithDisplayValue.Contains(x.Field)))
            {
                var displayValues = await (await _quoteLineRepository.GetQueryAsync())
                    .Where(x => x.Id == recordId)
                    .Select(x => new
                    {
                        FreightItemName = x.FreightItem.Name,
                        MaterialItemName = x.MaterialItem.Name,
                        LoadAtName = x.LoadAt.DisplayName,
                        DeliverToName = x.DeliverTo.DisplayName,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomName = x.FreightUom.Name,
                        DriverPayTimeClassificationName = x.DriverPayTimeClassification.Name,
                    })
                    .FirstOrDefaultAsync();

                if (updateNewValues)
                {
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemFreightItem).ToList().ForEach(x => x.NewDisplayValue = displayValues?.FreightItemName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemMaterialItem).ToList().ForEach(x => x.NewDisplayValue = displayValues?.MaterialItemName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemLoadAt).ToList().ForEach(x => x.NewDisplayValue = displayValues?.LoadAtName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemDeliverTo).ToList().ForEach(x => x.NewDisplayValue = displayValues?.DeliverToName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemMaterialUom).ToList().ForEach(x => x.NewDisplayValue = displayValues?.MaterialUomName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemFreightUom).ToList().ForEach(x => x.NewDisplayValue = displayValues?.FreightUomName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemDriverPayTimeClassification).ToList().ForEach(x => x.NewDisplayValue = displayValues?.DriverPayTimeClassificationName);
                }
                else
                {
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemFreightItem).ToList().ForEach(x => x.OldDisplayValue = displayValues?.FreightItemName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemMaterialItem).ToList().ForEach(x => x.OldDisplayValue = displayValues?.MaterialItemName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemLoadAt).ToList().ForEach(x => x.OldDisplayValue = displayValues?.LoadAtName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemDeliverTo).ToList().ForEach(x => x.OldDisplayValue = displayValues?.DeliverToName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemMaterialUom).ToList().ForEach(x => x.OldDisplayValue = displayValues?.MaterialUomName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemFreightUom).ToList().ForEach(x => x.OldDisplayValue = displayValues?.FreightUomName);
                    diffs.Where(x => x.Field == QuoteFieldEnum.LineItemDriverPayTimeClassification).ToList().ForEach(x => x.OldDisplayValue = displayValues?.DriverPayTimeClassificationName);
                }
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task<int> CopyQuote(EntityDto input)
        {
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            var allowLoadBasedRates = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowLoadBasedRates);

            var quote = await (await _quoteRepository.GetQueryAsync())
                .AsNoTracking()
                .Include(x => x.QuoteLines)
                    .ThenInclude(x => x.QuoteLineVehicleCategories)
                .FirstAsync(x => x.Id == input.Id);

            var today = await GetToday();

            var newQuote = new Quote
            {
                CustomerId = quote.CustomerId,
                ContactId = quote.ContactId,
                OfficeId = quote.OfficeId,
                Name = quote.Name,
                Description = quote.Description,
                ProposalDate = today,
                ProposalExpiryDate = today.AddDays(30),
                Status = quote.Status,
                SalesPersonId = quote.SalesPersonId,
                PONumber = quote.PONumber,
                IsTaxExempt = quote.IsTaxExempt,
                SpectrumNumber = quote.SpectrumNumber,
                BaseFuelCost = quote.BaseFuelCost,
                FuelSurchargeCalculationId = quote.FuelSurchargeCalculationId,
                Directions = quote.Directions,
                Notes = quote.Notes,
                CaptureHistory = false,
            };

            if (Session.OfficeCopyChargeTo)
            {
                newQuote.ChargeTo = quote.ChargeTo;
            }

            var newQuoteLines = quote.QuoteLines.Select(s =>
            {
                var newQuoteLine = new QuoteLine
                {
                    LoadAtId = s.LoadAtId,
                    DeliverToId = s.DeliverToId,
                    FreightItemId = s.FreightItemId,
                    MaterialItemId = s.MaterialItemId,
                    MaterialUomId = s.MaterialUomId,
                    FreightUomId = s.FreightUomId,
                    Designation = s.Designation,
                    PricePerUnit = s.PricePerUnit,
                    MaterialCostRate = s.MaterialCostRate,
                    IsPricePerUnitOverridden = s.IsPricePerUnitOverridden,
                    FreightRate = s.FreightRate,
                    IsFreightRateOverridden = s.IsFreightRateOverridden,
                    LeaseHaulerRate = s.LeaseHaulerRate,
                    IsLeaseHaulerRateOverridden = s.IsLeaseHaulerRateOverridden,
                    FreightRateToPayDrivers = s.FreightRateToPayDrivers,
                    IsFreightRateToPayDriversOverridden = s.IsFreightRateToPayDriversOverridden,
                    DriverPayTimeClassificationId = s.DriverPayTimeClassificationId,
                    HourlyDriverPayRate = s.HourlyDriverPayRate,
                    TravelTime = s.TravelTime,
                    ProductionPay = allowProductionPay && s.ProductionPay,
                    RequireTicket = s.RequireTicket,
                    LoadBased = allowLoadBasedRates && s.LoadBased,
                    MaterialQuantity = s.MaterialQuantity,
                    FreightQuantity = s.FreightQuantity,
                    JobNumber = s.JobNumber,
                    Note = s.Note,
                    BedConstruction = s.BedConstruction,
                    Quote = newQuote,
                };
                foreach (var vehicleCategory in s.QuoteLineVehicleCategories)
                {
                    newQuoteLine.QuoteLineVehicleCategories.Add(new QuoteLineVehicleCategory
                    {
                        QuoteLine = newQuoteLine,
                        VehicleCategoryId = vehicleCategory.VehicleCategoryId,
                    });
                }
                return newQuoteLine;
            }).ToList();

            await _quoteLineRepository.InsertRangeAsync(newQuoteLines);

            return await _quoteRepository.InsertAndGetIdAsync(newQuote);
        }

        [AbpAuthorize(
            AppPermissions.Pages_Quotes_Edit,
            AppPermissions.Pages_Orders_View,
            RequireAllPermissions = true
        )]
        public async Task<int> CreateQuoteFromOrder(CreateQuoteFromOrderInput input)
        {
            var order = await (await _orderRepository.GetQueryAsync())
                .Include(x => x.Customer)
                .Include(x => x.OrderLines)
                .ThenInclude(x => x.OrderLineVehicleCategories)
                .FirstOrDefaultAsync(x => x.Id == input.OrderId);

            if (order == null)
            {
                throw new UserFriendlyException("Order with the specified Id wasn't found");
            }

            if (order.OfficeId != Session.OfficeId)
            {
                throw new AbpAuthorizationException("A user is not allowed to edit the Order from another office.");
            }

            if (!order.Customer.IsActive)
            {
                throw new UserFriendlyException("Quotes can't be created for inactive customers.");
            }

            var today = await GetToday();

            var quote = new Quote
            {
                ContactId = order.ContactId,
                CustomerId = order.CustomerId,
                OfficeId = order.OfficeId,
                Directions = order.Directions,
                Name = input.QuoteName,
                PONumber = order.PONumber,
                Status = QuoteStatus.Active,
                SalesPersonId = AbpSession.UserId,
                Notes = await SettingManager.GetSettingValueAsync(AppSettings.Quote.DefaultNotes),
                ProposalDate = today,
                ProposalExpiryDate = today.AddDays(30),
            };

            if (Session.OfficeCopyChargeTo)
            {
                quote.ChargeTo = order.ChargeTo;
            }

            quote.Id = await _quoteRepository.InsertAndGetIdAsync(quote);

            var orderLines = order.OrderLines
                .OrderBy(x => x.LineNumber)
                .ToList();

            foreach (var orderLine in orderLines)
            {
                var newQuoteLine = new QuoteLine
                {
                    QuoteId = quote.Id,
                    Designation = orderLine.Designation,
                    JobNumber = orderLine.JobNumber,
                    Note = orderLine.Note,
                    FreightRate = orderLine.FreightPricePerUnit,
                    IsFreightRateOverridden = orderLine.IsFreightPricePerUnitOverridden,
                    PricePerUnit = orderLine.MaterialPricePerUnit,
                    MaterialCostRate = orderLine.MaterialCostRate,
                    IsPricePerUnitOverridden = orderLine.IsMaterialPricePerUnitOverridden,
                    LeaseHaulerRate = orderLine.LeaseHaulerRate,
                    IsLeaseHaulerRateOverridden = orderLine.IsLeaseHaulerPriceOverridden,
                    FreightRateToPayDrivers = orderLine.FreightRateToPayDrivers,
                    IsFreightRateToPayDriversOverridden = orderLine.IsFreightRateToPayDriversOverridden,
                    DriverPayTimeClassificationId = orderLine.DriverPayTimeClassificationId,
                    HourlyDriverPayRate = orderLine.HourlyDriverPayRate,
                    TravelTime = orderLine.TravelTime,
                    ProductionPay = orderLine.ProductionPay,
                    RequireTicket = orderLine.RequireTicket,
                    LoadBased = orderLine.LoadBased,
                    MaterialQuantity = orderLine.MaterialQuantity,
                    FreightQuantity = orderLine.FreightQuantity,
                    FreightItemId = orderLine.FreightItemId,
                    MaterialItemId = orderLine.MaterialItemId,
                    LoadAtId = orderLine.LoadAtId,
                    DeliverToId = orderLine.DeliverToId,
                    MaterialUomId = orderLine.MaterialUomId,
                    FreightUomId = orderLine.FreightUomId,
                    BedConstruction = orderLine.BedConstruction,
                };

                newQuoteLine.QuoteLineVehicleCategories = orderLine.OrderLineVehicleCategories
                    .Select(vehicleCategory => new QuoteLineVehicleCategory
                    {
                        QuoteLine = newQuoteLine,
                        VehicleCategoryId = vehicleCategory.VehicleCategoryId,
                    }).ToList();

                newQuoteLine.Id = await _quoteLineRepository.InsertAndGetIdAsync(newQuoteLine);

                orderLine.QuoteLineId = newQuoteLine.Id;
            }

            order.QuoteId = quote.Id;

            return quote.Id;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit, AppPermissions.Pages_Orders_Edit)]
        public async Task SetQuoteStatus(SetQuoteStatusInput model)
        {
            var quote = await _quoteRepository.GetAsync(model.Id);
            quote.Status = model.Status;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task<bool> CanDeleteQuote(EntityDto input)
        {

            var quote = await (await _quoteRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    x.CaptureHistory,
                }).SingleAsync();

            if (quote.CaptureHistory)
            {
                return false;
            }

            var hasOrders = await (await _orderRepository.GetQueryAsync()).AnyAsync(x => x.QuoteId == input.Id);
            if (hasOrders)
            {
                return false;
            }

            return true;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task DeleteQuote(EntityDto input)
        {
            var canDelete = await CanDeleteQuote(input);
            if (!canDelete)
            {
                throw new UserFriendlyException("You can't delete selected row because it has data associated with it.");
            }
            await _quoteLineRepository.DeleteAsync(x => x.QuoteId == input.Id);
            await _quoteRepository.DeleteAsync(input.Id);
            await DeleteTrackableQuoteEmails(input.Id);
        }

        private async Task DeleteTrackableQuoteEmails(int quoteId)
        {
            var quoteEmails = await (await _quoteEmailRepository.GetQueryAsync())
                .Include(x => x.Email)
                .ThenInclude(x => x.Events)
                .Include(x => x.Email)
                .ThenInclude(x => x.Receivers)
                .Where(x => x.QuoteId == quoteId)
                .ToListAsync();

            foreach (var quoteEmail in quoteEmails)
            {
                if (quoteEmail.Email != null)
                {

                    foreach (var emailEvent in quoteEmail.Email.Events)
                    {
                        await _trackableEmailEventRepository.DeleteAsync(emailEvent);
                    }

                    foreach (var emailReceiver in quoteEmail.Email.Receivers)
                    {
                        await _trackableEmailReceiverRepository.DeleteAsync(emailReceiver);
                    }

                    await _trackableEmailRepository.DeleteAsync(quoteEmail.Email);
                }

                await _quoteEmailRepository.DeleteAsync(quoteEmail);
            }
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task InactivateQuote(EntityDto input)
        {
            var quote = await _quoteRepository.GetAsync(input.Id);
            quote.InactivationDate = await GetToday();
            quote.Status = QuoteStatus.Inactive;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<PagedResultDto<QuoteLineDto>> GetQuoteLines(GetQuoteLinesInput input)
        {
            var allowProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay);
            var allowLoadBasedRates = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.AllowLoadBasedRates);

            var query = await _quoteLineRepository.GetQueryAsync();

            var totalCount = await query.CountAsync();

            var items = await query
                .Where(x => x.QuoteId == input.QuoteId)
                .WhereIf(input.LoadAtId.HasValue || input.ForceDuplicateFilters, x => x.LoadAtId == input.LoadAtId)
                .WhereIf(input.DeliverToId.HasValue || input.ForceDuplicateFilters, x => x.DeliverToId == input.DeliverToId)
                .WhereIf(input.ItemId.HasValue, x => x.FreightItemId == input.ItemId || x.MaterialItemId == input.ItemId)
                .WhereIf(input.MaterialUomId.HasValue, x => x.MaterialUomId == input.MaterialUomId)
                .WhereIf(input.FreightUomId.HasValue, x => x.FreightUomId == input.FreightUomId)
                .WhereIf(input.Designation.HasValue, x => x.Designation == input.Designation)
                .Select(x => new QuoteLineDto
                {
                    Id = x.Id,
                    LoadAtName = x.LoadAt.DisplayName,
                    DeliverToName = x.DeliverTo.DisplayName,
                    FreightItemName = x.FreightItem.Name,
                    MaterialItemName = x.MaterialItem.Name,
                    MaterialUomName = x.MaterialUom.Name,
                    FreightUomName = x.FreightUom.Name,
                    Designation = x.Designation,
                    PricePerUnit = x.PricePerUnit,
                    FreightRate = x.FreightRate,
                    LeaseHaulerRate = x.LeaseHaulerRate,
                    FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                    ProductionPay = allowProductionPay && x.ProductionPay,
                    RequireTicket = x.RequireTicket,
                    LoadBased = allowLoadBasedRates && x.LoadBased,
                    MaterialQuantity = x.MaterialQuantity,
                    FreightQuantity = x.FreightQuantity,
                    TruckCategory = x.QuoteLineVehicleCategories
                        .Select(quoteLineVehicleCategory => quoteLineVehicleCategory.VehicleCategory.Name).ToList(),
                })
                .OrderBy(input.Sorting)
                //.PageBy(input)
                .ToListAsync();

            return new PagedResultDto<QuoteLineDto>(
                totalCount,
                items);
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<PagedResultDto<QuoteDeliveryDto>> GetQuoteLineDeliveries(EntityDto input)
        {
            var quoteLine = await (await _quoteLineRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    Tickets = x.OrderLines
                        .SelectMany(ol => ol.Tickets)
                        .Select(t => new QuoteDeliveryRawDto
                        {
                            Date = t.OrderLine.Order.DeliveryDate,
                            Designation = t.OrderLine.Designation,
                            OrderLineMaterialUomId = t.OrderLine.MaterialUomId,
                            OrderLineFreightUomId = t.OrderLine.FreightUomId,
                            TicketUomId = t.FreightUomId,
                            FreightQuantity = t.FreightQuantity,
                            MaterialQuantity = t.MaterialQuantity,
                        }),
                })
                .FirstAsync();

            var groupedItems = quoteLine.Tickets.GroupBy(x => new { x.Date, x.Designation })
                .Select(g => new QuoteDeliveryDto
                {
                    Date = g.Key.Date,
                    Designation = g.Key.Designation,
                    ActualFreightQuantity = g.Sum(t => t.GetFreightQuantity() ?? 0),
                    ActualMaterialQuantity = g.Sum(t => t.GetMaterialQuantity() ?? 0),
                }).ToList();

            return new PagedResultDto<QuoteDeliveryDto>(groupedItems.Count, groupedItems);
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<QuoteLineEditDto> GetQuoteLineForEdit(GetQuoteLineForEditInput input)
        {
            QuoteLineEditDto quoteLineEditDto;

            if (input.Id.HasValue)
            {
                quoteLineEditDto = await (await _quoteLineRepository.GetQueryAsync())
                    .Select(x => new QuoteLineEditDto
                    {
                        Id = x.Id,
                        QuoteId = x.QuoteId,
                        LoadAtId = x.LoadAtId,
                        LoadAtName = x.LoadAt.DisplayName,
                        DeliverToId = x.DeliverToId,
                        DeliverToName = x.DeliverTo.DisplayName,
                        FreightItemId = x.FreightItemId,
                        FreightItemName = x.FreightItem.Name,
                        UseZoneBasedRates = x.FreightItem.UseZoneBasedRates,
                        MaterialItemId = x.MaterialItemId,
                        MaterialItemName = x.MaterialItem.Name,
                        MaterialUomId = x.MaterialUomId,
                        MaterialUomName = x.MaterialUom.Name,
                        FreightUomId = x.FreightUomId,
                        FreightUomName = x.FreightUom.Name,
                        FreightUomBaseId = (UnitOfMeasureBaseEnum?)x.FreightUom.UnitOfMeasureBaseId,
                        Designation = x.Designation,
                        PricePerUnit = x.PricePerUnit,
                        MaterialCostRate = x.MaterialCostRate,
                        IsPricePerUnitOverridden = x.IsPricePerUnitOverridden,
                        FreightRate = x.FreightRate,
                        IsFreightRateOverridden = x.IsFreightRateOverridden,
                        LeaseHaulerRate = x.LeaseHaulerRate,
                        IsLeaseHaulerRateOverridden = x.IsLeaseHaulerRateOverridden,
                        FreightRateToPayDrivers = x.FreightRateToPayDrivers,
                        IsFreightRateToPayDriversOverridden = x.IsFreightRateToPayDriversOverridden,
                        DriverPayTimeClassificationId = x.DriverPayTimeClassificationId,
                        DriverPayTimeClassificationName = x.DriverPayTimeClassification.Name,
                        HourlyDriverPayRate = x.HourlyDriverPayRate,
                        TravelTime = x.TravelTime,
                        ProductionPay = x.ProductionPay,
                        RequireTicket = x.RequireTicket,
                        LoadBased = x.LoadBased,
                        MaterialQuantity = x.MaterialQuantity,
                        FreightQuantity = x.FreightQuantity,
                        JobNumber = x.JobNumber,
                        Note = x.Note,
                        BedConstruction = x.BedConstruction,
                        CustomerPricingTierId = x.Quote.Customer.PricingTierId,
                        CustomerIsCod = x.Quote.Customer.IsCod,
                        VehicleCategories = x.QuoteLineVehicleCategories.Select(vc => new QuoteLineVehicleCategoryDto
                        {
                            Id = vc.VehicleCategory.Id,
                            Name = vc.VehicleCategory.Name,
                        }).ToList(),
                    })
                    .SingleAsync(x => x.Id == input.Id.Value);
            }
            else if (input.QuoteId.HasValue)
            {
                var quoteDetails = await (await _quoteRepository.GetQueryAsync())
                    .Where(x => x.Id == input.QuoteId)
                    .Select(x => new
                    {
                        CustomerIsCod = x.Customer.IsCod,
                        CustomerPricingTierId = x.Customer.PricingTierId,
                    }).FirstAsync();

                var requiredTicketEntry = await SettingManager.GetRequiredTicketEntry();

                quoteLineEditDto = new QuoteLineEditDto
                {
                    QuoteId = input.QuoteId.Value,
                    ProductionPay = await SettingManager.GetSettingValueAsync<bool>(AppSettings.TimeAndPay.DefaultToProductionPay),
                    RequireTicket = requiredTicketEntry.GetRequireTicketDefaultValue(),
                    CustomerPricingTierId = quoteDetails.CustomerPricingTierId,
                    CustomerIsCod = quoteDetails.CustomerIsCod,
                };
            }
            else
            {
                throw new ArgumentNullException(nameof(input.QuoteId));
            }

            return quoteLineEditDto;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit, AppPermissions.Pages_Quotes_Items_Create)]
        public async Task EditQuoteLine(QuoteLineEditDto model)
        {
            var quoteLine = model.Id.HasValue ? await _quoteLineRepository.GetAsync(model.Id.Value) : new QuoteLine();
            var isNew = quoteLine.Id == 0;

            if (!isNew)
            {
                await PermissionChecker.AuthorizeAsync(AppPermissions.Pages_Quotes_Edit);
            }

            if (isNew)
            {
                quoteLine.QuoteId = model.QuoteId;
            }

            var fieldDiffs = await UpdateValuesAndGetDiff(quoteLine, model);

            await UpdateDiffDisplayValues(quoteLine.Id, false, fieldDiffs);

            var existingVehicleCategories = model.Id.HasValue ? await (await _quoteLineVehicleCategoryRepository.GetQueryAsync())
                .Where(vc => vc.QuoteLineId == model.Id)
                .ToListAsync() : new List<QuoteLineVehicleCategory>();

            await _quoteLineVehicleCategoryRepository.DeleteRangeAsync(
                existingVehicleCategories
                    .Where(e => !model.VehicleCategories.Any(m => m.Id == e.VehicleCategoryId))
                    .ToList()
            );

            await _quoteLineVehicleCategoryRepository.InsertRangeAsync(
                model.VehicleCategories
                    .Where(m => !existingVehicleCategories.Any(e => e.VehicleCategoryId == m.Id))
                    .Select(x => new QuoteLineVehicleCategory
                    {
                        QuoteLine = quoteLine,
                        VehicleCategoryId = x.Id,
                    })
                    .ToList()
            );

            if (isNew)
            {
                await _quoteLineRepository.InsertAsync(quoteLine);
            }
            await CurrentUnitOfWork.SaveChangesAsync();

            await UpdateDiffDisplayValues(quoteLine.Id, true, fieldDiffs);

            await InsertQuoteHistory(fieldDiffs, isNew ? QuoteChangeType.LineItemAdded : QuoteChangeType.LineItemEdited, quoteLine.QuoteId);
        }

        [HttpPost]
        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task DeleteQuoteLines(IdListInput input)
        {
            if (await (await _quoteLineRepository.GetQueryAsync())
                    .AnyAsync(x => input.Ids.Contains(x.Id) && x.OrderLines.Any()))
            {
                throw new UserFriendlyException(L("UnableToDeleteQuoteLineWithAssociatedData"));
            }

            var quoteLines = await (await _quoteLineRepository.GetQueryAsync())
                .Where(x => input.Ids.Contains(x.Id))
                .ToListAsync();

            foreach (var quoteLine in quoteLines)
            {
                var fieldDiffs = await UpdateValuesAndGetDiff(quoteLine.Clone(), new QuoteLineEditDto());
                await UpdateDiffDisplayValues(quoteLine.Id, false, fieldDiffs);
                await InsertQuoteHistory(fieldDiffs, QuoteChangeType.LineItemDeleted, quoteLine.QuoteId);
                await _quoteLineRepository.DeleteAsync(quoteLine);
            }
        }

        private async Task<List<QuoteFieldDiff>> UpdateValuesAndGetDiff(QuoteLine quoteLine, QuoteLineEditDto model)
        {
            var captureHistory = await (await _quoteRepository.GetQueryAsync())
                                     .Where(x => x.Id == quoteLine.QuoteId)
                                     .Select(x => x.CaptureHistory)
                                     .FirstOrDefaultAsync();

            var fieldDiffs = new List<QuoteFieldDiff>();

            if (quoteLine.LoadAtId != model.LoadAtId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemLoadAt, quoteLine.LoadAtId, model.LoadAtId));
                }
                quoteLine.LoadAtId = model.LoadAtId;
            }

            if (quoteLine.DeliverToId != model.DeliverToId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemDeliverTo, quoteLine.DeliverToId, model.DeliverToId));
                }
                quoteLine.DeliverToId = model.DeliverToId;
            }

            if (quoteLine.FreightItemId != model.FreightItemId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemFreightItem, quoteLine.FreightItemId, model.FreightItemId));
                }
                quoteLine.FreightItemId = model.FreightItemId;
            }

            if (quoteLine.MaterialItemId != model.MaterialItemId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemMaterialItem, quoteLine.MaterialItemId, model.MaterialItemId));
                }
                quoteLine.MaterialItemId = model.MaterialItemId;
            }

            if (quoteLine.MaterialUomId != model.MaterialUomId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemMaterialUom, quoteLine.MaterialUomId, model.MaterialUomId));
                }
                quoteLine.MaterialUomId = model.MaterialUomId;
            }

            if (quoteLine.FreightUomId != model.FreightUomId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemFreightUom, quoteLine.FreightUomId, model.FreightUomId));
                }
                quoteLine.FreightUomId = model.FreightUomId;
            }

            if (quoteLine.Designation != model.Designation)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemDesignation, (int)quoteLine.Designation, quoteLine.Designation.GetDisplayName(), (int)model.Designation, model.Designation.GetDisplayName()));
                }
                quoteLine.Designation = model.Designation;
            }

            if (quoteLine.PricePerUnit != model.PricePerUnit)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemPricePerUnit, quoteLine.PricePerUnit?.ToString(), model.PricePerUnit?.ToString()));
                }
                quoteLine.PricePerUnit = model.PricePerUnit;
            }
            quoteLine.IsPricePerUnitOverridden = model.IsPricePerUnitOverridden;
            quoteLine.MaterialCostRate = model.MaterialCostRate;

            if (quoteLine.FreightRate != model.FreightRate)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemFreightRate, quoteLine.FreightRate?.ToString(), model.FreightRate?.ToString()));
                }
                quoteLine.FreightRate = model.FreightRate;
            }
            quoteLine.IsFreightRateOverridden = model.IsFreightRateOverridden;

            if (quoteLine.LeaseHaulerRate != model.LeaseHaulerRate)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemLeaseHaulerRate, quoteLine.LeaseHaulerRate?.ToString(), model.LeaseHaulerRate?.ToString()));
                }
                quoteLine.LeaseHaulerRate = model.LeaseHaulerRate;
            }
            quoteLine.IsLeaseHaulerRateOverridden = model.IsLeaseHaulerRateOverridden;

            if (quoteLine.FreightRateToPayDrivers != model.FreightRateToPayDrivers)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemFreightRateToPayDrivers, quoteLine.FreightRateToPayDrivers?.ToString(), model.FreightRateToPayDrivers?.ToString()));
                }
                quoteLine.FreightRateToPayDrivers = model.FreightRateToPayDrivers;
            }
            quoteLine.IsFreightRateToPayDriversOverridden = model.IsFreightRateToPayDriversOverridden;

            if (quoteLine.HourlyDriverPayRate != model.HourlyDriverPayRate)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemHourlyDriverPayRate, quoteLine.HourlyDriverPayRate?.ToString(), model.HourlyDriverPayRate?.ToString()));
                }
                quoteLine.HourlyDriverPayRate = model.HourlyDriverPayRate;
            }

            if (quoteLine.TravelTime != model.TravelTime)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemTravelTime, quoteLine.TravelTime?.ToString(), model.TravelTime?.ToString()));
                }
                quoteLine.TravelTime = model.TravelTime;
            }

            if (quoteLine.DriverPayTimeClassificationId != model.DriverPayTimeClassificationId)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemDriverPayTimeClassification, quoteLine.DriverPayTimeClassificationId, model.DriverPayTimeClassificationId));
                }
                quoteLine.DriverPayTimeClassificationId = model.DriverPayTimeClassificationId;
            }

            if (quoteLine.ProductionPay != model.ProductionPay)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemProductionPay, quoteLine.ProductionPay.ToString(), model.ProductionPay.ToString()));
                }
                quoteLine.ProductionPay = model.ProductionPay;
            }

            if (quoteLine.RequireTicket != model.RequireTicket)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemRequireTicket, quoteLine.RequireTicket.ToString(), model.RequireTicket.ToString()));
                }
                quoteLine.RequireTicket = model.RequireTicket;
            }

            if (quoteLine.LoadBased != model.LoadBased)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemLoadBased, quoteLine.LoadBased.ToString(), model.LoadBased.ToString()));
                }
                quoteLine.LoadBased = model.LoadBased;
            }

            if (quoteLine.MaterialQuantity != model.MaterialQuantity)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemMaterialQuantity, quoteLine.MaterialQuantity?.ToString(), model.MaterialQuantity?.ToString()));
                }
                quoteLine.MaterialQuantity = model.MaterialQuantity;
            }

            if (quoteLine.FreightQuantity != model.FreightQuantity)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemFreightQuantity, quoteLine.FreightQuantity?.ToString(), model.FreightQuantity?.ToString()));
                }
                quoteLine.FreightQuantity = model.FreightQuantity;
            }

            if (quoteLine.JobNumber != model.JobNumber)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemJobNumber, quoteLine.JobNumber, model.JobNumber));
                }
                quoteLine.JobNumber = model.JobNumber;
            }

            if (quoteLine.Note != model.Note)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemNote, quoteLine.Note, model.Note));
                }
                quoteLine.Note = model.Note;
            }

            if (quoteLine.BedConstruction != model.BedConstruction)
            {
                if (captureHistory)
                {
                    fieldDiffs.Add(new QuoteFieldDiff(QuoteFieldEnum.LineItemBedConstruction, (int?)quoteLine.BedConstruction, quoteLine.BedConstruction.GetDisplayName(), (int?)model.BedConstruction, model.BedConstruction.GetDisplayName()));
                }
                quoteLine.BedConstruction = model.BedConstruction;
            }


            return fieldDiffs;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<byte[]> GetQuoteReport(GetQuoteReportInput input)
        {
            var data = await (await _quoteRepository.GetQueryAsync())
                .Where(x => x.Id == input.QuoteId)
                .Select(x => new QuoteReportDto
                {
                    ContactAttn = x.Contact.Name,
                    ContactPhoneNumber = x.Contact.PhoneNumber,
                    CustomerName = x.Customer.Name,
                    CustomerAddress1 = x.Customer.Address1,
                    CustomerAddress2 = x.Customer.Address2,
                    CustomerCity = x.Customer.City,
                    CustomerState = x.Customer.State,
                    CustomerZipCode = x.Customer.ZipCode,
                    CustomerCountryCode = x.Customer.CountryCode,
                    OfficeId = x.OfficeId,
                    QuotePoNumber = x.PONumber,
                    QuoteId = x.Id,
                    QuoteName = x.Name,
                    QuoteNotes = x.Notes,
                    QuoteBaseFuelCost = x.BaseFuelCost,
                    QuoteProposalDate = x.ProposalDate,
                    QuoteProposalExpiryDate = x.ProposalExpiryDate,
                    SalesPersonId = x.SalesPersonId,
                    Items = x.QuoteLines.Select(s => new QuoteReportItemDto
                    {
                        MaterialQuantity = s.MaterialQuantity,
                        FreightQuantity = s.FreightQuantity,
                        MaterialUomName = s.MaterialUom.Name,
                        FreightUomName = s.FreightUom.Name,
                        FreightRate = s.FreightRate,
                        LeaseHaulerRate = s.LeaseHaulerRate,
                        FreightRateToPayDrivers = s.FreightRateToPayDrivers,
                        PricePerUnit = s.PricePerUnit,
                        FreightItemName = s.FreightItem.Name,
                        MaterialItemName = s.MaterialItem.Name,
                        FreightItemDescription = s.FreightItem.Description,
                        MaterialItemDescription = s.MaterialItem.Description,
                        Designation = s.Designation,
                        LoadAtName = s.LoadAt.DisplayName,
                        DeliverToName = s.DeliverTo.DisplayName,
                        JobNumber = s.JobNumber,
                        Note = s.Note,
                        QuoteLineVehicleCategories = s.QuoteLineVehicleCategories
                            .Select(vc => vc.VehicleCategory.Name)
                            .ToList(),
                    }).ToList(),
                })
                .FirstAsync();

            var user = await (await _userRepository.GetQueryAsync())
                .Where(x => x.Id == data.SalesPersonId || x.Id == Session.UserId)
                .Select(x => new
                {
                    Id = x.Id,
                    Email = x.EmailAddress,
                    FullName = x.Name + " " + x.Surname,
                    SignaturePictureId = x.SignaturePictureId,
                })
                .OrderByDescending(x => x.Id == data.SalesPersonId) //get sales rep, but fallback to the current user if creator is not set
                .FirstAsync();

            data.UserEmail = user.Email;
            data.UserFullName = user.FullName;
            data.LogoBytes = await _logoProvider.GetReportLogoAsBytesAsync(data.OfficeId);
            data.SignatureBytes = await _binaryObjectManager.GetImageAsBytesAsync(user.SignaturePictureId);
            data.Today = await GetToday();
            data.CompanyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);
            data.CurrencyCulture = await SettingManager.GetCurrencyCultureAsync();
            data.HideLoadAt = input.HideLoadAt;
            data.ShowTruckCategories = await SettingManager.GetSettingValueAsync<bool>(AppSettings.General.AllowSpecifyingTruckAndTrailerCategoriesOnQuotesAndOrders);
            data.QuoteGeneralTermsAndConditions = await SettingManager.GetSettingValueAsync(AppSettings.Quote.GeneralTermsAndConditions);
            data.SeparateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);

            data.QuoteGeneralTermsAndConditions = data.QuoteGeneralTermsAndConditions
                .Replace("{CompanyName}", data.CompanyName)
                .Replace("{CompanyNameUpperCase}", data.CompanyName.ToUpper());

            await SetQuoteCaptureHistory(input.QuoteId);

            var result = QuoteReportGenerator.GenerateReport(data);

            return result;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<EmailQuoteReportDto> GetEmailQuoteReportModel(EntityDto input)
        {
            var user = await (await _userRepository.GetQueryAsync())
                .Where(x => x.Id == Session.UserId)
                .Select(x => new
                {
                    Email = x.EmailAddress,
                    FirstName = x.Name,
                    LastName = x.Surname,
                    PhoneNumber = x.PhoneNumber,
                })
                .FirstAsync();

            var quote = await (await _quoteRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    ContactEmail = x.Contact.Email,
                })
                .FirstAsync();

            var companyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);

            var subject = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailSubjectTemplate);

            var body = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailBodyTemplate);
            body = ReplaceEmailBodyTemplateTokens(body, user.FirstName, user.LastName, user.PhoneNumber, companyName);

            return new EmailQuoteReportDto
            {
                QuoteId = input.Id,
                From = user.Email,
                To = quote.ContactEmail,
                CC = user.Email,
                Subject = subject,
                Body = body,
            };
        }

        public static string ReplaceEmailBodyTemplateTokens(string bodyTemplate, string userFirstName, string userLastName, string userPhoneNumber, string companyName)
        {
            return bodyTemplate
                .Replace("{User.FirstName}", userFirstName)
                .Replace("{User.LastName}", userLastName)
                .Replace("{User.PhoneNumber}", userPhoneNumber)
                .Replace("{CompanyName}", companyName);
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_View)]
        public async Task<EmailQuoteReportResult> EmailQuoteReport(EmailQuoteReportDto input)
        {
            var reportBytes = await GetQuoteReport(new GetQuoteReportInput { QuoteId = input.QuoteId, HideLoadAt = input.HideLoadAt });
            var message = new MailMessage
            {
                From = new MailAddress(input.From),
                Subject = input.Subject,
                Body = input.Body,
                IsBodyHtml = false,
            };
            foreach (var to in EmailHelper.SplitEmailAddresses(input.To))
            {
                message.To.Add(to);
            }
            foreach (var cc in EmailHelper.SplitEmailAddresses(input.CC))
            {
                message.CC.Add(cc);
            }

            var filename = await SettingManager.GetSettingValueAsync(AppSettings.Quote.EmailSubjectTemplate);
            filename = Utilities.RemoveInvalidFileNameChars(filename);
            filename += ".pdf";

            using (var stream = new MemoryStream(reportBytes))
            {
                stream.Seek(0, SeekOrigin.Begin);
                message.Attachments.Add(new Attachment(stream, filename));

                try
                {
                    var trackableEmailId = await _trackableEmailSender.SendTrackableAsync(message);
                    var quote = await _quoteRepository.GetAsync(input.QuoteId);
                    quote.LastQuoteEmailId = trackableEmailId;
                    await _quoteEmailRepository.InsertAsync(new QuoteEmail
                    {
                        EmailId = trackableEmailId,
                        QuoteId = quote.Id,
                    });
                }
                catch (SmtpException ex)
                {
                    if (ex.Message.Contains("The from address does not match a verified Sender Identity"))
                    {
                        return new EmailQuoteReportResult
                        {
                            FromEmailAddressIsNotVerifiedError = true,
                        };
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            await SetQuoteCaptureHistory(input.QuoteId);

            return new EmailQuoteReportResult
            {
                Success = true,
            };
        }

        private async Task SetQuoteCaptureHistory(int quoteId)
        {
            var quote = await _quoteRepository.GetAsync(quoteId);
            quote.CaptureHistory = true;
        }

        [AbpAuthorize(AppPermissions.Pages_Quotes_Edit)]
        public async Task ActivateQuote(EntityDto input)
        {
            var quote = await _quoteRepository.GetAsync(input.Id);
            quote.InactivationDate = null;
            quote.Status = QuoteStatus.Active;
        }
    }
}
