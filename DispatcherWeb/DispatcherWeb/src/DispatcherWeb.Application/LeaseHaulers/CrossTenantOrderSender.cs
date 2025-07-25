using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Services.Dto;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.Timing;
using Abp.UI;
using DispatcherWeb.Configuration;
using DispatcherWeb.Customers;
using DispatcherWeb.Drivers;
using DispatcherWeb.Exceptions;
using DispatcherWeb.FuelSurchargeCalculations;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Invoices;
using DispatcherWeb.Items;
using DispatcherWeb.LeaseHaulers.Dto;
using DispatcherWeb.LeaseHaulers.Dto.CrossTenantOrderSender;
using DispatcherWeb.Locations;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.Offices;
using DispatcherWeb.Orders;
using DispatcherWeb.Storage;
using DispatcherWeb.Trucks;
using DispatcherWeb.UnitsOfMeasure;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.LeaseHaulers
{
    public class CrossTenantOrderSender : DispatcherWebDomainServiceBase, ICrossTenantOrderSender
    {
        private readonly IRepository<LeaseHauler> _leaseHaulerRepository;
        private readonly IRepository<OrderLine> _orderLineRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Office> _officeRepository;
        private readonly IRepository<Item> _itemRepository;
        private readonly IRepository<Location> _locationRepository;
        private readonly IRepository<LocationCategory> _locationCategoryRepository;
        private readonly IRepository<LocationContact> _locationContactRepository;
        private readonly IRepository<UnitOfMeasure> _uomRepository;
        private readonly IRepository<FuelSurchargeCalculation> _fuelSurchargeCalculationRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<LeaseHaulerTruck> _leaseHaulerTruckRepository;
        private readonly IRepository<LeaseHaulerDriver> _leaseHaulerDriverRepository;
        private readonly IRepository<Invoice> _invoiceRepository;
        private readonly OrderTaxCalculator _orderTaxCalculator;
        private readonly IFuelSurchargeCalculator _fuelSurchargeCalculator;
        private readonly IBinaryObjectManager _binaryObjectManager;

        public TenantManager TenantManager { get; }

        public CrossTenantOrderSender(
            TenantManager tenantManager,
            IRepository<LeaseHauler> leaseHaulerRepository,
            IRepository<OrderLine> orderLineRepository,
            IRepository<Order> orderRepository,
            IRepository<Customer> customerRepository,
            IRepository<Office> officeRepository,
            IRepository<Item> itemRepository,
            IRepository<Location> locationRepository,
            IRepository<LocationCategory> locationCategoryRepository,
            IRepository<LocationContact> locationContactRepository,
            IRepository<UnitOfMeasure> uomRepository,
            IRepository<FuelSurchargeCalculation> fuelSurchargeCalculationRepository,
            IRepository<Ticket> ticketRepository,
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IRepository<LeaseHaulerTruck> leaseHaulerTruckRepository,
            IRepository<LeaseHaulerDriver> leaseHaulerDriverRepository,
            IRepository<Invoice> invoiceRepository,
            OrderTaxCalculator orderTaxCalculator,
            IFuelSurchargeCalculator fuelSurchargeCalculator,
            IBinaryObjectManager binaryObjectManager
            )
        {
            TenantManager = tenantManager;
            _leaseHaulerRepository = leaseHaulerRepository;
            _orderLineRepository = orderLineRepository;
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
            _officeRepository = officeRepository;
            _itemRepository = itemRepository;
            _locationRepository = locationRepository;
            _locationCategoryRepository = locationCategoryRepository;
            _locationContactRepository = locationContactRepository;
            _uomRepository = uomRepository;
            _fuelSurchargeCalculationRepository = fuelSurchargeCalculationRepository;
            _ticketRepository = ticketRepository;
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _leaseHaulerTruckRepository = leaseHaulerTruckRepository;
            _leaseHaulerDriverRepository = leaseHaulerDriverRepository;
            _invoiceRepository = invoiceRepository;
            _orderTaxCalculator = orderTaxCalculator;
            _fuelSurchargeCalculator = fuelSurchargeCalculator;
            _binaryObjectManager = binaryObjectManager;
        }

        public async Task<SendOrderLineToHaulingCompanyInput> GetInputForSendOrderLineToHaulingCompany(int orderLineId)
        {
            var result = new SendOrderLineToHaulingCompanyInput
            {
                OrderLineId = orderLineId,
            };

            var leaseHaulers = await (await _leaseHaulerRepository.GetQueryAsync())
                .Where(x => x.IsActive)
                .Where(x => x.HaulingCompanyTenantId.HasValue)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                })
                .Take(2)
                .ToListAsync();

            if (!leaseHaulers.Any())
            {
                throw new UserFriendlyException(L("NoLeaseHaulerFoundWithHaulingCompanyTenantIdSet"));
            }

            if (leaseHaulers.Count == 1)
            {
                result.LeaseHaulerId = leaseHaulers[0].Id;
                result.LeaseHaulerName = leaseHaulers[0].Name;
            }

            return result;
        }

        public async Task SendOrderLineToHaulingCompany(SendOrderLineToHaulingCompanyInput input)
        {
            var leaseHauler = await (await _leaseHaulerRepository.GetQueryAsync())
                .Where(x => x.Id == input.LeaseHaulerId)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.HaulingCompanyTenantId,
                }).FirstOrDefaultAsync();

            if (leaseHauler == null)
            {
                throw new UserFriendlyException("Lease Hauler wasn't found, please try again.");
            }

            if (leaseHauler.HaulingCompanyTenantId == null)
            {
                //this shouldn't happen if the filter on the dropdown is working
                throw new UserFriendlyException("Selected Lease Hauler isn't connected to a hauling company. Please try again.");
            }

            var haulingCompanyTenant = await (await TenantManager.GetQueryAsync())
                .Where(x => x.Id == leaseHauler.HaulingCompanyTenantId)
                .Select(x => new
                {
                    x.Id,
                    x.IsActive,
                }).FirstOrDefaultAsync();

            if (haulingCompanyTenant == null)
            {
                throw new UserFriendlyException("Selected Lease Hauler isn't connected to a hauling company. Please contact support if the issue persists.");
            }

            if (!haulingCompanyTenant.IsActive)
            {
                throw new UserFriendlyException("Selected Lease Hauler is connected to an inactive hauling company. Please contact support if the issue persists.");
            }

            var sourceOrderLine = await GetOrderLineDto(input.OrderLineId);

            if (sourceOrderLine.HaulingCompanyTenantId.HasValue)
            {
                throw new UserFriendlyException("This line item is already sent to a lease hauler");
            }

            if (sourceOrderLine.Order.IsClosed)
            {
                throw new UserFriendlyException("This order is marked as closed");
            }

            if (sourceOrderLine.IsCancelled)
            {
                throw new UserFriendlyException("This order line is marked as cancelled");
            }

            if (sourceOrderLine.IsComplete)
            {
                throw new UserFriendlyException("This order line is marked as complete");
            }

            var materialCompanyTenantId = await Session.GetTenantIdAsync();
            var haulingCompanyTenantId = haulingCompanyTenant.Id;
            var materialCompanyName = await SettingManager.GetSettingValueAsync(AppSettings.General.CompanyName);
            var sourceTimezone = await GetTimezone();
            var destinationTimezone = await SettingManager.GetSettingValueForTenantAsync(TimingSettingNames.TimeZone, haulingCompanyTenantId);

            using (CurrentUnitOfWork.SetTenantId(haulingCompanyTenantId))
            {
                var materialCompanyCustomer = await _customerRepository
                    .FirstOrDefaultAsync(x => x.MaterialCompanyTenantId == materialCompanyTenantId);
                if (materialCompanyCustomer == null)
                {
                    materialCompanyCustomer = new Customer
                    {
                        Name = materialCompanyName.Truncate(EntityStringFieldLengths.Customer.Name),
                        IsActive = true,
                        MaterialCompanyTenantId = materialCompanyTenantId,
                    };
                    await _customerRepository.InsertAndGetIdAsync(materialCompanyCustomer);
                }

                var destinationOfficeId = await (await _officeRepository.GetQueryAsync())
                    .OrderBy(x => x.Id)
                    .Select(x => x.Id)
                    .FirstOrDefaultAsync();

                if (destinationOfficeId == 0)
                {
                    throw new UserFriendlyException("Selected Lease Hauler doesn't have an office set. Please contact support if the error persists.");
                }

                var sourceOrder = sourceOrderLine.Order;
                var destinationOrder = await _orderRepository
                    .FirstOrDefaultAsync(x => x.MaterialCompanyOrderId == sourceOrderLine.Order.Id);
                if (destinationOrder == null)
                {
                    destinationOrder = new Order
                    {
                        TenantId = haulingCompanyTenantId,
                        DeliveryDate = sourceOrder.DeliveryDate,
                        MaterialCompanyTenantId = materialCompanyTenantId,
                        MaterialCompanyOrderId = sourceOrder.Id,
                        CustomerId = materialCompanyCustomer.Id,
                        Directions = sourceOrder.Directions,
                        IsClosed = sourceOrder.IsClosed,
                        IsPending = sourceOrder.IsPending,
                        PONumber = sourceOrder.PONumber,
                        Priority = sourceOrder.Priority,
                        SpectrumNumber = sourceOrder.SpectrumNumber,
                        OfficeId = destinationOfficeId,
                        Shift = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.General.UseShifts, haulingCompanyTenantId)
                            ? Shift.Shift1
                            : null,
                    };

                    var defaultFuelSurchargeCalculationId = await SettingManager.GetDefaultFuelSurchargeCalculationIdForTenant(haulingCompanyTenantId);
                    if (defaultFuelSurchargeCalculationId > 0)
                    {
                        var defaultFuelSurchargeCalculation = await (await _fuelSurchargeCalculationRepository.GetQueryAsync())
                            .Where(x => x.Id == defaultFuelSurchargeCalculationId)
                            .Select(x => new
                            {
                                x.BaseFuelCost,
                            })
                            .FirstOrDefaultAsync();

                        destinationOrder.FuelSurchargeCalculationId = defaultFuelSurchargeCalculationId;
                        destinationOrder.BaseFuelCost = defaultFuelSurchargeCalculation.BaseFuelCost;
                    }

                    await _orderRepository.InsertAndGetIdAsync(destinationOrder);
                }

                var destinationFreightItem = await FindOrCreateItemAsync(sourceOrderLine.FreightItem);
                var destinationMaterialItem = await FindOrCreateItemAsync(sourceOrderLine.MaterialItem);

                var freightUomName = sourceOrderLine.FreightUom?.Name ?? sourceOrderLine.MaterialUom?.Name;
                var destinationFreightUom = await FindOrGetFallbackUnitOfMeasureAsync(freightUomName);
                var destinationMaterialUom = await FindOrGetNullUnitOfMeasureAsync(sourceOrderLine.MaterialUom?.Name);

                var destinationOrderLine = new OrderLine
                {
                    MaterialCompanyTenantId = materialCompanyTenantId,
                    MaterialCompanyOrderLineId = sourceOrderLine.Id,
                    OrderId = destinationOrder.Id,
                    Designation = DesignationEnum.FreightOnly,
                    FreightItemId = destinationFreightItem?.Id,
                    MaterialItemId = destinationMaterialItem?.Id,
                    FreightUomId = destinationFreightUom.Id,
                    MaterialUomId = destinationMaterialUom?.Id,
                    FreightQuantity = sourceOrderLine.FreightQuantity,
                    MaterialQuantity = sourceOrderLine.MaterialQuantity,
                    FreightPricePerUnit = sourceOrderLine.LeaseHaulerRate,
                    FreightRateToPayDrivers = sourceOrderLine.LeaseHaulerRate,
                    TravelTime = sourceOrderLine.TravelTime,
                    FirstStaggeredTimeOnJob = sourceOrderLine.FirstStaggeredTimeOnJob,
                    MaterialPrice = 0,
                    IsMultipleLoads = sourceOrderLine.IsMultipleLoads,
                    JobNumber = sourceOrderLine.JobNumber,
                    Note = sourceOrderLine.Note,
                    NumberOfTrucks = sourceOrderLine.NumberOfTrucks,
                    StaggeredTimeInterval = sourceOrderLine.StaggeredTimeInterval,
                    StaggeredTimeKind = sourceOrderLine.StaggeredTimeKind,
                    TimeOnJob = sourceOrderLine.TimeOnJob,
                    RequireTicket = sourceOrderLine.RequireTicket,
                };

                var existingOrderLines = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => x.OrderId == destinationOrder.Id)
                    .Select(x => new
                    {
                        x.Id,
                        x.LineNumber,
                    }).ToListAsync();

                destinationOrderLine.LineNumber = existingOrderLines.Count + 1;

                if (sourceOrderLine.LoadAt != null)
                {
                    var destinationLoadAt = await FindOrCreateLocationAsync(sourceOrderLine.LoadAt);
                    destinationOrderLine.LoadAtId = destinationLoadAt.Id;
                }

                if (sourceOrderLine.DeliverTo != null)
                {
                    var destinationDeliverTo = await FindOrCreateLocationAsync(sourceOrderLine.DeliverTo);
                    destinationOrderLine.DeliverToId = destinationDeliverTo.Id;
                }

                destinationOrderLine.FreightPrice = (destinationOrderLine.FreightQuantity ?? 0) * (destinationOrderLine.FreightPricePerUnit ?? 0);

                destinationOrderLine.ProductionPay = await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.TimeAndPay.AllowProductionPay, haulingCompanyTenantId)
                    && await SettingManager.GetSettingValueForTenantAsync<bool>(AppSettings.TimeAndPay.DefaultToProductionPay, haulingCompanyTenantId);

                await _orderLineRepository.InsertAndGetIdAsync(destinationOrderLine);

                await _orderTaxCalculator.CalculateTotalsAsync(destinationOrder.Id);
                await _fuelSurchargeCalculator.RecalculateOrderLinesWithTicketsForOrder(destinationOrder.Id);

                using (CurrentUnitOfWork.SetTenantId(materialCompanyTenantId))
                {
                    var sourceOrderEntity = await _orderRepository.GetAsync(sourceOrder.Id);
                    sourceOrderEntity.HasLinkedHaulingCompanyOrders = true;
                    var sourceOrderLineEntity = await _orderLineRepository.GetAsync(sourceOrderLine.Id);
                    sourceOrderLineEntity.HaulingCompanyOrderLineId = destinationOrderLine.Id;
                    sourceOrderLineEntity.HaulingCompanyTenantId = haulingCompanyTenantId;
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }
        }

        private async Task<OrderLineDto> GetOrderLineDto(int orderLineId)
        {
            var orderLineDto = await (await _orderLineRepository.GetQueryAsync())
                .Where(x => x.Id == orderLineId)
                .Select(x => new OrderLineDto
                {
                    Id = x.Id,
                    HaulingCompanyOrderLineId = x.HaulingCompanyOrderLineId,
                    HaulingCompanyTenantId = x.HaulingCompanyTenantId,
                    MaterialCompanyOrderLineId = x.MaterialCompanyOrderLineId,
                    MaterialCompanyTenantId = x.MaterialCompanyTenantId,
                    DeliverTo = x.DeliverToId == null ? null : new LocationDto
                    {
                        Name = x.DeliverTo.Name,
                        IsActive = x.DeliverTo.IsActive,
                        Abbreviation = x.DeliverTo.Abbreviation,
                        Category = new LocationCategoryDto
                        {
                            Name = x.DeliverTo.Category.Name,
                            PredefinedLocationCategoryKind = x.DeliverTo.Category.PredefinedLocationCategoryKind,
                        },
                        StreetAddress = x.DeliverTo.StreetAddress,
                        City = x.DeliverTo.City,
                        ZipCode = x.DeliverTo.ZipCode,
                        State = x.DeliverTo.State,
                        CountryCode = x.DeliverTo.CountryCode,
                        Notes = x.DeliverTo.Notes,
                        PlaceId = x.DeliverTo.PlaceId,
                        Latitude = x.DeliverTo.Latitude,
                        Longitude = x.DeliverTo.Longitude,
                        LocationContacts = x.DeliverTo.LocationContacts.Select(c => new LocationContactDto
                        {
                            Name = c.Name,
                            Phone = c.Phone,
                            Email = c.Email,
                            Title = c.Title,
                        }).ToList(),
                    },
                    LoadAt = x.LoadAtId == null ? null : new LocationDto
                    {
                        Name = x.LoadAt.Name,
                        IsActive = x.LoadAt.IsActive,
                        Abbreviation = x.LoadAt.Abbreviation,
                        Category = new LocationCategoryDto
                        {
                            Name = x.LoadAt.Category.Name,
                            PredefinedLocationCategoryKind = x.LoadAt.Category.PredefinedLocationCategoryKind,
                        },
                        StreetAddress = x.LoadAt.StreetAddress,
                        City = x.LoadAt.City,
                        ZipCode = x.LoadAt.ZipCode,
                        State = x.LoadAt.State,
                        CountryCode = x.LoadAt.CountryCode,
                        Notes = x.LoadAt.Notes,
                        PlaceId = x.LoadAt.PlaceId,
                        Latitude = x.LoadAt.Latitude,
                        Longitude = x.LoadAt.Longitude,
                        LocationContacts = x.DeliverTo.LocationContacts.Select(c => new LocationContactDto
                        {
                            Name = c.Name,
                            Phone = c.Phone,
                            Email = c.Email,
                            Title = c.Title,
                        }).ToList(),
                    },
                    FreightPricePerUnit = x.FreightPricePerUnit,
                    LeaseHaulerRate = x.LeaseHaulerRate,
                    TravelTime = x.TravelTime,
                    FreightQuantity = x.FreightQuantity,
                    MaterialQuantity = x.MaterialQuantity,
                    FreightUom = x.FreightUomId == null ? null : new UnitOfMeasureDto
                    {
                        Name = x.FreightUom.Name,
                    },
                    MaterialUom = x.MaterialUomId == null ? null : new UnitOfMeasureDto
                    {
                        Name = x.MaterialUom.Name,
                    },
                    IsCancelled = x.IsCancelled,
                    IsComplete = x.IsComplete,
                    IsMultipleLoads = x.IsMultipleLoads,
                    LineNumber = x.LineNumber,
                    JobNumber = x.JobNumber,
                    Note = x.Note,
                    NumberOfTrucks = x.NumberOfTrucks,
                    Order = new OrderDto
                    {
                        Id = x.Order.Id,
                        DeliveryDate = x.Order.DeliveryDate,
                        Customer = new CustomerDto
                        {
                            Name = x.Order.Customer.Name,
                        },
                        Directions = x.Order.Directions,
                        IsClosed = x.Order.IsClosed,
                        IsPending = x.Order.IsPending,
                        PONumber = x.Order.PONumber,
                        Priority = x.Order.Priority,
                        SpectrumNumber = x.Order.SpectrumNumber,
                    },
                    Designation = x.Designation,
                    FreightItem = x.FreightItem == null ? null : new ItemDto
                    {
                        Name = x.FreightItem.Name,
                        Description = x.FreightItem.Description,
                        IsActive = x.FreightItem.IsActive,
                        Type = x.FreightItem.Type,
                    },
                    MaterialItem = x.MaterialItem == null ? null : new ItemDto
                    {
                        Name = x.MaterialItem.Name,
                        Description = x.MaterialItem.Description,
                        IsActive = x.MaterialItem.IsActive,
                        Type = x.MaterialItem.Type,
                    },
                    FirstStaggeredTimeOnJob = x.FirstStaggeredTimeOnJob,
                    StaggeredTimeInterval = x.StaggeredTimeInterval,
                    StaggeredTimeKind = x.StaggeredTimeKind,
                    TimeOnJob = x.TimeOnJob,
                    RequireTicket = x.RequireTicket,
                }).FirstOrDefaultAsync();

            if (orderLineDto == null)
            {
                if (await _orderLineRepository.IsEntityDeleted(new EntityDto(orderLineId), CurrentUnitOfWork))
                {
                    throw new EntityDeletedException("OrderLine", "This order line has been deleted and can’t be edited");
                }
                throw new Exception($"OrderLine with id {orderLineId} wasn't found and is not deleted");
            }

            return orderLineDto;
        }

        private async Task<Location> FindOrCreateLocationAsync(LocationDto sourceLocation)
        {
            var destinationLocation = await (await _locationRepository.GetQueryAsync())
                .FirstOrDefaultAsync(x => sourceLocation.Name == x.Name
                    && sourceLocation.Abbreviation == x.Abbreviation
                    && sourceLocation.StreetAddress == x.StreetAddress
                    && sourceLocation.City == x.City
                    && sourceLocation.ZipCode == x.ZipCode
                    && sourceLocation.State == x.State
                    && sourceLocation.CountryCode == x.CountryCode
                    && sourceLocation.PlaceId == x.PlaceId
                    && sourceLocation.Latitude == x.Latitude
                    && sourceLocation.Longitude == x.Longitude);

            if (destinationLocation != null)
            {
                return destinationLocation;
            }

            var sourceCategory = sourceLocation.Category;
            var destinationCategory = await (await _locationCategoryRepository.GetQueryAsync())
                .Where(sourceCategory.PredefinedLocationCategoryKind.HasValue
                    ? x => x.PredefinedLocationCategoryKind == sourceCategory.PredefinedLocationCategoryKind
                    : x => x.Name == sourceCategory.Name)
                .FirstOrDefaultAsync();

            if (destinationCategory == null)
            {
                destinationCategory = new LocationCategory
                {
                    Name = sourceCategory.Name,
                    PredefinedLocationCategoryKind = sourceCategory.PredefinedLocationCategoryKind,
                };
                await _locationCategoryRepository.InsertAndGetIdAsync(destinationCategory);
            }

            destinationLocation = new Location
            {
                Name = sourceLocation.Name,
                IsActive = sourceLocation.IsActive,
                Abbreviation = sourceLocation.Abbreviation,
                CategoryId = destinationCategory.Id,
                StreetAddress = sourceLocation.StreetAddress,
                City = sourceLocation.City,
                ZipCode = sourceLocation.ZipCode,
                State = sourceLocation.State,
                CountryCode = sourceLocation.CountryCode,
                Notes = sourceLocation.Notes,
                PlaceId = sourceLocation.PlaceId,
                Latitude = sourceLocation.Latitude,
                Longitude = sourceLocation.Longitude,
            };
            await _locationRepository.InsertAndGetIdAsync(destinationLocation);

            if (sourceLocation.LocationContacts.Any())
            {
                foreach (var contact in sourceLocation.LocationContacts)
                {
                    await _locationContactRepository.InsertAsync(new LocationContact
                    {
                        Name = contact.Name,
                        Phone = contact.Phone,
                        Email = contact.Email,
                        Title = contact.Title,
                        LocationId = destinationLocation.Id,
                    });
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            return destinationLocation;
        }

        private async Task<Item> FindOrCreateItemAsync(ItemDto sourceItem)
        {
            if (sourceItem == null)
            {
                return null;
            }
            var destinationItem = await _itemRepository.FirstOrDefaultAsync(x => x.Name == sourceItem.Name);
            if (destinationItem == null)
            {
                destinationItem = new Item
                {
                    Name = sourceItem.Name,
                    Description = sourceItem.Description,
                    Type = sourceItem.Type,
                    IsActive = sourceItem.IsActive,
                };
                await _itemRepository.InsertAndGetIdAsync(destinationItem);
            }

            return destinationItem;
        }

        private async Task<Driver> FindOrCreateMaterialCompanyDriverAsync(DriverDto sourceDriver, int leaseHaulerId)
        {
            var destinationDriver = await (await _driverRepository.GetQueryAsync())
                .Where(x => x.HaulingCompanyDriverId == sourceDriver.Id)
                .FirstOrDefaultAsync();
            if (destinationDriver == null)
            {
                destinationDriver = new Driver
                {
                    FirstName = sourceDriver.FirstName,
                    LastName = sourceDriver.LastName,
                    IsInactive = sourceDriver.IsInactive,
                    EmailAddress = null, //we either need to set it to null here, or update user/driver logic to not connect those readonly drivers to a user, i.e. update the logic to ignore the email
                    CellPhoneNumber = sourceDriver.CellPhoneNumber,
                    OrderNotifyPreferredFormat = sourceDriver.OrderNotifyPreferredFormat,
                    HaulingCompanyDriverId = sourceDriver.Id,
                    HaulingCompanyTenantId = sourceDriver.TenantId,
                    IsExternal = true,
                };
                await _driverRepository.InsertAndGetIdAsync(destinationDriver);
                await _leaseHaulerDriverRepository.InsertAndGetIdAsync(new LeaseHaulerDriver
                {
                    LeaseHaulerId = leaseHaulerId,
                    DriverId = destinationDriver.Id,
                });
            }

            return destinationDriver;
        }

        private async Task<Truck> FindOrCreateMaterialCompanyTruckAsync(TruckDto sourceTruck, int leaseHaulerId)
        {
            var destinationTruck = await (await _truckRepository.GetQueryAsync())
                .Where(x => x.HaulingCompanyTruckId == sourceTruck.Id)
                .FirstOrDefaultAsync();
            if (destinationTruck == null)
            {
                destinationTruck = new Truck
                {
                    TruckCode = sourceTruck.TruckCode,
                    VehicleCategoryId = sourceTruck.VehicleCategoryId,
                    IsActive = sourceTruck.IsActive,
                    InactivationDate = sourceTruck.InactivationDate,
                    CanPullTrailer = sourceTruck.CanPullTrailer,
                    HaulingCompanyTruckId = sourceTruck.Id,
                    HaulingCompanyTenantId = sourceTruck.TenantId,
                };
                if (sourceTruck.DefaultDriver != null)
                {
                    var defaultDriver = await FindOrCreateMaterialCompanyDriverAsync(sourceTruck.DefaultDriver, leaseHaulerId);
                    destinationTruck.DefaultDriverId = defaultDriver.Id;
                }
                await _truckRepository.InsertAndGetIdAsync(destinationTruck);
                await _leaseHaulerTruckRepository.InsertAndGetIdAsync(new LeaseHaulerTruck
                {
                    LeaseHaulerId = leaseHaulerId,
                    TruckId = destinationTruck.Id,
                });
            }

            return destinationTruck;
        }

        private async Task<UnitOfMeasure> FindOrGetFallbackUnitOfMeasureAsync(string uomName)
        {
            return await (await _uomRepository.GetQueryAsync())
                    .OrderByDescending(x => x.Name == uomName)
                    .FirstAsync();
        }

        private async Task<UnitOfMeasure> FindOrGetNullUnitOfMeasureAsync(string uomName)
        {
            if (uomName.IsNullOrEmpty())
            {
                return null;
            }

            return await (await _uomRepository.GetQueryAsync())
                    .OrderByDescending(x => x.Name == uomName)
                    .FirstOrDefaultAsync();
        }

        public async Task SyncLinkedOrderLines(int sourceOrderLineId, int[] alreadySyncedOrderLineIds, List<string> updatedFields, IOrderLineUpdaterFactory orderLineUpdaterFactory)
        {
            if (alreadySyncedOrderLineIds.Contains(sourceOrderLineId))
            {
                return;
            }

            var sourceOrderLine = await GetOrderLineDto(sourceOrderLineId);
            if (sourceOrderLine.HaulingCompanyOrderLineId.HasValue && sourceOrderLine.HaulingCompanyTenantId.HasValue
                && !alreadySyncedOrderLineIds.Contains(sourceOrderLine.HaulingCompanyOrderLineId.Value))
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                var materialOrderLine = sourceOrderLine;
                using (CurrentUnitOfWork.SetTenantId(materialOrderLine.HaulingCompanyTenantId.Value))
                {
                    var haulingOrderLineUpdater = orderLineUpdaterFactory.Create(materialOrderLine.HaulingCompanyOrderLineId.Value, alreadySyncedOrderLineIds.Union(new[] { sourceOrderLine.Id }).ToArray());
                    var haulingOrderLine = await haulingOrderLineUpdater.GetEntityAsync();
                    foreach (var fieldName in updatedFields)
                    {
                        switch (fieldName)
                        {
                            case nameof(OrderLine.FreightItemId):
                                var destinationFreightItem = await FindOrCreateItemAsync(sourceOrderLine.FreightItem);
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.FreightItemId, destinationFreightItem?.Id);
                                break;

                            case nameof(OrderLine.MaterialItemId):
                                var destinationMaterialItem = await FindOrCreateItemAsync(sourceOrderLine.MaterialItem);
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.MaterialItemId, destinationMaterialItem?.Id);
                                break;

                            case nameof(OrderLine.FreightUomId):
                            case nameof(OrderLine.MaterialUomId):
                                if (haulingOrderLine.Designation.MaterialOnly())
                                {
                                    await haulingOrderLineUpdater.UpdateFieldAsync(x => x.Designation, DesignationEnum.FreightOnly);
                                }
                                var destinationFreightUom = await FindOrGetFallbackUnitOfMeasureAsync(sourceOrderLine.FreightUom?.Name ?? sourceOrderLine.MaterialUom?.Name);
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.FreightUomId, destinationFreightUom.Id);
                                var destinationMaterialUom = await FindOrGetNullUnitOfMeasureAsync(sourceOrderLine.MaterialUom?.Name);
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.MaterialUomId, destinationMaterialUom?.Id);
                                break;

                            case nameof(OrderLine.FreightQuantity):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.FreightQuantity, sourceOrderLine.FreightQuantity);
                                break;

                            case nameof(OrderLine.MaterialQuantity):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.MaterialQuantity, sourceOrderLine.MaterialQuantity);
                                break;

                            case nameof(OrderLine.TravelTime):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.TravelTime, sourceOrderLine.TravelTime);
                                break;

                            case nameof(OrderLine.LeaseHaulerRate):
                                var oldFreightPricePerUnit = haulingOrderLine.FreightPricePerUnit;
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.FreightPricePerUnit, sourceOrderLine.LeaseHaulerRate); //not a typo
                                if (oldFreightPricePerUnit == haulingOrderLine.FreightRateToPayDrivers && oldFreightPricePerUnit != haulingOrderLine.FreightPricePerUnit)
                                {
                                    await haulingOrderLineUpdater.UpdateFieldAsync(x => x.FreightRateToPayDrivers, sourceOrderLine.LeaseHaulerRate);
                                }
                                break;

                            case nameof(OrderLine.FirstStaggeredTimeOnJob):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.FirstStaggeredTimeOnJob, sourceOrderLine.FirstStaggeredTimeOnJob);
                                break;

                            case nameof(OrderLine.IsMultipleLoads):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.IsMultipleLoads, sourceOrderLine.IsMultipleLoads);
                                break;

                            case nameof(OrderLine.JobNumber):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.JobNumber, sourceOrderLine.JobNumber);
                                break;

                            case nameof(OrderLine.Note):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.Note, sourceOrderLine.Note);
                                break;

                            case nameof(OrderLine.NumberOfTrucks):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.NumberOfTrucks, sourceOrderLine.NumberOfTrucks);
                                break;

                            case nameof(OrderLine.StaggeredTimeInterval):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.StaggeredTimeInterval, sourceOrderLine.StaggeredTimeInterval);
                                break;

                            case nameof(OrderLine.StaggeredTimeKind):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.StaggeredTimeKind, sourceOrderLine.StaggeredTimeKind);
                                break;

                            case nameof(OrderLine.TimeOnJob):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.TimeOnJob, sourceOrderLine.TimeOnJob);
                                break;

                            case nameof(OrderLine.RequireTicket):
                                await haulingOrderLineUpdater.UpdateFieldAsync(x => x.RequireTicket, sourceOrderLine.RequireTicket);
                                break;
                        }
                    }
                    if (updatedFields.Intersect(new[] { nameof(OrderLine.FreightQuantity), nameof(OrderLine.LeaseHaulerRate) }).Any()
                        && !haulingOrderLine.IsFreightPriceOverridden)
                    {
                        await haulingOrderLineUpdater.UpdateFieldAsync(x => x.FreightPrice, (haulingOrderLine.FreightQuantity ?? 0) * (haulingOrderLine.FreightPricePerUnit ?? 0));
                    }
                    await haulingOrderLineUpdater.SaveChangesAsync();
                    await _orderTaxCalculator.CalculateTotalsAsync(haulingOrderLine.OrderId);
                }
            }

            if (sourceOrderLine.MaterialCompanyOrderLineId.HasValue && sourceOrderLine.MaterialCompanyTenantId.HasValue
                && !alreadySyncedOrderLineIds.Contains(sourceOrderLine.MaterialCompanyOrderLineId.Value))
            {
                await CurrentUnitOfWork.SaveChangesAsync();
                var haulingOrderLine = sourceOrderLine;
                using (CurrentUnitOfWork.SetTenantId(sourceOrderLine.MaterialCompanyTenantId.Value))
                {
                    var materialOrderLineUpdater = orderLineUpdaterFactory.Create(sourceOrderLine.MaterialCompanyOrderLineId.Value, alreadySyncedOrderLineIds.Union(new[] { sourceOrderLine.Id }).ToArray());
                    var materialOrderLine = await materialOrderLineUpdater.GetEntityAsync();
                    foreach (var fieldName in updatedFields)
                    {
                        switch (fieldName)
                        {
                            case nameof(OrderLine.FreightItemId):
                                var destinationService = await FindOrCreateItemAsync(sourceOrderLine.FreightItem);
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.FreightItemId, destinationService?.Id);
                                break;

                            case nameof(OrderLine.MaterialItemId):
                                var destinationMaterialService = await FindOrCreateItemAsync(sourceOrderLine.MaterialItem);
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.MaterialItemId, destinationMaterialService?.Id);
                                break;

                            case nameof(OrderLine.FreightUomId):
                                if (materialOrderLine.Designation.MaterialOnly())
                                {
                                    await materialOrderLineUpdater.UpdateFieldAsync(x => x.Designation, DesignationEnum.FreightAndMaterial);
                                }
                                var destinationFreightUom = await FindOrGetFallbackUnitOfMeasureAsync(sourceOrderLine.FreightUom?.Name ?? sourceOrderLine.MaterialUom?.Name);
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.FreightUomId, destinationFreightUom.Id);
                                var destinationMaterialUom = await FindOrGetNullUnitOfMeasureAsync(sourceOrderLine.MaterialUom?.Name);
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.MaterialUomId, destinationMaterialUom?.Id);
                                break;

                            case nameof(OrderLine.FreightQuantity):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.FreightQuantity, sourceOrderLine.FreightQuantity);
                                break;

                            case nameof(OrderLine.MaterialQuantity):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.MaterialQuantity, sourceOrderLine.MaterialQuantity);
                                break;

                            case nameof(OrderLine.TravelTime):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.TravelTime, sourceOrderLine.TravelTime);
                                break;

                            case nameof(OrderLine.FreightPricePerUnit):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.LeaseHaulerRate, sourceOrderLine.FreightPricePerUnit); //not a typo
                                break;

                            case nameof(OrderLine.FirstStaggeredTimeOnJob):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.FirstStaggeredTimeOnJob, sourceOrderLine.FirstStaggeredTimeOnJob);
                                break;

                            case nameof(OrderLine.IsMultipleLoads):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.IsMultipleLoads, sourceOrderLine.IsMultipleLoads);
                                break;

                            case nameof(OrderLine.JobNumber):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.JobNumber, sourceOrderLine.JobNumber);
                                break;

                            case nameof(OrderLine.Note):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.Note, sourceOrderLine.Note);
                                break;

                            case nameof(OrderLine.NumberOfTrucks):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.NumberOfTrucks, sourceOrderLine.NumberOfTrucks);
                                break;

                            case nameof(OrderLine.StaggeredTimeInterval):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.StaggeredTimeInterval, sourceOrderLine.StaggeredTimeInterval);
                                break;

                            case nameof(OrderLine.StaggeredTimeKind):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.StaggeredTimeKind, sourceOrderLine.StaggeredTimeKind);
                                break;

                            case nameof(OrderLine.TimeOnJob):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.TimeOnJob, sourceOrderLine.TimeOnJob);
                                break;

                            case nameof(OrderLine.RequireTicket):
                                await materialOrderLineUpdater.UpdateFieldAsync(x => x.RequireTicket, sourceOrderLine.RequireTicket);
                                break;
                        }
                    }
                    if (updatedFields.Contains(nameof(OrderLine.FreightQuantity))
                        && !materialOrderLine.IsFreightPriceOverridden)
                    {
                        await materialOrderLineUpdater.UpdateFieldAsync(x => x.FreightPrice, (materialOrderLine.FreightQuantity ?? 0) * (materialOrderLine.FreightPricePerUnit ?? 0));
                    }
                    if (updatedFields.Contains(nameof(OrderLine.MaterialQuantity))
                        && !materialOrderLine.IsMaterialPriceOverridden)
                    {
                        await materialOrderLineUpdater.UpdateFieldAsync(x => x.MaterialPrice, (materialOrderLine.MaterialQuantity ?? 0) * (materialOrderLine.MaterialPricePerUnit ?? 0));
                    }
                    await materialOrderLineUpdater.SaveChangesAsync();
                    await _orderTaxCalculator.CalculateTotalsAsync(materialOrderLine.OrderId);
                }
            }
        }

        public async Task SendInvoiceTicketsToCustomerTenant(EntityDto input)
        {
            var invoice = await (await _invoiceRepository.GetQueryAsync())
                .Where(x => x.Id == input.Id)
                .Select(x => new
                {
                    CustomerTenantId = x.Customer.MaterialCompanyTenantId,
                }).FirstAsync();

            if (invoice.CustomerTenantId == null)
            {
                throw new UserFriendlyException("The selected customer is not linked to a tenant. Tickets for this invoice cannot be sent to customer tenant.");
            }

            var sourceTickets = await (await _ticketRepository.GetQueryAsync())
                .Where(x => x.InvoiceLine.InvoiceId == input.Id && x.MaterialCompanyTicketId == null)
                .Select(x => new TicketDto
                {
                    Id = x.Id,
                    OrderLineId = x.OrderLineId,
                    MaterialCompanyOrderLineId = x.OrderLine.MaterialCompanyOrderLineId,
                    TicketNumber = x.TicketNumber,
                    FreightQuantity = x.FreightQuantity,
                    MaterialQuantity = x.MaterialQuantity,
                    Truck = x.TruckId == null ? null : new TruckDto
                    {
                        Id = x.Truck.Id,
                        TenantId = x.Truck.TenantId,
                        TruckCode = x.Truck.TruckCode,
                        VehicleCategoryId = x.Truck.VehicleCategoryId,
                        DefaultDriver = x.Truck.DefaultDriverId == null ? null : new DriverDto
                        {
                            Id = x.Truck.DefaultDriver.Id,
                            TenantId = x.Truck.DefaultDriver.TenantId,
                            FirstName = x.Truck.DefaultDriver.FirstName,
                            LastName = x.Truck.DefaultDriver.LastName,
                            IsInactive = x.Truck.DefaultDriver.IsInactive,
                            EmailAddress = x.Truck.DefaultDriver.EmailAddress,
                            CellPhoneNumber = x.Truck.DefaultDriver.CellPhoneNumber,
                            OrderNotifyPreferredFormat = x.Truck.DefaultDriver.OrderNotifyPreferredFormat,
                        },
                        IsActive = x.Truck.IsActive,
                        InactivationDate = x.Truck.InactivationDate,
                        CanPullTrailer = x.Truck.CanPullTrailer,
                    },
                    TruckCode = x.TruckCode,
                    Trailer = x.TrailerId == null ? null : new TruckDto
                    {
                        Id = x.Trailer.Id,
                        TenantId = x.Trailer.TenantId,
                        TruckCode = x.Trailer.TruckCode,
                        VehicleCategoryId = x.Trailer.VehicleCategoryId,
                        DefaultDriver = null,
                        IsActive = x.Trailer.IsActive,
                        InactivationDate = x.Trailer.InactivationDate,
                        CanPullTrailer = x.Trailer.CanPullTrailer,
                    },
                    Driver = x.DriverId == null ? null : new DriverDto
                    {
                        Id = x.Driver.Id,
                        TenantId = x.Driver.TenantId,
                        FirstName = x.Driver.FirstName,
                        LastName = x.Driver.LastName,
                        IsInactive = x.Driver.IsInactive,
                        EmailAddress = x.Driver.EmailAddress,
                        CellPhoneNumber = x.Driver.CellPhoneNumber,
                        OrderNotifyPreferredFormat = x.Driver.OrderNotifyPreferredFormat,
                    },
                    FreightItem = x.FreightItem == null ? null : new ItemDto
                    {
                        Name = x.FreightItem.Name,
                        Description = x.FreightItem.Description,
                        IsActive = x.FreightItem.IsActive,
                        Type = x.FreightItem.Type,
                    },
                    MaterialItem = x.MaterialItem == null ? null : new ItemDto
                    {
                        Name = x.MaterialItem.Name,
                        Description = x.MaterialItem.Description,
                        IsActive = x.MaterialItem.IsActive,
                        Type = x.MaterialItem.Type,
                    },
                    DeliverTo = x.DeliverToId == null ? null : new LocationDto
                    {
                        Name = x.DeliverTo.Name,
                        IsActive = x.DeliverTo.IsActive,
                        Abbreviation = x.DeliverTo.Abbreviation,
                        Category = new LocationCategoryDto
                        {
                            Name = x.DeliverTo.Category.Name,
                            PredefinedLocationCategoryKind = x.DeliverTo.Category.PredefinedLocationCategoryKind,
                        },
                        StreetAddress = x.DeliverTo.StreetAddress,
                        City = x.DeliverTo.City,
                        ZipCode = x.DeliverTo.ZipCode,
                        State = x.DeliverTo.State,
                        CountryCode = x.DeliverTo.CountryCode,
                        Notes = x.DeliverTo.Notes,
                        PlaceId = x.DeliverTo.PlaceId,
                        Latitude = x.DeliverTo.Latitude,
                        Longitude = x.DeliverTo.Longitude,
                        LocationContacts = x.DeliverTo.LocationContacts.Select(c => new LocationContactDto
                        {
                            Name = c.Name,
                            Phone = c.Phone,
                            Email = c.Email,
                            Title = c.Title,
                        }).ToList(),
                    },
                    LoadAt = x.LoadAtId == null ? null : new LocationDto
                    {
                        Name = x.LoadAt.Name,
                        IsActive = x.LoadAt.IsActive,
                        Abbreviation = x.LoadAt.Abbreviation,
                        Category = new LocationCategoryDto
                        {
                            Name = x.LoadAt.Category.Name,
                            PredefinedLocationCategoryKind = x.LoadAt.Category.PredefinedLocationCategoryKind,
                        },
                        StreetAddress = x.LoadAt.StreetAddress,
                        City = x.LoadAt.City,
                        ZipCode = x.LoadAt.ZipCode,
                        State = x.LoadAt.State,
                        CountryCode = x.LoadAt.CountryCode,
                        Notes = x.LoadAt.Notes,
                        PlaceId = x.LoadAt.PlaceId,
                        Latitude = x.LoadAt.Latitude,
                        Longitude = x.LoadAt.Longitude,
                        LocationContacts = x.DeliverTo.LocationContacts.Select(c => new LocationContactDto
                        {
                            Name = c.Name,
                            Phone = c.Phone,
                            Email = c.Email,
                            Title = c.Title,
                        }).ToList(),
                    },
                    FreightUom = x.FreightUomId == null ? null : new UnitOfMeasureDto
                    {
                        Name = x.FreightUom.Name,
                    },
                    MaterialUom = x.MaterialUomId == null ? null : new UnitOfMeasureDto
                    {
                        Name = x.MaterialUom.Name,
                    },
                    TicketDateTime = x.TicketDateTime,
                    TicketPhotoId = x.TicketPhotoId,
                    TicketPhotoFilename = x.TicketPhotoFilename,
                    FuelSurcharge = x.FuelSurcharge,
                    //Load
                }).ToListAsync();

            if (!sourceTickets.Any())
            {
                throw new UserFriendlyException("There are no new tickets to send");
            }

            var sourceTicketIds = sourceTickets.Select(x => x.Id).ToList();
            var sourceTicketEntities = await (await _ticketRepository.GetQueryAsync()).Where(x => sourceTicketIds.Contains(x.Id)).ToListAsync();
            var ticketPairs = new List<(Ticket haulingTicket, Ticket materialTicket)>();
            var haulingCompanyTenantId = await Session.GetTenantIdAsync();
            var materialCompanyTenantId = invoice.CustomerTenantId;

            var sourceBlobs = new List<BinaryObject>();
            var sourceBlobIds = sourceTickets
                .Where(x => x.TicketPhotoId.HasValue)
                .Select(x => x.TicketPhotoId.Value)
                .ToList();
            foreach (var sourceBlobId in sourceBlobIds)
            {
                var sourceBlob = await _binaryObjectManager.GetOrNullAsync(sourceBlobId);
                if (sourceBlob != null)
                {
                    sourceBlobs.Add(sourceBlob);
                }
            }

            using (CurrentUnitOfWork.SetTenantId(materialCompanyTenantId))
            {
                var destinationOrderLineIds = sourceTickets.Select(x => x.MaterialCompanyOrderLineId).ToList();
                var destinationOrderLines = await (await _orderLineRepository.GetQueryAsync())
                    .Where(x => destinationOrderLineIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.Order.CustomerId,
                        x.Order.OfficeId,
                        x.Order.Shift,
                        x.Designation,
                    })
                    .ToListAsync();

                var destinationLeaseHauler = await (await _leaseHaulerRepository.GetQueryAsync())
                    .Where(x => x.HaulingCompanyTenantId == haulingCompanyTenantId)
                    .Select(x => new
                    {
                        x.Id,
                    }).FirstOrDefaultAsync();

                if (destinationLeaseHauler == null)
                {
                    throw new UserFriendlyException("Material Company doesn't have a lease hauler linked to your tenant. Please contact support to resolve this issue.");
                }


                foreach (var sourceTicket in sourceTickets)
                {
                    var destinationOrderLine = destinationOrderLines.FirstOrDefault(x => x.Id == sourceTicket.MaterialCompanyOrderLineId);
                    if (destinationOrderLine == null)
                    {
                        Logger.Warn("Destination Order Line wasn't found, a ticket will be skipped");
                        continue;
                    }

                    var destinationTicket = new Ticket
                    {
                        TicketNumber = sourceTicket.TicketNumber,
                        FreightQuantity = sourceTicket.FreightQuantity,
                        MaterialQuantity = sourceTicket.MaterialQuantity,
                        TruckCode = sourceTicket.TruckCode,
                        TicketDateTime = sourceTicket.TicketDateTime,
                        TicketPhotoFilename = sourceTicket.TicketPhotoFilename,
                        FuelSurcharge = sourceTicket.FuelSurcharge,
                        OrderLineId = destinationOrderLine.Id,
                        OfficeId = destinationOrderLine.OfficeId,
                        Shift = destinationOrderLine.Shift,
                        CustomerId = destinationOrderLine.CustomerId,
                        NonbillableFreight = !destinationOrderLine.Designation.HasFreight(),
                        NonbillableMaterial = !destinationOrderLine.Designation.HasMaterial(),
                        CarrierId = destinationLeaseHauler.Id,
                        HaulingCompanyTenantId = haulingCompanyTenantId,
                        HaulingCompanyTicketId = sourceTicket.Id,
                    };

                    if (sourceTicket.Truck != null)
                    {
                        var truck = await FindOrCreateMaterialCompanyTruckAsync(sourceTicket.Truck, destinationLeaseHauler.Id);
                        destinationTicket.TruckId = truck.Id;
                    }

                    if (sourceTicket.Trailer != null)
                    {
                        var trailer = await FindOrCreateMaterialCompanyTruckAsync(sourceTicket.Trailer, destinationLeaseHauler.Id);
                        destinationTicket.TrailerId = trailer.Id;
                    }

                    if (sourceTicket.Driver != null)
                    {
                        var driver = await FindOrCreateMaterialCompanyDriverAsync(sourceTicket.Driver, destinationLeaseHauler.Id);
                        destinationTicket.DriverId = driver.Id;
                    }

                    if (sourceTicket.FreightItem != null)
                    {
                        var item = await FindOrCreateItemAsync(sourceTicket.FreightItem);
                        destinationTicket.FreightItemId = item?.Id;
                    }

                    if (sourceTicket.MaterialItem != null)
                    {
                        var item = await FindOrCreateItemAsync(sourceTicket.MaterialItem);
                        destinationTicket.MaterialItemId = item?.Id;
                    }

                    if (sourceTicket.DeliverTo != null)
                    {
                        var deliverTo = await FindOrCreateLocationAsync(sourceTicket.DeliverTo);
                        destinationTicket.DeliverToId = deliverTo.Id;
                    }

                    if (sourceTicket.LoadAt != null)
                    {
                        var loadAt = await FindOrCreateLocationAsync(sourceTicket.LoadAt);
                        destinationTicket.LoadAtId = loadAt.Id;
                    }

                    if (sourceTicket.FreightUom != null)
                    {
                        var unitOfMeasure = await FindOrGetFallbackUnitOfMeasureAsync(sourceTicket.FreightUom.Name);
                        destinationTicket.FreightUomId = unitOfMeasure.Id;
                    }

                    if (sourceTicket.MaterialUom != null)
                    {
                        var unitOfMeasure = await FindOrGetFallbackUnitOfMeasureAsync(sourceTicket.MaterialUom.Name);
                        destinationTicket.MaterialUomId = unitOfMeasure.Id;
                    }

                    if (sourceTicket.TicketPhotoId != null)
                    {
                        var sourceBlob = sourceBlobs.FirstOrDefault(x => x.Id == sourceTicket.TicketPhotoId);
                        if (sourceBlob != null)
                        {
                            var destinationBinaryObject = new BinaryObject(materialCompanyTenantId, sourceBlob.Bytes, sourceBlob.Description);
                            await _binaryObjectManager.SaveAsync(destinationBinaryObject);
                            destinationTicket.TicketPhotoId = destinationBinaryObject.Id;
                        }
                    }

                    await _ticketRepository.InsertAndGetIdAsync(destinationTicket);
                    var sourceTicketEntity = sourceTicketEntities.FirstOrDefault(x => x.Id == sourceTicket.Id);
                    if (sourceTicketEntity != null)
                    {
                        ticketPairs.Add((sourceTicketEntity, destinationTicket));
                    }
                    else
                    {
                        Logger.Warn($"Source ticket entity with id {sourceTicket.Id} wasn't found");
                    }
                }
                await CurrentUnitOfWork.SaveChangesAsync();
            }

            foreach (var (haulingTicket, materialTicket) in ticketPairs)
            {
                haulingTicket.MaterialCompanyTenantId = materialCompanyTenantId;
                haulingTicket.MaterialCompanyTicketId = materialTicket.Id;
            }
            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task SyncMaterialCompanyTrucksIfNeeded(int haulingCompanyTruckId)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
            var sourceTruck = await (await _truckRepository.GetQueryAsync())
                .Where(x => x.Id == haulingCompanyTruckId)
                .Select(x => new TruckDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    TruckCode = x.TruckCode,
                    VehicleCategoryId = x.VehicleCategoryId,
                    DefaultDriver = x.DefaultDriverId == null ? null : new DriverDto
                    {
                        Id = x.DefaultDriver.Id,
                        TenantId = x.DefaultDriver.TenantId,
                        FirstName = x.DefaultDriver.FirstName,
                        LastName = x.DefaultDriver.LastName,
                        IsInactive = x.DefaultDriver.IsInactive,
                        EmailAddress = x.DefaultDriver.EmailAddress,
                        CellPhoneNumber = x.DefaultDriver.CellPhoneNumber,
                        OrderNotifyPreferredFormat = x.DefaultDriver.OrderNotifyPreferredFormat,
                    },
                    IsActive = x.IsActive,
                    InactivationDate = x.InactivationDate,
                    CanPullTrailer = x.CanPullTrailer,
                }).FirstAsync();

            List<Truck> linkedTrucks;

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
            {
                linkedTrucks = await (await _truckRepository.GetQueryAsync())
                    .Include(x => x.DefaultDriver)
                    .Include(x => x.LeaseHaulerTruck)
                    .Where(x => x.HaulingCompanyTruckId == haulingCompanyTruckId)
                    .ToListAsync();
            }

            foreach (var linkedTruck in linkedTrucks)
            {
                using (CurrentUnitOfWork.SetTenantId(linkedTruck.TenantId))
                {
                    linkedTruck.TruckCode = sourceTruck.TruckCode;
                    linkedTruck.VehicleCategoryId = sourceTruck.VehicleCategoryId;
                    linkedTruck.IsActive = sourceTruck.IsActive;
                    linkedTruck.InactivationDate = sourceTruck.InactivationDate;
                    linkedTruck.CanPullTrailer = sourceTruck.CanPullTrailer;
                    if (sourceTruck.DefaultDriver == null)
                    {
                        linkedTruck.DefaultDriverId = null;
                    }
                    else if (linkedTruck.DefaultDriver?.HaulingCompanyDriverId != sourceTruck.DefaultDriver.Id)
                    {
                        var linkedDriver = await FindOrCreateMaterialCompanyDriverAsync(sourceTruck.DefaultDriver, linkedTruck.LeaseHaulerTruck.LeaseHaulerId);
                        linkedTruck.DefaultDriverId = linkedDriver.Id;
                    }
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }
        }

        public async Task SyncMaterialCompanyDriversIfNeeded(int haulingCompanyDriverId)
        {
            await CurrentUnitOfWork.SaveChangesAsync();
            var sourceDriver = await (await _driverRepository.GetQueryAsync())
                .Where(x => x.Id == haulingCompanyDriverId)
                .Select(x => new DriverDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    IsInactive = x.IsInactive,
                    EmailAddress = x.EmailAddress,
                    CellPhoneNumber = x.CellPhoneNumber,
                    OrderNotifyPreferredFormat = x.OrderNotifyPreferredFormat,
                }).FirstAsync();

            List<Driver> linkedDrivers;

            using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MustHaveTenant))
            {
                linkedDrivers = await (await _driverRepository.GetQueryAsync())
                    .Where(x => x.HaulingCompanyDriverId == haulingCompanyDriverId)
                    .ToListAsync();
            }

            foreach (var linkedDriver in linkedDrivers)
            {
                using (CurrentUnitOfWork.SetTenantId(linkedDriver.TenantId))
                {
                    linkedDriver.FirstName = sourceDriver.FirstName;
                    linkedDriver.LastName = sourceDriver.LastName;
                    linkedDriver.IsInactive = sourceDriver.IsInactive;
                    linkedDriver.EmailAddress = null;
                    linkedDriver.CellPhoneNumber = sourceDriver.CellPhoneNumber;
                    linkedDriver.OrderNotifyPreferredFormat = sourceDriver.OrderNotifyPreferredFormat;
                    await CurrentUnitOfWork.SaveChangesAsync();
                }
            }
        }
    }
}
