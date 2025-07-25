using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Collections.Extensions;
using Abp.Domain.Repositories;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.Configuration;
using DispatcherWeb.Payments.Dto;
using GlobalPayments.Api.Entities;
using GlobalPayments.Api.PaymentMethods;
using GlobalPayments.Api.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

namespace DispatcherWeb.Payments
{
    [AbpAuthorize(AppPermissions.Pages_Orders_Edit)]
    public class PaymentAppService : DispatcherWebAppServiceBase, IPaymentAppService
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfigurationRoot _appConfiguration;
        private readonly IRepository<Payment> _paymentRepository;
        private readonly IRepository<PaymentHeartlandKey> _paymentHeartlandKeyRepository;
        private readonly IOfficeSettingsManager _officeSettingsManager;

        public PaymentAppService(
            IWebHostEnvironment hostingEnvironment,
            IAppConfigurationAccessor configurationAccessor,
            IRepository<Payment> paymentRepository,
            IRepository<PaymentHeartlandKey> paymentHeartlandKeyRepository,
            IOfficeSettingsManager officeSettingsManager
            )
        {
            _env = hostingEnvironment;
            _appConfiguration = configurationAccessor.Configuration;
            _paymentRepository = paymentRepository;
            _paymentHeartlandKeyRepository = paymentHeartlandKeyRepository;
            _officeSettingsManager = officeSettingsManager;
        }

        public async Task<AuthorizeChargeResult> AuthorizeCharge(AuthorizeChargeDto input)
        {
            Logger.Info("Entered AuthorizeCharge with input: " + SerializeToJson(input));

            ValidateAmount(input.AuthorizationAmount);

            try
            {
                await ConfigureServiceAsync();

                var address = new Address
                {
                    PostalCode = input.ZipCode,
                    StreetAddress1 = input.StreetAddress,
                };

                if (!input.NewCreditCardTempToken.IsNullOrEmpty())
                {
                    var newCard = new CreditCardData
                    {
                        Token = input.NewCreditCardTempToken,
                    };

                    Logger.Info("Sending request to verify card and issue new multi-use token");

                    var getMultiUseTokenResponse = newCard.Verify()
                        .WithRequestMultiUseToken(true)
                        .WithAddress(address)
                        .Execute();

                    CheckResponseCode(getMultiUseTokenResponse);

                    input.CreditCardToken = getMultiUseTokenResponse.Token;
                }

                var card = new CreditCardData
                {
                    Token = input.CreditCardToken,
                };

                Logger.Info("Sending request to authorize order charge");

                var response = card
                    .Authorize(input.AuthorizationAmount)
                    .WithCurrency("USD")
                    .WithAddress(address)
                    .WithDescription(input.PaymentDescription)
                    .WithAllowDuplicates(false)
                    .Execute();

                CheckResponseCode(response);

                var payment = new Payment
                {
                    CreditCardToken = input.CreditCardToken,
                    CreditCardStreetAddress = input.StreetAddress,
                    CreditCardZipCode = input.ZipCode,
                    AuthorizationDateTime = DateTime.UtcNow,
                    AuthorizationAmount = input.AuthorizationAmount,
                    AuthorizationTransactionId = response.TransactionId,
                    PaymentDescription = input.PaymentDescription,
                    TenantId = await Session.GetTenantIdAsync(),
                    AuthorizationUserId = Session.UserId,
                };

                await _paymentRepository.InsertAsync(payment);
                await CurrentUnitOfWork.SaveChangesAsync();

                return new AuthorizeChargeResult
                {
                    AuthorizationDateTime = payment.AuthorizationDateTime,
                    PaymentId = payment.Id,
                };
            }
            catch (GatewayException e)
            {
                Logger.Error($"GatewayException: {SerializeToJson(e)}", e);

                if (string.IsNullOrEmpty(e.ResponseCode) && string.IsNullOrEmpty(e.ResponseMessage))
                {
                    throw new UserFriendlyException("We are currently unable to reach the payment processor. Please try back later.", e.Message);
                }

                throw new UserFriendlyException("Unexpected Gateway Response",
                    string.Join(" - ", new[] { e.ResponseCode, e.ResponseMessage }));
            }
            catch (ApiException e)
            {
                Logger.Error($"ApiException: {SerializeToJson(e)}", e);

                if (_env.IsDevelopment())
                {
                    throw new UserFriendlyException("Operation completed unsuccessfully", "Error details: " + e.Message, e);
                }

                throw;
            }
        }

        public async Task CancelAuthorization(EntityDto input)
        {
            var payment = await _paymentRepository.GetAsync(input.Id);

            if (payment.AuthorizationCaptureTransactionId != null)
            {
                throw new UserFriendlyException("Payment has already been finalized", "Please refresh the page and try again.");
            }

            try
            {
                await ConfigureServiceAsync();

                var response = Transaction
                    .FromId(payment.AuthorizationTransactionId)
                    .Reverse(payment.AuthorizationAmount ?? 0)
                    .WithDescription(payment.PaymentDescription)
                    .Execute();

                CheckResponseCode(response);

                payment.IsCancelledOrRefunded = true;
                payment.CancelOrRefundUserId = Session.UserId;
            }
            catch (GatewayException e)
            {
                Logger.Error($"GatewayException: {SerializeToJson(e)}", e);

                if (string.IsNullOrEmpty(e.ResponseCode) && string.IsNullOrEmpty(e.ResponseMessage))
                {
                    throw new UserFriendlyException("We are currently unable to reach the payment processor. Please try back later.", e.Message);
                }

                throw new UserFriendlyException("Unexpected Gateway Response",
                    string.Join(" - ", new[] { e.ResponseCode, e.ResponseMessage }));
            }
            catch (ApiException e)
            {
                if (_env.IsDevelopment())
                {
                    throw new UserFriendlyException("Operation completed unsuccessfully", "Error details: " + e.Message, e);
                }
                throw;
            }
        }

        public async Task<CaptureAuthorizationResult> CaptureAuthorization(CaptureAuthorizationDto input)
        {
            var payment = await _paymentRepository.GetAsync(input.PaymentId);

            if (payment.AuthorizationTransactionId.IsNullOrEmpty())
            {
                throw new UserFriendlyException("Payment is not authorized", "Please refresh the page and try again.");
            }

            if (!payment.AuthorizationCaptureTransactionId.IsNullOrEmpty())
            {
                throw new UserFriendlyException("Payment is not authorized", "Please refresh the page and try again.");
            }

            ValidateAmount(input.ActualAmount);

            try
            {
                await ConfigureServiceAsync();

                var captureResponse = Transaction
                    .FromId(payment.AuthorizationTransactionId)
                    .Capture(input.ActualAmount)
                    .WithDescription(payment.PaymentDescription)
                    .Execute();

                CheckResponseCode(captureResponse);

                payment.AuthorizationCaptureDateTime = DateTime.UtcNow;
                payment.AuthorizationCaptureAmount = input.ActualAmount;
                //order.AuthorizationCaptureSettlementAmount = captureResponse.SettlementAmount;
                payment.AuthorizationCaptureTransactionId = captureResponse.TransactionId;
                payment.AuthorizationCaptureResponse = JsonConvert.SerializeObject(captureResponse);
                payment.AuthorizationCaptureUserId = Session.UserId;

                return new CaptureAuthorizationResult
                {
                    AuthorizationCaptureDateTime = payment.AuthorizationCaptureDateTime,
                };
            }
            catch (GatewayException e)
            {
                Logger.Error($"GatewayException: {SerializeToJson(e)}", e);

                if (string.IsNullOrEmpty(e.ResponseCode) && string.IsNullOrEmpty(e.ResponseMessage))
                {
                    throw new UserFriendlyException("We are currently unable to reach the payment processor. Please try back later.", e.Message);
                }

                throw new UserFriendlyException("Unexpected Gateway Response",
                    string.Join(" - ", new[] { e.ResponseCode, e.ResponseMessage }));
            }
            catch (ApiException e)
            {
                if (_env.IsDevelopment())
                {
                    throw new UserFriendlyException("Operation completed unsuccessfully", "Error details: " + e.Message, e);
                }
                throw;
            }
        }

        public async Task RefundPayment(EntityDto input)
        {
            var payment = await _paymentRepository.GetAsync(input.Id);

            if (payment.AuthorizationCaptureTransactionId.IsNullOrEmpty())
            {
                throw new UserFriendlyException("You can't refund a payment unless the payment was finalized", "Please refresh the page and try again.");
            }

            try
            {
                await ConfigureServiceAsync();

                var refundAmount = payment.AuthorizationCaptureAmount;

                var refundResponse = Transaction
                    .FromId(payment.AuthorizationCaptureTransactionId)
                    .Refund(refundAmount)
                    .WithCurrency("USD")
                    .WithDescription(payment.PaymentDescription)
                    .Execute();

                CheckResponseCode(refundResponse);

                payment.IsCancelledOrRefunded = true;
                payment.CancelOrRefundUserId = Session.UserId;
            }
            catch (GatewayException e)
            {
                Logger.Error($"GatewayException: {SerializeToJson(e)}", e);

                if (string.IsNullOrEmpty(e.ResponseCode) && string.IsNullOrEmpty(e.ResponseMessage))
                {
                    throw new UserFriendlyException("We are currently unable to reach the payment processor. Please try back later.", e.Message);
                }

                throw new UserFriendlyException("Unexpected Gateway Response",
                    string.Join(" - ", new[] { e.ResponseCode, e.ResponseMessage }));
            }
            catch (ApiException e)
            {
                if (_env.IsDevelopment())
                {
                    throw new UserFriendlyException("Operation completed unsuccessfully", "Error details: " + e.Message, e);
                }
                throw;
            }
        }

        //Commented out on upgrade to NET6 since we don't need it anyway. We just need to be sure it doesn't break receipts.
        // We probably need to mock some behavior to have to allow them to do receipts without posting to a payment service
        private void ConfigureServiceForSecretKey(string secretKey)
        {
            throw new UserFriendlyException("Payment Services are unavailable");
            //ServicesContainer.ConfigureService(new GatewayConfig
            //{
            //    SecretApiKey = secretKey,
            //    DeveloperId = _appConfiguration["App:App.Heartland.DeveloperId"],
            //    VersionNumber = _appConfiguration["App:App.Heartland.VersionNumber"],
            //    ServiceUrl = _appConfiguration["App:App.Heartland.ServiceUrl"]
            //});
        }

        private async Task ConfigureServiceAsync()
        {
            Logger.Info("Entered ConfigureServiceAsync for office id " + Session.OfficeId);

            var secretKey = await _officeSettingsManager.GetHeartlandSecretKeyAsync();

            if (secretKey.IsNullOrEmpty())
            {
                Logger.Warn("Heartland Secret API Key is null or empty");
            }

            //Commented on .NET 6 upgrade.
            throw new UserFriendlyException("Payment Services are unavailable");
            //ServicesContainer.ConfigureService(new GatewayConfig
            //{
            //    SecretApiKey = secretKey,
            //    DeveloperId = _appConfiguration["App:App.Heartland.DeveloperId"],
            //    VersionNumber = _appConfiguration["App:App.Heartland.VersionNumber"],
            //    ServiceUrl = _appConfiguration["App:App.Heartland.ServiceUrl"]
            //});
        }

        private void CheckResponseCode(Transaction transaction)
        {
            Logger.Info("Validating response code: " + SerializeToJson(transaction));

            if (transaction.ResponseCode != "00")
            {
                var errorCodeAndMessage = $"{transaction.ResponseCode} - {transaction.ResponseMessage}";
                var errorDescription = GetUserFriendlyErrorDescription(transaction);

                Logger.Error($"CheckResponseCode: {errorCodeAndMessage}");

                if (errorDescription != null)
                {
                    throw new UserFriendlyException($"Error {errorCodeAndMessage}", errorDescription);
                }

                Logger.Error($"No UserFriendlyDescription for ResponseCode {transaction.ResponseCode}");

                throw new UserFriendlyException($"Unexpected error {errorCodeAndMessage}", "Please contact the developers");
            }
        }

        private static string GetUserFriendlyErrorDescription(Transaction transaction)
        {
            //return null if the error is unexpected and we don't have a user friendly exception for it
            //return an empty string if ResponseMessage would contain enough details on itself
            switch (transaction.ResponseCode)
            {
                //case "00": //APPROVAL

                case "02": //CALL
                    return "The issuing bank prevented the transaction. Ask the customer to call their credit card bank and figure out why the transaction was declined.";

                case "03": //TERM ID ERROR - Configuration error / credit card type is not supported?
                    return null;

                case "04": //HOLD-CALL
                    return transaction.ResponseMessage?.ToLower().Contains("hold") == true
                        ? "Retain card. Usually returned when the Issuer would like the merchant to take possession of the card due to potential fraud."
                        : ""; //Can also be returned if the transaction declines due to an AVS/CVV setting. The response text in this case is “DO NOT HONOR DUE TO AVS/CVV SETTINGS”.

                case "05": //DECLINE
                    return "Do not honor. Normally occurs when cardholder has exceeded their allowable credit line.";

                case "06": //ERROR - merchant closed, no match.
                    return null;

                case "07": //HOLD-CALL
                    return transaction.ResponseMessage?.ToLower().Contains("hold") == true
                        ? "Retain card."
                        : null;

                case "09": //NO ORIGINAL - Incremental or Void doesn't reference an original transaction.
                    return null;

                case "10": //PARTIAL APPROVAL
                    return null;

                case "12": //INVALID TRANS
                    return "Invalid transaction.";

                case "13": //AMOUNT ERROR
                    return "Please make sure the amount you entered is a positive number.";

                case "14": //CARD NO. ERROR
                    return "Issuer cannot find the account. Please double check the card number and try again.";

                case "15": //NO SUCH ISSUER.
                    return "The first six digits of the card are not recognized by the Issuer. Please double check the card number and try again.";

                case "19": //RE ENTER
                    return "Please try again.";

                case "25": //INVALID ICC DATA - Required data for processing chip transactions was missing from the authorization request or data could not be parsed
                    return null; //shouldn't happen since we are not working with the card chip

                case "41": //HOLD-CALL - lost card.
                    return "This card was reported as lost. You should retain the card and call the phone number on the back of the card to report the decline.";

                case "43": //HOLD-CALL - stolen card.
                    return "This card was reported as stolen. You should retain the card and call the phone number on the back of the card to report the decline.";

                case "44": //HOLD-CALL - pick up card.
                    return "The card has been lost, stolen, or otherwise flagged for pickup by the issuing bank. If you have the physical card in your possession, you should not return the card to the customer. You should retain the card and call the phone number on the back of the card to report the decline.";

                case "51": //DECLINE
                    return "Insufficient funds.";

                case "52": //NO CHECK ACCOUNT
                    return "Debit/check card being attempted is not linked to a Checking Account.";

                case "53": //NO SAVE ACCOUNT
                    return "Debit/check card being used is not tied to a Savings Account.";

                case "54": //EXPIRED CARD—card is expired. This response can also be returned in a Card Not Present environment if the cardholder tries to provide a valid expiration date, but the Issuer knows it is expired (indicates potential fraud).
                    return "Please make sure the card expiration date is correct and not in the past";

                case "55": //WRONG PIN. Occurs in PIN-based Debit when the consumer enters the wrong 4-digit PIN.
                    return null;

                case "56": //INVALID TRANS / INVALID CARD
                    return null;

                case "57": //SERV NOT ALLOWED - service not allowed. Can be an incorrect MID or terminal number, or attempt to process an unsupported card.
                    return "This card is not configured for that type of transaction or unsupported card.";

                case "58": //SERV NOT ALLOWED
                    return "Service not allowed. Occurs when the POS attempts a transaction type that they are not set up for based on their MCC. (i.e., a merchant set up with a Direct Marketing MCC trying to perform a Debit transaction).";

                case "61": //DECLINE. Occurs in PIN-based debit when the cardholder has exceeded their withdrawal limit when performing cash back.
                    return "Cardholder has exceeded their withdrawal limit when performing cash back.";

                case "62": //DECLINE. Occurs on swiped transactions when the Service Code encoded on the mag stripe does not equal the one stored at the Issuer (potential fraudulent card).
                    return "Service Code encoded on the mag stripe does not equal the one stored at the Issuer (potential fraudulent card).";

                case "63": //SEC VIOLATION
                    return "The three-digit CVV2 or four-digit CID code on the back of the credit card wasn’t read correctly.";

                case "65": //DECLINE
                    return "Activity limit. Cardholder has exceeded the number of times the card can be used in a specific time period.";

                case "75": //PIN EXCEEDED
                    return "Number of attempts to enter the PIN has been exceeded.";

                case "76": //NO ACTION TAKEN
                    return "Reversal data in the POS transaction does not match the Issuer data.";

                case "77": //NO ACTION TAKEN
                    return "Duplicate reversal or duplicate transaction.";

                case "78": //NO ACCOUNT
                    return "Account suspended, cancelled, or inactive.";

                case "80": //DATE ERROR
                    return null;

                case "82": //CASHBACK NO APP
                    return null;

                //case "85": //CARD OK
                //    return null;

                case "86": //CANT VERIFY PIN
                    return "";

                case "91": //NO REPLY
                    return "Time out. We are currently unable to reach the payment processor. Please try back later.";

                case "94": //DUPLICATE TRANSACTION
                    return "Transaction entered is a duplicate on the Host.";

                case "96": //SYSTEM ERROR
                    return "A temporary error occurred during the transaction. Wait a minute or two and try again. Contact your payment processor if it still didn’t work.";

                case "97": //TRANSLATE ERROR
                    return "Decryption error: Contact Customer Service.";

                case "CA": //AVS Referral
                    return null;

                case "EB": //CHECK DIGIT ERR
                    return "Please make sure the card number is correct";

                case "EC": //CID FORMAT ERROR - format error
                    return null;

                case "FR": //FRAUD
                    return "Transaction declined because possible fraud was detected by Heartland.";

                case "N5": //MUST CLOSE BATCH—(GSAP)
                    return "Terminal has not been balanced within time specified by Global Payments for this merchant. Send a batch close request to resume processing.";

                case "N7": //CVV2 MISMATCH
                    return "Incorrect number of CVV2/CID digits sent";

                case "PD": //PARAMETER DOWNLOAD - EMV PDL system response. Response text indicates EMV PDL status code.
                    return null;

                case "R0": //STOP SPECIFIC
                    return "Stop a specific recurring payment";

                case "R1": //REVOKE AUTH
                    return "Revoke authorization for further recurring payments";

                case "R3": //CANCEL PAYMENT
                    return "Cancel all recurring payments for the card number in the request.";

                default:
                    return null;
            }
        }

        private string SerializeToJson(ApiException e)
        {
            if (e is GatewayException ge)
            {
                return SerializeToJson(new
                {
                    ge.ResponseCode,
                    ge.ResponseMessage,
                    ge.Message,
                    InnerExceptionMessage = ge.InnerException?.Message,
                });
            }

            return SerializeToJson(new
            {
                e.Message,
                InnerExceptionMessage = e.InnerException?.Message,
            });
        }

        private string SerializeToJson(object value)
        {
            return JsonConvert.SerializeObject(value, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            });
        }

        private static void ValidateAmount(decimal? amount)
        {
            if (!amount.HasValue)
            {
                throw new UserFriendlyException("Entered empty amount");
            }

            if (Math.Round(amount.Value, 2) != amount.Value)
            {
                throw new UserFriendlyException($"Amount {amount} is invalid, you can only have 2 decimal places at most",
                    "Please make sure you are not entering 3 or more digits after a decimal point");
            }
        }

        public async Task UpdatePaymentsFromHeartland(UpdatePaymentsFromHeartlandInput input)
        {
            var payments = await (await _paymentRepository.GetQueryAsync())
                .Where(x => x.AuthorizationCaptureDateTime >= input.StartDate
                    && x.AuthorizationCaptureDateTime < input.EndDate.AddDays(1)
                    || x.AuthorizationDateTime >= input.StartDate
                    && x.AuthorizationDateTime < input.EndDate.AddDays(1)
                    || x.CreationTime >= input.StartDate
                    && x.CreationTime < input.EndDate.AddDays(1))
                .ToListAsync();

            var apiKeys = await _officeSettingsManager.GetHeartlandKeysForOffices();

            var apiKeyGroups = apiKeys
                .Where(x => !string.IsNullOrEmpty(x.PublicKey) && !string.IsNullOrEmpty(x.SecretKey))
                .WhereIf(!input.AllOffices, x => x.OfficeId == Session.OfficeId)
                .GroupBy(x => new { x.PublicKey, x.SecretKey });

            foreach (var apiKeyGroup in apiKeyGroups)
            {
                ConfigureServiceForSecretKey(apiKeyGroup.Key.SecretKey);
                var heartlandPublicKeyId = await GetHeartlandPublicKeyIdAsync(apiKeyGroup.Key.PublicKey);

                try
                {
                    var transactions = ReportingService.FindTransactions()
                        .WithStartDate(input.StartDate)
                        .WithEndDate(input.EndDate.AddDays(1))
                        .Execute();

                    foreach (var transaction in transactions)
                    {
                        Payment matchingPayment = null;

                        switch (transaction.ServiceName)
                        {
                            //auth or captured auth, both matched with AuthorizationTransactionId, not AuthorizationCaptureTransactionId
                            case "CreditAuth":
                            //payment without pre-authorization?
                            case "CreditSale":
                                matchingPayment = payments.FirstOrDefault(x => x.AuthorizationTransactionId == transaction.TransactionId);

                                //ensure we don't have a transaction already
                                matchingPayment ??= await FindPaymentByAnyTransactionIdAsync(transaction);

                                if (matchingPayment != null)
                                {
                                    matchingPayment.FillSummaryFieldsFrom(transaction);
                                }
                                else
                                {
                                    var newPayment = new Payment
                                    {
                                        TenantId = await Session.GetTenantIdAsync(),
                                        PaymentHeartlandKeyId = heartlandPublicKeyId,
                                    }.FillAuthorizationFieldsFrom(transaction)
                                     .FillSummaryFieldsFrom(transaction);

                                    if (transaction.ServiceName == "CreditSale")
                                    {
                                        newPayment.FillAuthorizationCaptureFieldsFrom(transaction);
                                    }
                                    else if (transaction.Status == "A") //authorized or captured
                                    {
                                        if (transaction.BatchSequenceNumber != null || transaction.SettlementAmount > 0)
                                        {
                                            //captured
                                            newPayment.FillAuthorizationCaptureFieldsFrom(transaction);
                                        }
                                    }
                                    else if (transaction.Status == "R")
                                    {
                                        //authorization cancelled
                                        newPayment.IsCancelledOrRefunded = true;
                                    }
                                    else if (transaction.Status == "I")
                                    {
                                        //authorization declined, ignore
                                        continue;
                                    }
                                    else
                                    {
                                        Logger.Error("Unknown heartland transaction: " + JsonConvert.SerializeObject(transaction));
                                        //ignore unknown transactions
                                        continue;
                                    }

                                    await _paymentRepository.InsertAsync(newPayment);
                                }
                                break;

                            //get multiuse token request, ignore
                            case "CreditAccountVerify":
                                break;

                            //cancelled auth
                            case "CreditReversal":
                                matchingPayment = payments.FirstOrDefault(x => x.AuthorizationTransactionId == transaction.OriginalTransactionId);
                                matchingPayment ??= await FindPaymentByAnyTransactionIdAsync(transaction);

                                if (matchingPayment != null)
                                {
                                    matchingPayment.FillSummaryFieldsFrom(transaction);
                                    matchingPayment.IsCancelledOrRefunded = true;
                                }
                                break;

                            //refund
                            case "CreditReturn":
                                matchingPayment = payments.FirstOrDefault(x => x.AuthorizationTransactionId == transaction.OriginalTransactionId);
                                matchingPayment ??= await FindPaymentByAnyTransactionIdAsync(transaction);

                                if (matchingPayment != null)
                                {
                                    matchingPayment.FillSummaryFieldsFrom(transaction);
                                    matchingPayment.IsCancelledOrRefunded = true;
                                }
                                break;

                            default:
                                Logger.Error("Unknown heartland transaction: " + JsonConvert.SerializeObject(transaction));
                                break;
                        }

                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                }
                catch (GatewayException e)
                {
                    Logger.Error($"GatewayException: {SerializeToJson(e)}", e);

                    if (string.IsNullOrEmpty(e.ResponseCode) && string.IsNullOrEmpty(e.ResponseMessage))
                    {
                        throw new UserFriendlyException("We are currently unable to reach the payment processor. Please try back later.", e.Message);
                    }

                    throw new UserFriendlyException("Unexpected Gateway Response",
                        string.Join(" - ", new[] { e.ResponseCode, e.ResponseMessage }));
                }
                catch (ApiException e)
                {
                    if (_env.IsDevelopment())
                    {
                        throw new UserFriendlyException("Operation completed unsuccessfully", "Error details: " + e.Message, e);
                    }
                    throw;
                }
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<int> GetHeartlandPublicKeyIdAsync()
        {
            var heartlandPublicKey = await _officeSettingsManager.GetHeartlandPublicKeyAsync();
            return await GetHeartlandPublicKeyIdAsync(heartlandPublicKey);
        }

        public async Task<int> GetHeartlandPublicKeyIdAsync(string heartlandPublicKey)
        {
            var existingRecord = await (await _paymentHeartlandKeyRepository.GetQueryAsync()).FirstOrDefaultAsync(x => x.PublicKey == heartlandPublicKey);
            if (existingRecord != null)
            {
                return existingRecord.Id;
            }
            return await _paymentHeartlandKeyRepository.InsertAndGetIdAsync(new PaymentHeartlandKey { PublicKey = heartlandPublicKey });
        }

        private async Task<Payment> FindPaymentByAnyTransactionIdAsync(TransactionSummary transaction)
        {
            return await (await _paymentRepository.GetQueryAsync()).FirstOrDefaultAsync(x =>
                                    x.AuthorizationTransactionId == transaction.TransactionId
                                    || x.AuthorizationTransactionId == transaction.OriginalTransactionId
                                    || x.AuthorizationCaptureTransactionId == transaction.TransactionId
                                    || x.AuthorizationCaptureTransactionId == transaction.OriginalTransactionId);
        }
    }
}
