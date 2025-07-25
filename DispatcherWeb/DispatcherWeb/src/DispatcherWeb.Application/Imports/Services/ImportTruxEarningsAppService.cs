using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
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
using DispatcherWeb.Locations;
using DispatcherWeb.Orders;
using DispatcherWeb.Tickets;
using DispatcherWeb.Tickets.Dto;
using DispatcherWeb.TimeClassifications;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trux;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Imports.Services
{
    [AbpAuthorize(AppPermissions.Pages_Imports_Tickets_TruxEarnings)]
    public class ImportTruxEarningsAppService : ImportDataBaseAppService<TruxImportRow>, IImportTruxEarningsAppService
    {
        private readonly IRepository<TruxEarnings> _truxEarningsRepository;
        private readonly IRepository<TruxEarningsBatch> _truxEarningsBatchRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<OrderLineTruck> _orderLineTruckRepository;
        private readonly IRepository<DriverAssignment> _driverAssignmentRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Drivers.EmployeeTime> _employeeTimeRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<UnitOfMeasure> _uomRepository;
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<LocationCategory> _locationCategoryRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<TimeClassification> _timeClassificationRepository;
        private readonly ISecureFileBlobService _secureFileBlobService;
        private readonly UserManager _userManager;
        private string _filePath = null;
        private int? _officeId = null;
        private bool _useShifts;
        private bool _useForProductionPay;
        private int _truxCustomerId;
        private Dictionary<int, string> _uoms;
        private int _itemId;
        private int _temporaryLocationCategoryId;
        private int _timeClassificationId;
        private TruxEarningsBatch _truxEarningsBatch = null;
        private List<TruxEarnings> _truxEarnings = null;

        public ImportTruxEarningsAppService(
            IRepository<TruxEarnings> truxEarningsRepository,
            IRepository<TruxEarningsBatch> truxEarningsBatchRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<OrderLineTruck> orderLineTruckRepository,
            IRepository<DriverAssignment> driverAssignmentRepository,
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
            _truxEarningsRepository = truxEarningsRepository;
            _truxEarningsBatchRepository = truxEarningsBatchRepository;
            _orderRepository = orderRepository;
            _orderLineRepository = orderLineRepository;
            _orderLineTruckRepository = orderLineTruckRepository;
            _driverAssignmentRepository = driverAssignmentRepository;
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

        public async Task<ValidateTruxFileResult> ValidateFileAsync(string filePath)
        {
            _filePath = filePath;
            _tenantId = await Session.GetTenantIdAsync();
            _userId = Session.UserId ?? 0;
            try
            {
                var result = new ValidateTruxFileResult
                {
                    IsValid = true,
                };
                await using (var fileStream = await _secureFileBlobService.GetStreamFromAzureBlobAsync(filePath))
                using (TextReader textReader = new StreamReader(fileStream))
                using (var reader = new ImportReader(textReader, null))
                {

                    var header = string.Join(",", reader.GetCsvHeaders());
                    if (header != "Job Id,Shift/Assignment,Job Name,Start Date,Truck Type,Status,Truck Id,Driver Name,Hauler Name,Punch In Datetime,Punch Out Datetime,Hours,Tons,Loads,Unit,Rate,Total")
                    {
                        LogError("Received header doesn't match the expected header. Received: " + header);
                        throw new UserFriendlyException(L("ThisDoesntLookLike{0}File_PleaseVerifyAndUploadAgain", "Trux Earnings"));
                    }
                }

                await using (var fileStream = await _secureFileBlobService.GetStreamFromAzureBlobAsync(filePath))
                using (TextReader textReader = new StreamReader(fileStream))
                using (var reader = new ImportReader(textReader, null))
                {
                    var newTruxEarningsIds = new List<int>();

                    int rowNumber = 0;
                    foreach (var row in reader.AsEnumerable<TruxImportRow>())
                    {
                        rowNumber++;
                        if (!IsRowEmpty(row) && row.ShiftAssignment.HasValue)
                        {
                            if (newTruxEarningsIds.Contains(row.ShiftAssignment.Value))
                            {
                                result.DuplicateShiftAssignmentsInFile.Add(row.ShiftAssignment.Value);
                            }
                            newTruxEarningsIds.Add(row.ShiftAssignment.Value);
                        }
                    }
                    result.TotalRecordCount = newTruxEarningsIds.Count;

                    result.DuplicateShiftAssignments = await GetDuplicateShiftAssignmentsAsync(newTruxEarningsIds);
                    if (result.DuplicateShiftAssignments.Any())
                    {
                        LogWarning("Records with same shift/assignment already exist: " + string.Join(", ", result.DuplicateShiftAssignments));
                    }

                    if (result.DuplicateShiftAssignmentsInFile.Any())
                    {
                        LogWarning("Records with same shift/assignment not allowed: " + string.Join(", ", result.DuplicateShiftAssignmentsInFile));
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
                LogError($"Error in the ImportTruxEarningsAppService.ValidateFile method: {e}");
                throw new UserFriendlyException("Unknown validation error occurred");
            }
        }

        private async Task<List<int>> GetDuplicateShiftAssignmentsAsync(List<int> shiftAssignments)
        {
            var result = new List<int>();

            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MustHaveTenant))
            using (UnitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                foreach (var idChunk in shiftAssignments.Chunk(900))
                {
                    var ids = idChunk.ToList();
                    var existingIds = await (await _truxEarningsRepository.GetQueryAsync())
                        .Where(x => ids.Contains(x.Id))
                        .Select(x => x.Id)
                        .ToListAsync();
                    result.AddRange(existingIds);
                }
            }

            return result;
        }

        protected override async Task<bool> CacheResourcesBeforeImportAsync(IImportReader reader)
        {
            _filePath = _importJobArgs.File;
            _officeId = (await _userManager.GetQueryAsync()).Where(x => x.Id == _userId).Select(x => x.OfficeId).FirstOrDefault();
            if (_officeId == null)
            {
                _result.NotFoundOffices.Add(_userId.ToString());
                return false;
            }

            _useShifts = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.General.UseShifts, _tenantId);

            _useForProductionPay = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.Trux.UseForProductionPay, _tenantId) && await FeatureChecker.IsEnabledAsync(AppFeatures.DriverProductionPayFeature);
            if (_useForProductionPay)
            {
                _timeClassificationId = (await _timeClassificationRepository.GetQueryAsync()).Where(x => x.IsProductionBased).Select(x => x.Id).FirstOrDefault();
                if (_timeClassificationId == 0)
                {
                    LogError("ProductionBased time classification wasn't found");
                    _result.ResourceErrors.Add("ProductionBased time classification wasn't found");
                    return false;
                }
            }
            else
            {
                _timeClassificationId = (await _timeClassificationRepository.GetQueryAsync()).Where(x => x.Name == "Drive Truck").Select(x => x.Id).FirstOrDefault();
                if (_timeClassificationId == 0)
                {
                    LogError("'Drive Truck' time classification wasn't found");
                    _result.ResourceErrors.Add("Time classification named 'Drive Truck' wasn't found");
                    return false;
                }
            }


            _truxCustomerId = await SettingManager.GetSettingValueForTenantAsync<int>(AppSettings.Trux.TruxCustomerId, _tenantId);
            if (!(await _customerRepository.GetQueryAsync()).Any(x => x.Id == _truxCustomerId))
            {
                _result.ResourceErrors.Add("Trux Customer wasn't found, please select a Trux Customer in the settings");
                return false;
            }

            _uoms = (await _uomRepository.GetQueryAsync())
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                }).ToDictionary(x => x.Id, x => x.Name);

            _itemId = (await _itemRepository.GetQueryAsync())
                .Where(x => x.Name == "Trux Unknown")
                .Select(x => x.Id)
                .FirstOrDefault();

            if (_itemId == 0)
            {
                var item = new Item
                {
                    Name = "Trux Unknown",
                    IsActive = true,
                };
                await _itemRepository.InsertAsync(item);
                await CurrentUnitOfWork.SaveChangesAsync();
                _itemId = item.Id;
            }

            _temporaryLocationCategoryId = (await _locationCategoryRepository.GetQueryAsync())
                .Where(x => x.PredefinedLocationCategoryKind == PredefinedLocationCategoryKind.Temporary)
                .Select(x => x.Id)
                .FirstOrDefault();

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

            _truxEarningsBatch = new TruxEarningsBatch
            {
                FilePath = _importJobArgs.File,
                TenantId = _tenantId,
            };
            await _truxEarningsBatchRepository.InsertAsync(_truxEarningsBatch);
            await CurrentUnitOfWork.SaveChangesAsync();

            _truxEarnings = new List<TruxEarnings>();

            return await base.CacheResourcesBeforeImportAsync(reader);
        }

        protected override async Task ImportRowAndSaveAsync(TruxImportRow row, int rowNumber)
        {
            if (await ImportRowAsync(row))
            {
                //_result.ImportedNumber++;
                //CurrentUnitOfWork.SaveChanges();
            }

            WriteRowErrors(row, rowNumber);
        }

        protected override Task<bool> ImportRowAsync(TruxImportRow row)
        {
            if (row.Hours == null || row.JobId == null || row.ShiftAssignment == null || row.Loads == null || row.PunchInDatetime == null
                || row.PunchOutDatetime == null || row.Rate == null || row.StartDateTime == null || row.Tons == null || row.Total == null
                || row.Unit == null || string.IsNullOrEmpty(row.JobName))
            {
                LogWarning("The row was skipped because one of the required values were empty: " + Infrastructure.Utilities.Utility.Serialize(row));
                return Task.FromResult(false);
            }

            if (!row.Unit.ToLower().TrimEnd('s').IsIn("ton", "load", "hour"))
            {
                LogWarning("Unexpected Unit: " + row.Unit);
                row.AddParseErrorIfNotExist("Unit", row.Unit, typeof(string));
                return Task.FromResult(false);
            }

            if (!row.JobName.Contains(" to "))
            {
                LogWarning("Unexpected JobName, ' to ' is missing: " + row.JobName);
                row.AddParseErrorIfNotExist("JobName", row.JobName, typeof(string));
                return Task.FromResult(false);
            }

            var truxEarnings = new TruxEarnings
            {
                Id = row.ShiftAssignment.Value,
                TenantId = _tenantId,
                CreatorUserId = _userId,
                BatchId = _truxEarningsBatch.Id,
                DriverName = row.DriverName,
                HaulerName = row.HaulerName,
                Hours = row.Hours.Value,
                JobId = row.JobId.Value,
                JobName = row.JobName,
                Loads = row.Loads.Value,
                PunchInDatetime = row.PunchInDatetime.Value,
                PunchOutDatetime = row.PunchOutDatetime.Value,
                Rate = row.Rate.Value,
                StartDateTime = row.StartDateTime.Value,
                Status = row.Status,
                Tons = row.Tons.Value,
                Total = row.Total.Value,
                TruckType = row.TruckType,
                TruxTruckId = row.TruxTruckId,
                Unit = row.Unit,
            };
            _truxEarnings.Add(truxEarnings);

            return Task.FromResult(true);
        }

        protected override bool IsRowEmpty(TruxImportRow row)
        {
            return !(row.JobId > 0) && !(row.ShiftAssignment > 0);
        }

        protected override async Task<bool> PostImportTasksAsync()
        {
            if (!_truxEarnings.Any())
            {
                return true;
            }

            var separateItems = await FeatureChecker.IsEnabledAsync(AppFeatures.SeparateMaterialAndFreightItems);
            var requiredTicketEntry = await SettingManager.GetRequiredTicketEntry();

            var duplicateShiftAssignments = await GetDuplicateShiftAssignmentsAsync(_truxEarnings.Select(x => x.Id).ToList());
            _result.SkippedNumber += duplicateShiftAssignments.Count;

            var addedDriverAssignments = new List<DriverAssignment>();
            var shiftAssignmentIdsInFile = new List<int>();

            foreach (var truxGroup in _truxEarnings.Where(x => !duplicateShiftAssignments.Contains(x.Id)).GroupBy(x => x.JobId))
            {
                if (truxGroup.All(x => shiftAssignmentIdsInFile.Contains(x.Id)))
                {
                    AddResourceError($"Duplicate Shift/Assignment Id was found in the file.");
                    _result.SkippedNumber += truxGroup.Count();
                    continue;
                }
                await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    var jobId = truxGroup.Key;
                    var date = truxGroup.First().StartDateTime.ConvertTimeZoneTo(_timeZone).Date;

                    var order = new Order
                    {
                        DeliveryDate = date,
                        CustomerId = _truxCustomerId,
                        CreatorUserId = _userId,
                        OfficeId = _officeId.Value,
                        TenantId = _tenantId,
                        IsClosed = true,
                        IsImported = true,
                        Shift = _useShifts ? Shift.Shift1 : (Shift?)null,
                    };
                    int nextOrderLineNumber;
                    var existingOrder = (await _orderRepository.GetQueryAsync())
                        .Include(x => x.OrderLines)
                        .Where(x => x.DeliveryDate == order.DeliveryDate
                            && x.CustomerId == order.CustomerId
                            && x.OfficeId == order.OfficeId
                            && x.IsClosed
                            && x.IsImported).FirstOrDefault();
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


                    foreach (var row in truxGroup)
                    {
                        if (shiftAssignmentIdsInFile.Contains(row.Id))
                        {
                            AddResourceError($"Duplicate Shift/Assignment Id was found in the file.");
                            _result.SkippedNumber++;
                            continue;
                        }
                        await _truxEarningsRepository.InsertAsync(row);
                        shiftAssignmentIdsInFile.Add(row.Id);
                        _result.ImportedNumber++;

                        var orderLine = new OrderLine
                        {
                            Order = order,
                            TenantId = _tenantId,
                            CreatorUserId = _userId,
                            LineNumber = nextOrderLineNumber++,
                            MaterialQuantity = null,
                            FreightQuantity = GetQuantity(row),
                            FreightItemId = _itemId,
                            MaterialItemId = null,
                            MaterialPricePerUnit = null,
                            FreightPricePerUnit = row.Rate,
                            FreightRateToPayDrivers = row.Rate,
                            MaterialPrice = 0,
                            FreightPrice = row.Total,
                            MaterialUomId = GetUomId(row),
                            FreightUomId = GetUomId(row),
                            Designation = DesignationEnum.FreightOnly,
                            LoadAtId = await GetLoadAtIdAsync(row),
                            DeliverToId = await GetDeliverToIdAsync(row),
                            JobNumber = jobId.ToString(),
                            IsComplete = true,
                            NumberOfTrucks = 1,
                            ProductionPay = _useForProductionPay,
                            RequireTicket = requiredTicketEntry.GetRequireTicketDefaultValue(),
                        };
                        await _orderLineRepository.InsertAsync(orderLine);

                        var truck = await (await _truckRepository.GetQueryAsync())
                            .Where(x => x.TruxTruckId == row.TruxTruckId)
                            .Select(x => new
                            {
                                x.Id,
                                x.TruckCode,
                            }).FirstOrDefaultAsync();

                        var driver = await (await _driverRepository.GetQueryAsync())
                            .Where(x => x.FirstName + " " + x.LastName == row.DriverName)
                            .Select(x => new
                            {
                                x.Id,
                                x.UserId,
                                x.IsInactive,
                            })
                            .OrderByDescending(x => !x.IsInactive)
                            .FirstOrDefaultAsync();

                        if (driver == null)
                        {
                            AddResourceError($"Driver {row.DriverName} wasn’t found. You’ll need to fix the driver on the ticket view");
                        }
                        else if (driver.UserId == null)
                        {
                            AddResourceError($"Driver {row.DriverName} doesn't have a user linked. Employee Time records won't be created");
                        }

                        var ticket = new Ticket
                        {
                            OrderLine = orderLine,
                            TenantId = _tenantId,
                            CreatorUserId = _userId,
                            TicketNumber = row.Id.ToString(),
                            //FreightQuantity = GetQuantity(row),
                            //MaterialQuantity = 0,
                            TruckId = truck?.Id,
                            TruckCode = truck?.TruckCode ?? row.TruxTruckId,
                            CustomerId = order.CustomerId,
                            TicketDateTime = row.StartDateTime,
                            //FreightItemId = orderLine.FreightItemId,
                            //MaterialItemId = orderLine.MaterialItemId,
                            //FreightUomId = orderLine.FreightUomId,
                            //MaterialUomId = null,
                            OfficeId = _officeId,
                            DriverId = driver?.Id,
                            DeliverToId = orderLine.DeliverToId,
                            LoadAtId = orderLine.LoadAtId,
                            IsImported = true,
                            IsBilled = true,
                            NonbillableFreight = !orderLine.Designation.HasFreight(),
                            NonbillableMaterial = !orderLine.Designation.HasMaterial(),
                        };

                        TicketQuantityHelper.SetTicketQuantity(ticket, new TicketEditQuantityDto
                        {
                            FreightQuantity = orderLine.FreightQuantity,
                            MaterialQuantity = orderLine.FreightQuantity,
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

                        if (driver?.UserId != null)
                        {
                            var employeeTime = new Drivers.EmployeeTime
                            {
                                TenantId = _tenantId,
                                UserId = driver.UserId.Value,
                                StartDateTime = row.PunchInDatetime,
                                EndDateTime = row.PunchOutDatetime,
                                TimeClassificationId = _timeClassificationId,
                                EquipmentId = truck?.Id,
                                DriverId = driver.Id,
                                IsImported = true,
                            };
                            await _employeeTimeRepository.InsertAsync(employeeTime);
                        }

                        if (truck != null && driver != null)
                        {
                            var driverAssignment = new DriverAssignment
                            {
                                Date = date,
                                DriverId = driver.Id,
                                OfficeId = _officeId,
                                Shift = order.Shift,
                                StartTime = null,
                                TruckId = truck.Id,
                                TenantId = _tenantId,
                            };

                            var existing = addedDriverAssignments
                                .FirstOrDefault(x => x.Date == driverAssignment.Date
                                                    && x.TruckId == driverAssignment.TruckId
                                                    && x.Shift == driverAssignment.Shift
                                );

                            existing ??= await _driverAssignmentRepository
                                .FirstOrDefaultAsync(x => x.Date == driverAssignment.Date
                                                    && x.TruckId == driverAssignment.TruckId
                                                    && x.Shift == driverAssignment.Shift
                                );

                            if (existing == null)
                            {
                                await _driverAssignmentRepository.InsertAsync(driverAssignment);
                                addedDriverAssignments.Add(driverAssignment);
                            }
                            else
                            {
                                if (existing.DriverId != driverAssignment.DriverId || existing.OfficeId != driverAssignment.OfficeId)
                                {
                                    AddResourceError($"Driver assignment for truck {truck.TruckCode} on {date:d} already exists with a driver or office different than expected (Expected driver: {row.DriverName}, officeId: {_officeId})");
                                }
                            }
                        }

                        if (truck == null)
                        {
                            AddResourceError($"Truck with TruxId {row.TruxTruckId} wasn’t found. You’ll need to add the OrderLineTruck on the schedule view");
                        }
                        else
                        {
                            var orderLineTruck = new OrderLineTruck
                            {
                                IsDone = true,
                                OrderLine = orderLine,
                                TenantId = _tenantId,
                                TruckId = truck.Id,
                                DriverId = driver?.Id,
                            };
                            await _orderLineTruckRepository.InsertAsync(orderLineTruck);
                        }
                    }

                    await CurrentUnitOfWork.SaveChangesAsync();
                });
            }

            return true;
        }

        private int GetUomId(TruxEarnings row)
        {
            foreach (var uom in _uoms)
            {
                if (uom.Value.Equals(row.Unit, StringComparison.InvariantCultureIgnoreCase))
                {
                    return uom.Key;
                }
            }

            foreach (var uom in _uoms)
            {
                if (uom.Value.ToLower().TrimEnd('s').Equals(row.Unit.ToLower().TrimEnd('s'), StringComparison.InvariantCultureIgnoreCase))
                {
                    return uom.Key;
                }
            }

            return _uoms.First().Key;
        }

        private decimal GetQuantity(TruxEarnings row)
        {
            switch (row.Unit.ToLower().TrimEnd('s'))
            {
                case "ton": return row.Tons;
                case "hour": return row.Hours;
                case "load": return row.Loads;
            }
            return row.Tons;
        }

        private async Task<int?> GetLoadAtIdAsync(TruxEarnings row)
        {
            var parts = row.JobName.Split(" to ").SkipLast(1).ToList();
            int? locationId;

            if (parts.Count > 1)
            {
                locationId = await GetLocationIdByNameOrNullAsync(string.Join(" to ", parts));
                if (locationId != null)
                {
                    return locationId;
                }
            }

            locationId = await GetLocationIdByNameOrNullAsync(parts.First());
            if (locationId != null)
            {
                return locationId;
            }

            return await CreateLocationByName(parts.First());
        }

        private async Task<int?> GetDeliverToIdAsync(TruxEarnings row)
        {
            var parts = row.JobName.Split(" to ").Skip(1).ToList();

            var locationId = await GetLocationIdByNameOrNullAsync(parts.Last());
            if (locationId != null)
            {
                return locationId;
            }

            if (parts.Count > 1)
            {
                locationId = await GetLocationIdByNameOrNullAsync(string.Join(" to ", parts));
                if (locationId != null)
                {
                    return locationId;
                }
            }

            return await CreateLocationByName(parts.Last());
        }

        private async Task<int?> GetLocationIdByNameOrNullAsync(string name)
        {
            return (await (await _locationRepository.GetQueryAsync())
                .Where(x => x.Name == name)
                .Select(x => new
                {
                    x.Id,
                }).FirstOrDefaultAsync())?.Id;
        }

        private async Task<int?> CreateLocationByName(string name)
        {
            var location = new Location
            {
                Name = name,
                IsActive = true,
                CategoryId = _temporaryLocationCategoryId,
            };
            await _locationRepository.InsertAsync(location);
            CurrentUnitOfWork.SaveChanges();
            return location.Id;
        }

        private void LogWarning(string text)
        {
            Logger.Warn($"Trux Earnings Import warning (tenantId: {_tenantId}, userId: {_userId}, file:{_filePath}): " + text);
        }

        private void LogError(string text)
        {
            Logger.Error($"Trux Earnings Import error (tenantId: {_tenantId}, userId: {_userId}, file:{_filePath}): " + text);
        }
    }
}
