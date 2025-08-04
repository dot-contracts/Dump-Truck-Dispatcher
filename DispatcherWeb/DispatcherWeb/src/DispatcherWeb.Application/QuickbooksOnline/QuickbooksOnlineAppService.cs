/*
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Dto;
using DispatcherWeb.QuickbooksOnline.Dto;
using DispatcherWeb.Url;
using Intuit.Ipp.Core;
using Intuit.Ipp.Data;
using Intuit.Ipp.DataService;
using Intuit.Ipp.Exception;
using Intuit.Ipp.OAuth2PlatformClient;
using Intuit.Ipp.QueryFilter;
using Intuit.Ipp.Security;
using Microsoft.Extensions.Configuration;
using ThreadingTask = System.Threading.Tasks.Task;

namespace DispatcherWeb.QuickbooksOnline
{
    public class QuickbooksOnlineAppService : DispatcherWebAppServiceBase, IQuickbooksOnlineAppService
    {
        private const int CustomerNameMaxLength = 500;

        private readonly IConfigurationRoot _appConfiguration;
        private readonly IWebUrlService _webUrlService;
        private readonly IRepository<Invoices.Invoice> _invoiceRepository;
        private readonly IRepository<Invoices.InvoiceUploadBatch> _invoiceUploadBatchRepository;
        private readonly Dictionary<string, Item> _itemCache;
        private readonly Dictionary<string, Customer> _customerCache;
        private readonly Dictionary<string, Term> _termCache;
        private readonly Dictionary<string, Account> _accountCache;

        public QuickbooksOnlineAppService(
            IAppConfigurationAccessor configurationAccessor,
            IWebUrlService webUrlService,
            IEncryptionService encryptionService,
            IRepository<Invoices.Invoice> invoiceRepository,
            IRepository<Invoices.InvoiceUploadBatch> invoiceUploadBatchRepository
            )
        {
            _appConfiguration = configurationAccessor.Configuration;
            _webUrlService = webUrlService;
            _invoiceRepository = invoiceRepository;
            _invoiceUploadBatchRepository = invoiceUploadBatchRepository;
            _itemCache = new Dictionary<string, Item>();
            _customerCache = new Dictionary<string, Customer>();
            _termCache = new Dictionary<string, Term>();
            _accountCache = new Dictionary<string, Account>();
        }

        public async Task<string> GetInitiateAuthUrl()
        {
            var auth2Client = GetAuth2Client();
            var scopes = new[] { OidcScopes.Accounting }.ToList();
            var state = await StoreCsrfToken(auth2Client.GenerateCSRFToken());
            string authorizeUrl = auth2Client.GetAuthorizationURL(scopes, state);
            return authorizeUrl;
        }

        public async ThreadingTask HandleAuthCallback(string code, string realmId, string state)
        {
            var tokenResponse = await GetAuth2Client().GetBearerTokenAsync(code);

            await ValidateCsrfToken(state);

            await StoreAccessTokens(tokenResponse);
            await StoreRealmId(realmId);
        }

        private void ValidateTokenResponse(TokenResponse tokenResponse)
        {
            if (tokenResponse.IsError)
            {
                Logger.Error($"ValidateTokenResponse error ({tokenResponse.ErrorType}): {tokenResponse.Raw}");
                throw new UserFriendlyException("QuickBooks connection error, please try again");
            }

            if (string.IsNullOrEmpty(tokenResponse.AccessToken) || string.IsNullOrEmpty(tokenResponse.RefreshToken))
            {
                Logger.Error($"ValidateTokenResponse error, a token is missing: {tokenResponse.Raw}");
                throw new UserFriendlyException("QuickBooks connection error, access token or refresh token is missing");
            }

            //tokenResponse.IdentityToken is null
            //You need to validate all ID tokens on your server unless you know that they came directly from Intuit. 
            //var isTokenValid = await GetAuth2Client().ValidateIDTokenAsync(tokenResponse.IdentityToken);
        }

        private async ThreadingTask StoreAccessTokens(TokenResponse tokenResponse)
        {
            ValidateTokenResponse(tokenResponse);

            var accessTokenExpirationDate = Clock.Now.AddSeconds(tokenResponse.AccessTokenExpiresIn).ToString("u");
            var refreshTokenExpirationDate = Clock.Now.AddSeconds(tokenResponse.RefreshTokenExpiresIn).ToString("u");

            await StoreAccessTokens(tokenResponse.AccessToken, tokenResponse.RefreshToken, accessTokenExpirationDate, refreshTokenExpirationDate);
        }

        private async ThreadingTask StoreAccessTokens(string accessToken, string refreshToken, string accessTokenExpirationDate, string refreshTokenExpirationDate)
        {
            var isConnected = !string.IsNullOrEmpty(refreshToken);
            var tenantId = await AbpSession.GetTenantIdAsync(); // Use async version to prevent blocking
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Invoice.Quickbooks.IsConnected, isConnected.ToString(CultureInfo.InvariantCulture).ToLower(CultureInfo.InvariantCulture));
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Invoice.Quickbooks.AccessToken, accessToken);
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Invoice.Quickbooks.RefreshToken, refreshToken);
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Invoice.Quickbooks.AccessTokenExpirationDate, accessTokenExpirationDate);
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Invoice.Quickbooks.RefreshTokenExpirationDate, refreshTokenExpirationDate);
        }

        private async ThreadingTask StoreRealmId(string realmId)
        {
            //realmId: The ID that identifies the specific QuickBooks company to which a connection is made
            var tenantId = await AbpSession.GetTenantIdAsync(); // Use async version to prevent blocking
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Invoice.Quickbooks.RealmId, realmId);
        }

        private async Task<string> StoreCsrfToken(string csrfToken)
        {
            var tenantId = await AbpSession.GetTenantIdAsync(); // Use async version to prevent blocking
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Invoice.Quickbooks.CsrfToken, csrfToken);
            return csrfToken;
        }

        private async ThreadingTask ValidateCsrfToken(string receivedCsrfToken)
        {
            var storedCsrfToken = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.CsrfToken);
            if (string.IsNullOrEmpty(storedCsrfToken) || storedCsrfToken != receivedCsrfToken)
            {
                throw new UserFriendlyException("Connection to QuickBooks failed", "CSRF token mismatch");
            }
            await StoreCsrfToken("");
        }

        private async Task<ServiceContext> GetServiceContext()
        {
            var accessToken = await GetAccessTokenOrThrow();
            var realmId = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.RealmId);
            var serviceContext = new ServiceContext(realmId, IntuitServicesType.QBO, new OAuth2RequestValidator(accessToken));
            serviceContext.IppConfiguration.MinorVersion.Qbo = "45"; //23
            serviceContext.IppConfiguration.BaseUrl.Qbo = GetQboBaseUrl();
            serviceContext.IppConfiguration.Message.Request.SerializationFormat = Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
            serviceContext.IppConfiguration.Message.Response.SerializationFormat = Intuit.Ipp.Core.Configuration.SerializationFormat.Json;
            return serviceContext;
        }

        public async ThreadingTask ValidateAccessToken()
        {
            await GetRefreshTokenOrThrow(); //check that refresh token exists and is not expired
            await RefreshAccessToken(); //ensure we can obtain a new access token, no need to check that we had the old access token at this point anymore

            var serviceContext = await GetServiceContext();
            var queryService = new QueryService<CompanyInfo>(serviceContext);
            var companyInfo = queryService.ExecuteIdsQuery("SELECT * FROM CompanyInfo").FirstOrDefault();
        }

        public async ThreadingTask RevokeToken()
        {
            try
            {
                var refreshToken = await GetRefreshTokenOrThrow();
                var tokenResponse = await GetAuth2Client().RevokeTokenAsync(refreshToken);
            }
            catch (Exception e)
            {
                if (!(e is UserFriendlyException))
                {
                    Logger.Error(e.ToString());
                }
            }
            await StoreAccessTokens("", "", "", "");
            await StoreRealmId("");
            await StoreCsrfToken("");
        }

        [UnitOfWork(isTransactional: false)]
        public async Task<UploadInvoicesResult> UploadInvoices()
        {
            var invoicesToUpload = await (await _invoiceRepository.GetQueryAsync())
                .Where(x => x.QuickbooksExportDateTime == null && x.Status == InvoiceStatus.ReadyForExport)
                .ToInvoiceToUploadList(await GetTimezone());

            var invoiceNumberPrefix = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.InvoiceNumberPrefix);
            var taxCalculationType = (TaxCalculationType)await SettingManager.GetSettingValueAsync<int>(AppSettings.Invoice.TaxCalculationType);
            if (taxCalculationType == TaxCalculationType.MaterialTotal)
            {
                invoicesToUpload.SplitMaterialAndFreightLines();
            }

            //foreach (var invoice in invoicesToUpload)
            //{
            //    invoice.RecalculateTotals(taxCalculationType);
            //}

            var timezone = await GetTimezone();
            var today = await GetToday();

            await RefreshAccessToken();
            var serviceContext = await GetServiceContext();
            var customerQueryService = new QueryService<Customer>(serviceContext);
            var itemQueryService = new QueryService<Item>(serviceContext);
            var accountQueryService = new QueryService<Account>(serviceContext);
            var termQueryService = new QueryService<Term>(serviceContext);
            var dataService = new DataService(serviceContext);

            var qboSettings = await GetQuickbooksOnlineSettingsOrThrow();

            var taxRateFinder = new QuickbooksTaxRateFinder(serviceContext);

            var result = new UploadInvoicesResult();

            if (!invoicesToUpload.Any(x => x.InvoiceLines.Any()))
            {
                return result;
            }

            Invoices.InvoiceUploadBatch invoiceUploadBatch = null;

            foreach (var model in invoicesToUpload)
            {
                if (!model.InvoiceLines.Any())
                {
                    //Business Rules
                    //An invoice must have at least one Line for either a sales item or an inline subtotal.
                    continue;
                }

                var customer = FindOrCreateCustomer(dataService, customerQueryService, model.Customer);

                var message = model.Message;
                var jobNumber = model.GetJobNumberOnly();
                if (!string.IsNullOrEmpty(jobNumber))
                {
                    message = $"Job Nbr: {jobNumber}\n{message}";
                }


                var invoice = new Invoice
                {
                    Deposit = 0.00M,
                    DepositSpecified = true,
                    DocNumber = invoiceNumberPrefix + model.InvoiceId.ToString(),
                    CustomerRef = new ReferenceType { Value = customer.Id },
                    CustomerMemo = string.IsNullOrEmpty(message) ? null : new MemoRef() { Value = message },
                    //not used by QBO
                    //PONumber = model.GetPoNumberOrJobNumber()?.Truncate(15),
                    TotalAmtSpecified = true,
                    ApplyTaxAfterDiscount = false,
                    ApplyTaxAfterDiscountSpecified = true,
                    PrintStatus = PrintStatusEnum.NotSet,
                    PrintStatusSpecified = true,
                    EmailStatus = EmailStatusEnum.NotSet,
                    EmailStatusSpecified = true,
                    BalanceSpecified = true,
                    TxnDate = model.IssueDate ?? today,
                    TxnDateSpecified = true,
                    BillEmail = GetEmailAddressOrNull(model.EmailAddress),
                    BillAddr = GetPhysicalAddress(model.BillingAddress),
                    GlobalTaxCalculation = GlobalTaxCalculationEnum.NotApplicable,
                    GlobalTaxCalculationSpecified = true,
                };

                //is SalesTermRef required if no DueDate is specified?
                if (model.DueDate.HasValue)
                {
                    invoice.DueDate = model.DueDate.Value;
                    invoice.DueDateSpecified = true;
                }

                if (model.Terms.HasValue)
                {
                    var term = FindOrCreateTerm(dataService, termQueryService, model.Terms.Value);
                    invoice.SalesTermRef = new ReferenceType { Value = term.Id, name = term.Name };
                }

                var lineList = new List<Line>();
                foreach (var lineModel in model.InvoiceLines)
                {
                    var line = new Line
                    {
                        DetailType = LineDetailTypeEnum.SalesItemLineDetail,
                        DetailTypeSpecified = true,
                        Amount = lineModel.Subtotal,
                        AmountSpecified = true,
                        Description = lineModel.DescriptionAndTicketWithTruck,
                        LineNum = lineModel.LineNumber.ToString(),
                    };
                    var qty = lineModel.Quantity;
                    qty = qty == 0 ? 1 : qty;
                    var salesItemLine = new SalesItemLineDetail
                    {
                        Qty = qty,
                        QtySpecified = true,
                        TaxInclusiveAmt = lineModel.ExtendedAmount,
                        TaxInclusiveAmtSpecified = true,
                        TaxCodeRef = new ReferenceType { Value = lineModel.Tax > 0 ? "TAX" : "NON" },
                        AnyIntuitObject = lineModel.Subtotal / qty,
                        ItemElementName = ItemChoiceType.UnitPrice
                    };
                    line.AnyIntuitObject = salesItemLine;

                    if (lineModel.ItemId != null)
                    {
                        //When a line lacks an ItemRef it is treated as documentation and the Line.Amount attribute is ignored.
                        var item = FindOrCreateItem(dataService, itemQueryService, accountQueryService, qboSettings, lineModel);
                        salesItemLine.ItemRef = new ReferenceType { Value = item.Id, name = item.Name };

                        var ticketDate = lineModel.Ticket?.TicketDateTimeUtc?.ConvertTimeZoneTo(timezone).Date;
                        if (ticketDate.HasValue)
                        {
                            salesItemLine.ServiceDate = ticketDate.Value;
                            salesItemLine.ServiceDateSpecified = true;
                        }
                    }

                    //if (string.IsNullOrEmpty(modelLine.TicketNumber))
                    //{
                    //    line.DetailType = LineDetailTypeEnum.DescriptionOnly;
                    //}
                    //salesItemLine.ItemElementName = ItemChoiceType.UnitPrice;

                    lineList.Add(line);
                }
                invoice.Line = lineList.ToArray();

                if (model.Tax > 0)
                {
                    if (model.TaxRate == 0)
                    {
                        throw new UserFriendlyException($"Invoice #{model.InvoiceId} doesn't have TaxRate specified. Please update the invoice and try again");
                    }

                    var taxCodeId = taxRateFinder.GetTaxCodeId(model.TaxRate);
                    if (taxCodeId == null)
                    {
                        throw new UserFriendlyException($"This tax rate ({model.TaxRate}) doesn't exist in QuickBooks. Please add it to QuickBooks and try to upload the invoices again.");
                    }

                    invoice.TxnTaxDetail = new TxnTaxDetail
                    {
                        TxnTaxCodeRef = new ReferenceType { Value = taxCodeId }
                    };
                }

                var success = true;
                try
                {
                    invoice = dataService.Add(invoice);
                }
                catch (IdsException ex)
                {
                    if (ex.Message.Contains("Duplicate Document Number Error"))
                    {
                        success = false;
                        result.ErrorList.Add($"Duplicate Document Number {invoice.DocNumber}");
                    }
                    else
                    {
                        throw;
                    }
                }

                if (success)
                {
                    model.Invoice.QuickbooksExportDateTime = Clock.Now;
                    model.Invoice.QuickbooksInvoiceId = invoice.Id;
                    model.Invoice.Status = InvoiceStatus.Sent;

                    if (invoiceUploadBatch == null)
                    {
                        invoiceUploadBatch = new Invoices.InvoiceUploadBatch { TenantId = AbpSession.TenantId ?? 0 };
                        await _invoiceUploadBatchRepository.InsertAndGetIdAsync(invoiceUploadBatch);
                    }
                    model.Invoice.UploadBatchId = invoiceUploadBatch.Id;
                    result.UploadedInvoicesCount++;
                }

                await CurrentUnitOfWork.SaveChangesAsync();
            }

            await RefreshAccessToken();

            return result;
        }

        public async Task<List<SelectListDto>> GetIncomeAccountSelectList()
        {
            await RefreshAccessToken();
            var serviceContext = await GetServiceContext();
            var accountQueryService = new QueryService<Account>(serviceContext);

            var accounts = accountQueryService.ExecuteIdsQuery("Select * from Account where AccountType = 'Income'").ToList();
            await RefreshAccessToken();

            return accounts
                .Select(x => new SelectListDto
                {
                    Id = x.Id.ToString(),
                    Name = x.Name,
                }).ToList();
        }

        public async ThreadingTask SetQuickbooksOnlineSettings(QuickbooksOnlineSettingsDto input)
        {
            var tenantId = await AbpSession.GetTenantIdAsync(); // Use async version to prevent blocking
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Invoice.Quickbooks.DefaultIncomeAccountId, input.DefaultIncomeAccountId);
            await SettingManager.ChangeSettingForTenantAsync(tenantId, AppSettings.Invoice.Quickbooks.DefaultIncomeAccountName, input.DefaultIncomeAccountName);
        }

        private async Task<QuickbooksOnlineSettingsDto> GetQuickbooksOnlineSettingsOrThrow()
        {
            var result = new QuickbooksOnlineSettingsDto
            {
                DefaultIncomeAccountId = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.DefaultIncomeAccountId),
                DefaultIncomeAccountName = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.DefaultIncomeAccountName),
            };

            if (string.IsNullOrEmpty(result.DefaultIncomeAccountId))
            {
                throw new UserFriendlyException("You must first configure an income account in QuickBook settings to run this.");
            }

            return result;
        }

        private Customer FindOrCreateCustomer(DataService dataService, QueryService<Customer> customerQueryService, CustomerToUploadDto customerModel)
        {
            var customerName = RemoveRestrictedCharacters(customerModel.Name).Truncate(CustomerNameMaxLength);
            if (_customerCache.TryGetValue(customerName, out var customer))
            {
                return customer;
            }

            customer = customerQueryService.ExecuteIdsQuery($"SELECT * FROM Customer WHERE DisplayName = '{EscapeString(customerName)}'").FirstOrDefault();
            if (customer == null)
            {
                customer = new Customer
                {
                    DisplayName = customerName,
                    PrimaryEmailAddr = GetEmailAddressOrNull(customerModel.InvoiceEmail),
                    BillAddr = GetPhysicalAddress(customerModel.BillingAddress),
                    ShipAddr = GetPhysicalAddress(customerModel.ShippingAddress)
                };
                customer = dataService.Add(customer);
                _customerCache.Add(customerName, customer);
            }
            return customer;
        }

        private Item FindOrCreateItem(DataService dataService, QueryService<Item> itemQueryService, QueryService<Account> accountQueryService, QuickbooksOnlineSettingsDto qboSettings, InvoiceLineToUploadDto lineModel)
        {
            var itemName = RemoveRestrictedCharacters(lineModel.ItemName);
            if (_itemCache.TryGetValue(itemName, out var item))
            {
                return item;
            }

            item = itemQueryService.ExecuteIdsQuery($"SELECT * FROM Item WHERE Name = '{EscapeString(itemName)}'").FirstOrDefault();
            if (item == null)
            {
                //var isProduct = ticketModel.ServiceHasMaterialPricing;
                if (lineModel.ItemType == ItemType.InventoryPart)
                {
                    //Exclude "Inventory Part" until someone asks for it as an option.
                    //Inventory parts have more required fields, e.g. AssetAccount, CogsAccount, maybe Cost
                    throw new UserFriendlyException($"Inventory Parts cannot be exported, please ensure that you already have an item with a matching name ({itemName}) in QuickBooks");
                }

                var itemType = GetItemType(lineModel.ItemType);

                if (itemType == ItemTypeEnum.OtherCharge)
                {
                    throw new UserFriendlyException($"Products/Services with type {lineModel.ItemType?.GetDisplayName()} cannot be exported to QBO, please ensure that you already have an item with a matching name ({itemName}) in QuickBooks");
                }

                item = new Item
                {
                    Name = itemName,
                    Type = itemType,
                    TypeSpecified = true,
                    //InvStartDate = today, //required for ItemTypeEnum.Inventory
                    //InvStartDateSpecified = true,
                    //QtyOnHand = 1,
                    //TrackQtyOnHand = false
                    Taxable = lineModel.IsTaxable ?? true,
                    IncomeAccountRef = new ReferenceType
                    {
                        Value = !lineModel.ItemIncomeAccount.IsNullOrEmpty() ? GetAccountOrThrow(accountQueryService, lineModel.ItemIncomeAccount).Id : qboSettings.DefaultIncomeAccountId
                    },
                };
                item = dataService.Add(item);
                _itemCache.Add(itemName, item);
            }
            return item;
        }



        private Account GetAccountOrThrow(QueryService<Account> accountQueryService, string accountName)
        {
            if (!_accountCache.Any())
            {
                var apiAccounts = accountQueryService.ExecuteIdsQuery("SELECT * FROM Account where AccountType = 'Income'").ToList();
                foreach (var apiAccount in apiAccounts)
                {
                    if (!_accountCache.ContainsKey(apiAccount.Name.ToLowerInvariant()))
                    {
                        _accountCache.Add(apiAccount.Name.ToLowerInvariant(), apiAccount);
                    }
                }
            }

            if (_accountCache.TryGetValue(accountName.ToLowerInvariant(), out var account))
            {
                return account;
            }

            throw new UserFriendlyException($"Income Account {accountName} wasn't found in QuickBooks");
        }

        private Term FindOrCreateTerm(DataService dataService, QueryService<Term> termQueryService, BillingTermsEnum invoiceTerm)
        {
            var invoiceTermName = invoiceTerm.GetDisplayName();
            if (!_termCache.Any())
            {
                var apiTerms = termQueryService.ExecuteIdsQuery($"SELECT * FROM Term").ToList();
                foreach (var apiTerm in apiTerms)
                {
                    _termCache.Add(apiTerm.Name.ToLowerInvariant(), apiTerm);
                }
            }

            if (_termCache.TryGetValue(invoiceTermName.ToLowerInvariant(), out var term))
            {
                return term;
            }

            term = GetNewTerm(invoiceTerm);
            term = dataService.Add(term);
            _termCache.Add(invoiceTermName.ToLowerInvariant(), term);

            return term;
        }

        private Term GetNewTerm(BillingTermsEnum invoiceTerm)
        {
            var term = new Term
            {
                Name = invoiceTerm.GetDisplayName(),
            };

            switch (invoiceTerm)
            {
                case BillingTermsEnum.DueOnReceipt:
                case BillingTermsEnum.Net5:
                case BillingTermsEnum.Net10:
                case BillingTermsEnum.Net14:
                case BillingTermsEnum.Net15:
                case BillingTermsEnum.Net30:
                case BillingTermsEnum.Net60:
                    term.Type = SalesTermTypeEnum.Standard.ToString();
                    term.ItemsElementName = new[] {
                        ItemsChoiceType.DueDays,
                        ItemsChoiceType.DiscountDays
                    };
                    term.AnyIntuitObjects = new object[] {
                        GetDueDaysForTerm(invoiceTerm),
                        0
                    };
                    break;
                case BillingTermsEnum.DueByTheFirstOfTheMonth:
                    term.Type = SalesTermTypeEnum.DateDriven.ToString();
                    term.ItemsElementName = new[] {
                        ItemsChoiceType.DayOfMonthDue, //Payment must be received by this day of the month. Used only if DueDays is not specified. Required if DueDays not present
                        ItemsChoiceType.DiscountDayOfMonth, //Discount applies if paid before this day of month. Required if DueDays not present
                        ItemsChoiceType.DueNextMonthDays //Payment due next month if issued that many days before the DayOfMonthDue. Required if DueDays not present.
                    };
                    term.AnyIntuitObjects = new object[] {
                        1,
                        1,
                        0
                    };
                    break;
            }

            return term;
        }

        private int GetDueDaysForTerm(BillingTermsEnum invoiceTerm)
        {
            switch (invoiceTerm)
            {
                default:
                case BillingTermsEnum.DueOnReceipt:
                    return 0;
                case BillingTermsEnum.Net5:
                    return 5;
                case BillingTermsEnum.Net10:
                    return 10;
                case BillingTermsEnum.Net14:
                    return 14;
                case BillingTermsEnum.Net15:
                    return 15;
                case BillingTermsEnum.Net30:
                    return 30;
                case BillingTermsEnum.Net60:
                    return 60;
            }
        }

        private string EscapeString(string val)
        {
            return val?.Replace("'", @"\'");
        }

        private string RemoveRestrictedCharacters(string val)
        {
            return val?.Replace(":", "").Replace("–", "-").Replace("\t", "").Replace("\r", "").Replace("\n", "");
        }

        private PhysicalAddress GetPhysicalAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }
            var addressLines = address.Replace("\r", "").Split("\n");
            var result = new PhysicalAddress
            {
                Line1 = addressLines.FirstOrDefault(),
                Line2 = addressLines.Skip(1).FirstOrDefault(),
                Line3 = addressLines.Skip(2).FirstOrDefault(),
                Line4 = addressLines.Skip(3).FirstOrDefault(),
                Line5 = addressLines.Skip(4).FirstOrDefault()
            };
            return result;
        }

        private PhysicalAddress GetPhysicalAddress(PhysicalAddressDto dto)
        {
            return new PhysicalAddress
            {
                City = dto.City,
                CountryCode = dto.CountryCode,
                CountrySubDivisionCode = dto.State,
                Line1 = dto.Address1,
                Line2 = dto.Address2,
                PostalCode = dto.ZipCode
            };
        }

        private EmailAddress GetEmailAddressOrNull(string emailAddress)
        {
            return string.IsNullOrEmpty(emailAddress) ? null : new EmailAddress { Address = emailAddress };
        }

        public static ItemTypeEnum GetItemType(ItemType? ItemType)
        {
            switch (ItemType)
            {
                case ItemType.Discount:
                    return ItemTypeEnum.Discount;
                case ItemType.InventoryAssembly:
                    return ItemTypeEnum.Assembly;
                case ItemType.InventoryPart:
                    return ItemTypeEnum.Inventory;
                case ItemType.NonInventoryPart:
                    return ItemTypeEnum.NonInventory;
                case ItemType.OtherCharge:
                    return ItemTypeEnum.OtherCharge;
                case ItemType.Payment:
                    return ItemTypeEnum.Payment;
                case ItemType.SalesTaxItem:
                    return ItemTypeEnum.Tax;
                case ItemType.Service:
                    return ItemTypeEnum.Service;
                default:
                case ItemType.System:
                    //return null;
                    return ItemTypeEnum.OtherCharge;
            }
        }

        private async Task<string> RefreshAccessToken()
        {
            //You must make a RefreshToken call after a 401 error (Invalid Token error) to get a new access token, or call RefreshToken every every hour since the Access Token expires in one hour.
            var refreshToken = await GetRefreshTokenOrThrow();
            var tokenResponse = await GetAuth2Client().RefreshTokenAsync(refreshToken);
            await StoreAccessTokens(tokenResponse);
            return tokenResponse.AccessToken;
        }

        private async Task<string> GetAccessTokenOrThrow()
        {
            var accessToken = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.AccessToken);

            if (!string.IsNullOrEmpty(accessToken))
            {
                var accessTokenExpirationDate = await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.Invoice.Quickbooks.AccessTokenExpirationDate);
                if (accessTokenExpirationDate > Clock.Now.AddMinutes(1))
                {
                    return accessToken;
                }

                return await RefreshAccessToken();
            }

            throw new UserFriendlyException("Not connected to QuickBooks: access token is missing");
        }

        private async Task<string> GetRefreshTokenOrThrow()
        {
            var refreshToken = await SettingManager.GetSettingValueAsync(AppSettings.Invoice.Quickbooks.RefreshToken);

            if (!string.IsNullOrEmpty(refreshToken))
            {
                var refreshTokenExpirationDate = await SettingManager.GetSettingValueAsync<DateTime>(AppSettings.Invoice.Quickbooks.RefreshTokenExpirationDate);
                if (refreshTokenExpirationDate > Clock.Now.AddSeconds(15))
                {
                    return refreshToken;
                }

                throw new UserFriendlyException("Not connected to QuickBooks: refresh token has expired");
            }

            throw new UserFriendlyException("Not connected to QuickBooks: refresh token is missing");
        }

        private OAuth2Client GetAuth2Client()
        {
            var clientId = _appConfiguration["Quickbooks:ClientId"];
            var clientSecret = _appConfiguration["Quickbooks:ClientSecret"];
            var environment = _appConfiguration["Quickbooks:Environment"];
            var redirectUrl = GetAuthCallbackUrl();
            return new OAuth2Client(clientId, clientSecret, redirectUrl, environment);
        }

        private string GetQboBaseUrl()
        {
            var environment = _appConfiguration["Quickbooks:Environment"];
            switch (environment)
            {
                case "sandbox":
                    return "https://sandbox-quickbooks.api.intuit.com";
                case "production":
                    return "https://quickbooks.api.intuit.com/";
                default:
                    throw new ApplicationException("Unexpected 'Quickbooks:Environment' value: " + environment);
            }
        }

        private string GetAuthCallbackUrl()
        {
            string siteUrl = _webUrlService.GetSiteRootAddress();
            return $"{siteUrl}app/quickbooks/callback";
        }
    }

}
*/
