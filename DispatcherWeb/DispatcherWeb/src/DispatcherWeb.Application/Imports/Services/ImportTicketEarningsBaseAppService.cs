using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Linq.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization.Users;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Features;
using DispatcherWeb.Imports.Dto;
using DispatcherWeb.Imports.RowReaders;
using DispatcherWeb.Infrastructure.AzureBlobs;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulerRequests;
using DispatcherWeb.Locations;
using DispatcherWeb.LuckStone;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets;
using DispatcherWeb.Tickets.Dto;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    [AbpAuthorize]
    public abstract class ImportTicketEarningsBaseAppService : ImportDataBaseAppService<TicketEarningsImportRow>
    {
        private readonly IRepository<ImportedEarnings> _importedEarningsRepository;
        private readonly IRepository<ImportedEarningsBatch> _importedEarningsBatchRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<AvailableLeaseHaulerTruck> _availableLeaseHaulerTruckRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<UnitOfMeasure> _uomRepository;
        private readonly IRepository<Item> _itemRepository;
        protected readonly IRepository<Location> _locationRepository;
        private readonly IRepository<LocationCategory> _locationCategoryRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly ISecureFileBlobService _secureFileBlobService;
        private readonly UserManager _userManager;
        private string _filePath = null;
        private int? _officeId = null;
        private bool _useShifts;
        private Shift? _shift;
        private bool _useForProductionPay;
        private int _customerId;
        private string _haulerRef;
        protected Dictionary<string, int> _loadAtLocations;
        protected Dictionary<string, int> _deliverToLocations;
        private Dictionary<string, int> _items;
        private Dictionary<int, string> _uoms;
        private Dictionary<(int truckId, DateTime date), int?> _driversForTrucks;
        private int _temporaryLocationCategoryId;
        private int _timeClassificationId;
        private ImportedEarningsBatch _importedEarningsBatch = null;
        private List<ImportedEarnings> _importedEarnings = null;

        public ImportTicketEarningsBaseAppService(
            IRepository<ImportedEarnings> importedEarningsRepository,
            IRepository<ImportedEarningsBatch> importedEarningsBatchRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
            IRepository<AvailableLeaseHaulerTruck> availableLeaseHaulerTruckRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Drivers.EmployeeTime> employeeTimeRepository,
            IRepository<Customer> customerRepository,
            IRepository<UnitOfMeasure> uomRepository,
            IRepository<Item> itemRepository,
            IRepository<Location> locationRepository,
            IRepository<LocationCategory> locationCategoryRepository,
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IRepository<TimeClassification> timeClassificationRepository,
            ISecureFileBlobService secureFileBlobService,
            UserManager userManager
        )
        {
            _importedEarningsRepository = importedEarningsRepository;
            _importedEarningsBatchRepository = importedEarningsBatchRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
            _availableLeaseHaulerTruckRepository = availableLeaseHaulerTruckRepository;
            _ticketRepository = ticketRepository;
            _employeeTimeRepository = employeeTimeRepository;
            _customerRepository = customerRepository;
            _uomRepository = uomRepository;
            _itemRepository = itemRepository;
            _locationRepository = locationRepository;
            _locationCategoryRepository = locationCategoryRepository;
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _timeClassificationRepository = timeClassificationRepository;
            _secureFileBlobService = secureFileBlobService;
            _userManager = userManager;
        }

        public async Task<bool> LogImportWarning(LogImportWarningInput input)
        {
            _tenantId = await Session.GetTenantIdAsync();
            _userId = Session.UserId ?? 0;
            LogWarning(input.Text + "; Location: " + input.Location);
            return true;
        }

        protected abstract string GetExpectedCsvHeader();
        protected abstract TicketImportType TicketImportType { get; }
        protected string ImportDisplayName => TicketImportType.GetDisplayName();
        protected virtual string ImportFileDisplayName => ImportDisplayName;
        protected abstract Task<bool> GetProductionPayValue();
        protected abstract Task<int> GetCustomerId();
        protected abstract Task<string> GetHaulerRef();
        protected abstract TicketImportTruckMatching TruckMatching { get; }
        protected abstract bool AreRequiredFieldsFilled(TicketEarningsImportRow row);


        public async Task<ValidateTicketEarningsFileResult> ValidateFile(string filePath)
        {
            _filePath = filePath;
            _tenantId = await Session.GetTenantIdAsync();
            _userId = Session.UserId ?? 0;
            try
            {
                var result = new ValidateTicketEarningsFileResult
                {
                    IsValid = true,
                };
                await using (var fileStream = await _secureFileBlobService.GetStreamFromAzureBlobAsync(filePath))
                using (TextReader textReader = new StreamReader(fileStream))
                using (var reader = new ImportReader(textReader, null))
                {
                    var header = string.Join(",", reader.GetCsvHeaders()).TrimEnd(',');
                    if (header != GetExpectedCsvHeader())
                    {
                        LogError("Received header doesn't match the expected header. Received: " + header);
                        throw new UserFriendlyException(L("ThisDoesntLookLike{0}File_PleaseVerifyAndUploadAgain", ImportFileDisplayName));
                    }
                }

                await using (var fileStream = await _secureFileBlobService.GetStreamFromAzureBlobAsync(filePath))
                using (TextReader textReader = new StreamReader(fileStream))
                using (var reader = new ImportReader(textReader, null))
                {
                    var newTickets = new List<(string Site, string TicketNumber)>();

                    int rowNumber = 0;
                    foreach (var row in reader.AsEnumerable<TicketEarningsImportRow>())
                    {
                        rowNumber++;
                        if (!IsRowEmpty(row) && !string.IsNullOrEmpty(row.TicketNumber))
                        {
                            newTickets.Add((row.Site, row.TicketNumber));
                        }
                    }
                    result.TotalRecordCount = newTickets.Count;

                    result.DuplicateTickets = await GetDuplicateImportTicketsAsync(newTickets);
                    if (result.DuplicateTickets.Any())
                    {
                        LogWarning("Records with same site/ticket number already exist: " + string.Join(", ", result.DuplicateTickets.Select(x => $"{x.Site}/{x.TicketNumber}")));
                    }

                    return result;
                }
            }
            catch (UserFriendlyException)
            {
                throw;
            }
            catch (Exception e)
            {
                LogError($"Error in the ImportTicketEarningsBaseAppService.ValidateFile method: {e}");
                throw new UserFriendlyException("Unknown validation error occurred");
            }
        }

        private async Task<List<ValidateTicketEarningsFileResult.DuplicateTicket>> GetDuplicateImportTicketsAsync(List<(string Site, string TicketNumber)> newTickets)
        {
            var result = new List<ValidateTicketEarningsFileResult.DuplicateTicket>();

            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                foreach (var siteGroup in newTickets.GroupBy(x => x.Site))
                {
                    var site = siteGroup.Key;
                    var ticketImportType = TicketImportType;
                    foreach (var siteGroupChunk in siteGroup.Chunk(900))
                    {
                        var ticketNumbers = siteGroupChunk.Select(x => x.TicketNumber).ToList();
                        var existingTickets = await (await _importedEarningsRepository.GetQueryAsync())
                            .Where(x => x.Batch.TicketImportType == ticketImportType
                                && x.Site == site
                                && ticketNumbers.Contains(x.TicketNumber))
                            .Select(x => new ValidateTicketEarningsFileResult.DuplicateTicket
                            {
                                Id = x.Id,
                                Site = x.Site,
                                TicketNumber = x.TicketNumber,
                            })
                            .ToListAsync();
                        result.AddRange(existingTickets);
                    }
                }
            }

            return result;
        }

        protected override async Task<bool> CacheResourcesBeforeImportAsync(IImportReader reader)
        {
            _filePath = _importJobArgs.File;
            _officeId = await (await _userManager.GetQueryAsync()).Where(x => x.Id == _userId).Select(x => x.OfficeId).FirstOrDefaultAsync();
            if (_officeId == null)
            {
                _result.NotFoundOffices.Add(_userId.ToString());
                return false;
            }

            _useShifts = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.General.UseShifts, _tenantId);
            _shift = _useShifts ? Shift.Shift1 : (Shift?)null;

            _useForProductionPay = await GetProductionPayValue() && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature);
            if (_useForProductionPay)
            {
                _timeClassificationId = await (await _timeClassificationRepository.GetQueryAsync()).Where(x => x.IsProductionBased).Select(x => x.Id).FirstOrDefaultAsync();
                if (_timeClassificationId == 0)
                {
                    LogError("ProductionBased time classification wasn't found");
                    _result.ResourceErrors.Add("ProductionBased time classification wasn't found");
                    return false;
                }
            }
            else
            {
                _timeClassificationId = await (await _timeClassificationRepository.GetQueryAsync()).Where(x => x.Name == "Drive Truck").Select(x => x.Id).FirstOrDefaultAsync();
                if (_timeClassificationId == 0)
                {
                    LogError("'Drive Truck' time classification wasn't found");
                    _result.ResourceErrors.Add("Time classification named 'Drive Truck' wasn't found");
                    return false;
                }
            }


            _customerId = await GetCustomerId();
            if (!await (await _customerRepository.GetQueryAsync()).AnyAsync(x => x.Id == _customerId))
            {
                _result.ResourceErrors.Add($"{ImportDisplayName} Customer wasn't found, please select a {ImportDisplayName} Customer in the settings");
                return false;
            }

            _haulerRef = await GetHaulerRef();

            _uoms = await (await _uomRepository.GetQueryAsync())
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                }).ToDictionaryAsync(x => x.Id, x => x.Name);

            _temporaryLocationCategoryId = await (await _locationCategoryRepository.GetQueryAsync())
                .Where(x => x.PredefinedLocationCategoryKind == PredefinedLocationCategoryKind.Temporary)
                .Select(x => x.Id)
                .FirstOrDefaultAsync();

            if (_temporaryLocationCategoryId == 0)
            {
                var temporaryLocationCategory = new LocationCategory
                {
                    Name = "Temporary",
                    PredefinedLocationCategoryKind = PredefinedLocationCategoryKind.Temporary,
                };
                await _locationCategoryRepository.InsertAsync(temporaryLocationCategory);
                await CurrentUnitOfWork.SaveChangesAsync();
                _temporaryLocationCategoryId = temporaryLocationCategory.Id;
            }

            _importedEarningsBatch = new ImportedEarningsBatch
            {
                FilePath = _importJobArgs.File,
                TicketImportType = TicketImportType,
                TenantId = _tenantId,
            };
            await _importedEarningsBatchRepository.InsertAsync(_importedEarningsBatch);
            await CurrentUnitOfWork.SaveChangesAsync();

            _importedEarnings = new List<ImportedEarnings>();

            return await base.CacheResourcesBeforeImportAsync(reader);
        }

        protected override async Task ImportRowAndSaveAsync(TicketEarningsImportRow row, int rowNumber)
        {
            if (await ImportRowAsync(row))
            {
                //_result.ImportedNumber++;
                //CurrentUnitOfWork.SaveChanges();
            }

            if (!string.IsNullOrEmpty(_haulerRef)
                && _haulerRef != row.HaulerRef)
            {
                AddResourceError($"On importing tickets we found a HaulerRef {row.HaulerRef} on line {rowNumber} that doesn’t agree with your system settings. We added the entry, but you should review the record and your “{ImportDisplayName}” settings to verify these entries are correct.");
            }

            WriteRowErrors(row, rowNumber);
        }

        protected override Task<bool> ImportRowAsync(TicketEarningsImportRow row)
        {
            if (!AreRequiredFieldsFilled(row))
            {
                LogWarning("The row was skipped because one of the required values were empty: " + Infrastructure.Utilities.Utility.Serialize(row));
                return Task.FromResult(false);
            }

            var uomId = GetUomId(row.HaulPaymentRateUom);
            if (uomId == null)
            {
                LogWarning("Unexpected UOM: " + row.HaulPaymentRateUom);
                row.AddParseErrorIfNotExist(nameof(row.HaulPaymentRateUom), row.HaulPaymentRateUom, typeof(string));
                //don't return false to stop, continue with the import instead and leave the field empty
                //return false;
            }

            var importedEarnings = new ImportedEarnings
            {
                TicketNumber = row.TicketNumber,
                TenantId = _tenantId,
                CreatorUserId = _userId,
                BatchId = _importedEarningsBatch.Id,
                TicketDateTime = row.TicketDateTime.Value.ConvertTimeZoneFrom(_timeZone),
                Site = row.Site,
                HaulerRef = row.HaulerRef,
                CustomerName = row.CustomerName,
                LicensePlate = row.LicensePlate,
                HaulPaymentRate = row.HaulPaymentRate.Value,
                NetTons = row.NetTons.Value,
                HaulPayment = row.HaulPayment.Value,
                HaulPaymentRateUom = row.HaulPaymentRateUom,
                FscAmount = row.FscAmount ?? 0,
                ProductDescription = row.ProductDescription,
            };
            _importedEarnings.Add(importedEarnings);
            return Task.FromResult(true);
        }

        protected override bool IsRowEmpty(TicketEarningsImportRow row)
        {
            return string.IsNullOrEmpty(row.TicketNumber);
        }

        protected override async Task<bool> PostImportTasksAsync()
        {
            if (!_importedEarnings.Any())
            {
                return true;
            }

            var duplicateTickets = await GetDuplicateImportTicketsAsync(_importedEarnings.Select(x => (x.Site, x.TicketNumber)).ToList());
            _result.SkippedNumber += duplicateTickets.Count;

            var licensePlates = _importedEarnings.Select(x => x.LicensePlate).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
            var trucks = await (await _truckRepository.GetQueryAsync())
                .WhereIf(TruckMatching == TicketImportTruckMatching.ByLicensePlate, x => licensePlates.Contains(x.Plate))
                .WhereIf(TruckMatching == TicketImportTruckMatching.ByTruckCode, x => licensePlates.Contains(x.TruckCode))
                .Select(x => new
                {
                    x.Id,
                    LicensePlate = x.Plate,
                    x.TruckCode,
                    x.OfficeId,
                    LeaseHaulerId = (int?)x.LeaseHaulerTruck.LeaseHaulerId,
                    AlwaysShowOnSchedule = (bool?)x.LeaseHaulerTruck.AlwaysShowOnSchedule,
                })
                .ToListAsync();

            var dates = _importedEarnings
                .Select(x => x.TicketDateTime.ConvertTimeZoneTo(_timeZone).Date)
                .Distinct()
                .ToList();

            var truckIds = trucks.Select(x => x.Id).Distinct().ToList();

            await PopulateDriversForTrucksAsync(
                truckIds,
                dates
            );

            var driverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(x => x.Shift == _shift
                    && dates.Contains(x.Date)
                    && truckIds.Contains(x.TruckId))
                .Select(x => new
                {
                    x.Date,
                    x.TruckId,
                    x.DriverId,
                })
                .ToListAsync();

            var leaseHaulerRequests = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                .Where(x => x.LeaseHaulerRequest.Shift == _shift
                    && dates.Contains(x.LeaseHaulerRequest.Date)
                    && truckIds.Contains(x.TruckId))
                .Select(x => new
                {
                    x.LeaseHaulerRequest.Date,
                    x.TruckId,
                    DriverId = (int?)x.DriverId,
                })
                .ToListAsync();

            await PopulateLoadAtLocationsFromSites(_importedEarnings.Select(x => x.Site).Distinct(StringComparer.OrdinalIgnoreCase).ToList());
            await PopulateDeliverToLocationsByNamesAsync(_importedEarnings.Select(x => x.CustomerName).Distinct(StringComparer.OrdinalIgnoreCase).ToList());
            await PopulateItemsAsync(_importedEarnings.Select(x => x.ProductDescription).Distinct(StringComparer.OrdinalIgnoreCase).ToList());

            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var requiredTicketEntry = await SettingManager.GetRequiredTicketEntry();

            foreach (var rowGroup in _importedEarnings
                .Where(x => !duplicateTickets.Any(t => t.Site == x.Site && t.TicketNumber == x.TicketNumber))
                .GroupBy(x => new { x.TicketDateTime, x.CustomerName, x.Site, x.ProductDescription, x.HaulPaymentRateUom }))
            {
                await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    var date = rowGroup.Key.TicketDateTime.ConvertTimeZoneTo(_timeZone).Date;

                    var order = new Order
                    {
                        DeliveryDate = date,
                        CustomerId = _customerId,
                        CreatorUserId = _userId,
                        OfficeId = _officeId.Value,
                        TenantId = _tenantId,
                        IsClosed = true,
                        IsImported = true,
                        Shift = _shift,
                    };
                    int nextOrderLineNumber;
                    var existingOrder = await (await _orderRepository.GetQueryAsync())
                        .Include(x => x.OrderLines)
                        .FirstOrDefaultAsync(x => x.DeliveryDate == order.DeliveryDate
                            && x.CustomerId == order.CustomerId
                            && x.OfficeId == order.OfficeId
                            && x.IsClosed
                            && x.IsImported);
                    if (existingOrder == null)
                    {
                        nextOrderLineNumber = 1;
                        await _orderRepository.InsertAsync(order);
                    }
                    else
                    {
                        nextOrderLineNumber = existingOrder.OrderLines.Count() + 1;
                        order = existingOrder;
                    }

                    var orderLineTotal = rowGroup.Sum(x => x.HaulPayment);
                    var orderLine = new OrderLine
                    {
                        Order = order,
                        TenantId = _tenantId,
                        CreatorUserId = _userId,
                        LineNumber = nextOrderLineNumber++,
                        MaterialQuantity = null,
                        FreightQuantity = rowGroup.Sum(x => x.NetTons),
                        FreightItemId = _items[rowGroup.Key.ProductDescription.ToLower()],
                        MaterialItemId = null,
                        MaterialPricePerUnit = null,
                        FreightPricePerUnit = rowGroup.Average(x => x.HaulPaymentRate),
                        FreightRateToPayDrivers = rowGroup.Average(x => x.HaulPaymentRate),
                        MaterialPrice = 0,
                        FreightPrice = rowGroup.Sum(x => x.HaulPayment),
                        MaterialUomId = GetUomId(rowGroup.Key.HaulPaymentRateUom),
                        FreightUomId = GetUomId(rowGroup.Key.HaulPaymentRateUom),
                        Designation = DesignationEnum.FreightOnly,
                        LoadAtId = _loadAtLocations[rowGroup.Key.Site.ToLower()],
                        DeliverToId = _deliverToLocations[rowGroup.Key.CustomerName.ToLower()],
                        FuelSurchargeRate = rowGroup.Sum(x => x.FscAmount) / (orderLineTotal == 0 ? null : orderLineTotal),
                        IsComplete = true,
                        NumberOfTrucks = 1,
                        ProductionPay = _useForProductionPay,
                        RequireTicket = requiredTicketEntry.GetRequireTicketDefaultValue(),
                    };
                    await _orderLineRepository.InsertAsync(orderLine);

                    var addedOrderLineTrucks = new List<OrderLineTruck>();

                    foreach (var row in rowGroup)
                    {
                        await _importedEarningsRepository.InsertAsync(row);
                        _result.ImportedNumber++;

                        var matchingTrucks = trucks
                            .Where(x =>
                                TruckMatching == TicketImportTruckMatching.ByLicensePlate && x.LicensePlate.ToLower() == row.LicensePlate.ToLower()
                                || TruckMatching == TicketImportTruckMatching.ByTruckCode && x.TruckCode.ToLower() == row.LicensePlate.ToLower()
                            )
                            .ToList();
                        if (!matchingTrucks.Any())
                        {
                            AddResourceError($"Truck with {TruckMatching.GetDisplayName()} {row.LicensePlate} wasn’t found. You’ll need to fix the truck on the ticket view");
                        }
                        else if (matchingTrucks.Count > 1)
                        {
                            //first, remove lease hauler trucks that don't have a request for that day
                            matchingTrucks.RemoveAll(x => x.LeaseHaulerId != null
                                && x.AlwaysShowOnSchedule == false
                                && !leaseHaulerRequests.Any(r => r.Date == date
                                    && r.TruckId == x.Id
                                    && r.DriverId != null));

                            //if there are still multiple trucks, remove trucks that are not scheduled for that day
                            if (matchingTrucks.Count > 1)
                            {
                                matchingTrucks.RemoveAll(x => x.AlwaysShowOnSchedule != false //true or null
                                    && !driverAssignments.Any(r => r.Date == date
                                        && r.TruckId == x.Id
                                        && r.DriverId != null));
                            }

                            if (matchingTrucks.Count != 1)
                            {
                                AddResourceError($"Multiple trucks with {TruckMatching.GetDisplayName()} {row.LicensePlate} were found. You’ll need to fix the truck on the ticket view");
                                matchingTrucks.Clear();
                            }
                        }
                        var truck = matchingTrucks.FirstOrDefault();
                        var driverId = truck == null ? null : _driversForTrucks[(truck.Id, date)];
                        //var driver = drivers.FirstOrDefault(x => x.Id == driverId);

                        //if (driver == null)
                        //{
                        //    AddResourceError($"Driver {row.DriverName} wasn’t found. You’ll need to fix the driver on the ticket view");
                        //}
                        //else if (driver.UserId == null)
                        //{
                        //    AddResourceError($"Driver {row.DriverName} doesn't have a user linked. Employee Time records won't be created");
                        //}

                        var ticket = new Ticket
                        {
                            OrderLine = orderLine,
                            TenantId = _tenantId,
                            CreatorUserId = _userId,
                            TicketNumber = row.TicketNumber,
                            //FreightQuantity = row.NetTons,
                            //MaterialQuantity = 0,
                            FuelSurcharge = row.FscAmount,
                            TruckId = truck?.Id,
                            TruckCode = truck?.TruckCode,
                            CustomerId = order.CustomerId,
                            TicketDateTime = row.TicketDateTime,
                            //FreightItemId = orderLine.FreightItemId,
                            //MaterialItemId = orderLine.MaterialItemId,
                            //FreightUomId = orderLine.FreightUomId,
                            //MaterialUomId = orderLine.MaterialUomId,
                            OfficeId = truck?.OfficeId ?? _officeId,
                            CarrierId = truck?.LeaseHaulerId,
                            DriverId = driverId,
                            DeliverToId = orderLine.DeliverToId,
                            LoadAtId = orderLine.LoadAtId,
                            IsImported = true,
                            IsBilled = true,
                            NonbillableFreight = !orderLine.Designation.HasFreight(),
                            NonbillableMaterial = !orderLine.Designation.HasMaterial(),
                        };

                        TicketQuantityHelper.SetTicketQuantity(ticket, new TicketEditQuantityDto
                        {
                            FreightQuantity = row.NetTons,
                            MaterialQuantity = row.NetTons,
                            FreightItemId = orderLine.FreightItemId,
                            MaterialItemId = orderLine.MaterialItemId,
                            FreightUomId = orderLine.FreightUomId,
                        },
                        new TicketOrderLineDetailsDto
                        {
                            Designation = orderLine.Designation,
                            FreightItemId = orderLine.FreightItemId,
                            MaterialItemId = orderLine.MaterialItemId,
                            FreightQuantity = orderLine.FreightQuantity,
                            MaterialQuantity = orderLine.MaterialQuantity,
                            FreightUomId = orderLine.FreightUomId,
                            FreightUomBaseId = null, //we'll need to populate these if minimums are needed for the import
                            MaterialUomId = orderLine.MaterialUomId,
                        }, separateItems);

                        await _ticketRepository.InsertAsync(ticket);

                        //if (driver?.UserId != null && _useForProductionPay)
                        //{
                        //    var employeeTime = new Drivers.EmployeeTime
                        //    {
                        //        TenantId = _tenantId,
                        //        UserId = driver.UserId.Value,
                        //        StartDateTime = row.TicketDateTime,
                        //        TimeClassificationId = _timeClassificationId,
                        //        EquipmentId = truck?.Id,
                        //        DriverId = driver.Id,
                        //        IsImported = true
                        //    };
                        //    _employeeTimeRepository.Insert(employeeTime);
                        //}

                        if (truck != null && !addedOrderLineTrucks.Any(x => x.TruckId == truck.Id))
                        {
                            var orderLineTruck = new OrderLineTruck
                            {
                                IsDone = true,
                                OrderLine = orderLine,
                                TenantId = _tenantId,
                                TruckId = truck.Id,
                                DriverId = driverId,
                            };
                            await _orderLineTruckRepository.InsertAsync(orderLineTruck);
                            addedOrderLineTrucks.Add(orderLineTruck);
                        }
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                });
            }

            return true;
        }

        private int? GetUomId(string rowUom)
        {
            if (rowUom?.ToLower() == "ld")
            {
                rowUom = "Load";
            }

            foreach (var uom in _uoms)
            {
                if (uom.Value.Equals(rowUom, StringComparison.InvariantCultureIgnoreCase))
                {
                    return uom.Key;
                }
            }

            foreach (var uom in _uoms)
            {
                if (uom.Value.ToLower().TrimEnd('s').Equals(rowUom.ToLower().TrimEnd('s'), StringComparison.InvariantCultureIgnoreCase))
                {
                    return uom.Key;
                }
            }

            //return _uoms.First().Key;
            return null;
        }

        private async Task PopulateDriversForTrucksAsync(List<int> truckIds, List<DateTime> dateList)
        {
            _driversForTrucks ??= new();
            var driverAssignments = await (await _driverAssignmentRepository.GetQueryAsync())
                .Where(x => x.Shift == _shift
                    && dateList.Contains(x.Date)
                    && truckIds.Contains(x.TruckId))
                .Select(x => new
                {
                    x.Date,
                    x.TruckId,
                    x.DriverId,
                })
                .ToListAsync();

            var leaseHaulerRequests = await (await _availableLeaseHaulerTruckRepository.GetQueryAsync())
                .Where(x => x.LeaseHaulerRequest.Shift == _shift
                    && dateList.Contains(x.LeaseHaulerRequest.Date)
                    && truckIds.Contains(x.TruckId))
                .Select(x => new
                {
                    x.LeaseHaulerRequest.Date,
                    x.TruckId,
                    DriverId = (int?)x.DriverId,
                })
                .ToListAsync();

            var defaultDrivers = await (await _truckRepository.GetQueryAsync())
                .Where(x => truckIds.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.TruckCode,
                    x.DefaultDriverId,
                }).ToListAsync();

            foreach (var truckId in truckIds)
            {
                foreach (var date in dateList)
                {
                    int? driverId;
                    var driverAssignmentGroup = driverAssignments
                        .Union(leaseHaulerRequests)
                        .Where(x => x.Date == date && x.TruckId == truckId)
                        .ToList();
                    var truckCode = defaultDrivers.FirstOrDefault(x => x.Id == truckId)?.TruckCode;

                    if (driverAssignmentGroup.Count == 1)
                    {
                        driverId = driverAssignmentGroup[0].DriverId;
                        if (driverId == null)
                        {
                            AddResourceError($"Truck {truckCode} has no driver assigned on {date:d}. You’ll need to fix the driver on the ticket view");
                        }
                    }
                    else if (driverAssignmentGroup.Count == 0)
                    {
                        driverId = defaultDrivers.FirstOrDefault(x => x.Id == truckId)?.DefaultDriverId;
                        if (driverId == null)
                        {
                            AddResourceError($"Truck {truckCode} has no default driver and no driver assigned on {date:d}. You’ll need to fix the driver on the ticket view");
                        }
                    }
                    else
                    {
                        driverId = null;
                        AddResourceError($"Truck {truckCode} has more than one driver assigned on {date:d}. You’ll need to fix the driver on the ticket view");
                    }

                    _driversForTrucks.Add((truckId, date), driverId);
                }
            }
        }

        protected virtual async Task PopulateLoadAtLocationsFromSites(List<string> sites)
        {
            _loadAtLocations = await GetLocationDictionary(sites);
        }

        protected virtual async Task PopulateDeliverToLocationsByNamesAsync(List<string> customerNames)
        {
            _deliverToLocations = await GetLocationDictionary(customerNames);
        }

        protected async Task<Dictionary<string, int>> GetLocationDictionary(List<string> locationNames)
        {
            var result = new Dictionary<string, int>();
            var locations = (await _locationRepository.GetQueryAsync())
                .Where(x => locationNames.Contains(x.Name))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                });

            var newLocations = new List<Location>();
            foreach (var locationName in locationNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var existingLocation = await locations.FirstOrDefaultAsync(x => x.Name.ToLower() == locationName.ToLower());
                if (existingLocation != null)
                {
                    result.Add(locationName.ToLower(), existingLocation.Id);
                }
                else
                {
                    var location = new Location
                    {
                        Name = locationName,
                        IsActive = true,
                        CategoryId = _temporaryLocationCategoryId,
                    };
                    await _locationRepository.InsertAsync(location);
                    newLocations.Add(location);
                }
            }

            if (newLocations.Any())
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                newLocations.ForEach(location => result.Add(location.Name.ToLower(), location.Id));
            }

            return result;
        }

        private async Task PopulateItemsAsync(List<string> itemNames)
        {
            _items ??= new();
            var items = await (await _itemRepository.GetQueryAsync())
                .Where(x => itemNames.Contains(x.Name))
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                })
                .ToListAsync();

            var newItems = new List<Item>();

            foreach (var itemName in itemNames.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var existing = items.FirstOrDefault(x => x.Name.ToLower() == itemName.ToLower());
                if (existing != null)
                {
                    _items.Add(itemName.ToLower(), existing.Id);
                }
                else
                {
                    var item = new Item
                    {
                        Name = itemName,
                        IsActive = true,
                        Type = ItemType.NonInventoryPart,
                    };
                    await _itemRepository.InsertAsync(item);
                    newItems.Add(item);
                    AddResourceError($"An item {item.Name} wasn’t set up in your products and services. To be able to create these entries, we added this item. Please review to be sure it is set up correctly.");
                }
            }

            if (newItems.Any())
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                newItems.ForEach(item => _items.Add(item.Name.ToLower(), item.Id));
            }
        }

        private void LogWarning(string text)
        {
            Logger.Warn($"{ImportDisplayName} Earnings Import warning (tenantId: {_tenantId}, userId: {_userId}, file:{_filePath}): " + text);
        }

        private void LogError(string text)
        {
            Logger.Error($"{ImportDisplayName} Earnings Import error (tenantId: {_tenantId}, userId: {_userId}, file:{_filePath}): " + text);
        }
    }
}
