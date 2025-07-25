using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Abp.Application.Services;
using Abp.Authorization;
using Abp.BackgroundJobs;
using Abp.Configuration;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using DispatcherWeb.Authorization;
using DispatcherWeb.BackgroundJobs;
using DispatcherWeb.Customers;
using DispatcherWeb.Dispatching;
using DispatcherWeb.Drivers;
using DispatcherWeb.Fulcrum.Dto;
using DispatcherWeb.Infrastructure;
using DispatcherWeb.Infrastructure.Extensions;
using DispatcherWeb.Orders;
using DispatcherWeb.TaxRates;
using DispatcherWeb.Tickets;
using DispatcherWeb.Trucks;
using DispatcherWeb.Trucks.Dto;
using DispatcherWeb.VehicleCategories;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Fulcrum
{
    [AbpAuthorize]
    public class FulcrumAppService : DispatcherWebAppServiceBase, IFulcrumAppService
    {
        private readonly FulcrumHttpClient _fulcrumHttpClient;
        private readonly IRepository<VehicleCategory> _vehicleCategoryRepository;
        private readonly IRepository<Truck> _truckRepository;
        private readonly IRepository<Driver> _driverRepository;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Items.Item> _itemRepository;
        private readonly IRepository<TaxRate> _taxRateRepository;
        private readonly IRepository<LeaseHaulers.LeaseHauler> _leaseHaulerRepository;
        private readonly ISettingManager _settingManager;
        private readonly IBackgroundJobManager _backgroundJobManager;
        private readonly IRepository<Dispatch> _dispatchRepository;
        private readonly IRepository<Ticket> _ticketRepository;
        private List<VehicleTypeDto> fulcrumVehicleTypes;
        private readonly ITicketQuantityHelper _ticketQuantityHelper;

        public FulcrumAppService(
            FulcrumHttpClient fulcrumHttpClient,
            IRepository<VehicleCategory> vehicleCategoryRepository,
            IRepository<Truck> truckRepository,
            IRepository<Driver> driverRepository,
            IRepository<Customer> customerRepository,
            IRepository<Items.Item> itemRepository,
            IRepository<TaxRate> taxRateRepository,
            IRepository<LeaseHaulers.LeaseHauler> leaseHaulerRepository,
            ISettingManager settingManager,
            IBackgroundJobManager backgroundJobManager,
            IRepository<Dispatch> dispatchRepository,
            IRepository<Ticket> ticketRepository,
            ITicketQuantityHelper ticketQuantityHelper)
        {
            _fulcrumHttpClient = fulcrumHttpClient;
            _vehicleCategoryRepository = vehicleCategoryRepository;
            _truckRepository = truckRepository;
            _driverRepository = driverRepository;
            _customerRepository = customerRepository;
            _itemRepository = itemRepository;
            _taxRateRepository = taxRateRepository;
            _leaseHaulerRepository = leaseHaulerRepository;
            _settingManager = settingManager;
            _backgroundJobManager = backgroundJobManager;
            _dispatchRepository = dispatchRepository;
            _ticketRepository = ticketRepository;
            _ticketQuantityHelper = ticketQuantityHelper;
        }

        private static class FulcrumUrlPaths
        {
            public const string Login = "v2/auth/login-jwt";
            public const string VehicleType = "v2/vehicletype";
            public const string Vehicle = "v2/vehicle";
            public const string Driver = "v2/driver";
            public const string Customer = "v2/customer";
            public const string Product = "v2/product";
            public const string TaxCode = "v2/taxcode";
            public const string Hauler = "v2/hauler";
            public const string DtdTicket = "v2/dtdticket";
        }

        private async Task<LoginDto> LoginToFulcrumApiAsync()
        {
            var settings = await _settingManager.GetFulcrumSettingsOrThrowAsync();

            var settingPayload = new
            {
                customerNumber = settings.CustomerNumber,
                email = settings.UserName,
                password = settings.Password,
            };

            return await _fulcrumHttpClient.SendFulcrumRequest<LoginDto>(HttpMethod.Post, FulcrumUrlPaths.Login, settingPayload);
        }

        [RemoteService(false)]
        [UnitOfWork(IsDisabled = true)]
        public async Task SyncFulcrumEntityAsync(FulcrumEntity fulcrumEntity)
        {
            var authData = await LoginToFulcrumApiAsync();

            switch (fulcrumEntity)
            {
                case FulcrumEntity.Truck:
                    await SyncVehicleTypeAsync(authData.Token);
                    await SyncVehicleAsync(authData.Token);
                    break;
                case FulcrumEntity.Driver:
                    await SyncDriverAsync(authData.Token);
                    break;
                case FulcrumEntity.Customer:
                    await SyncCustomerAsync(authData.Token);
                    break;
                case FulcrumEntity.Product:
                    await SyncProductAsync(authData.Token);
                    break;
                case FulcrumEntity.TaxRate:
                    await SyncTaxRateAsync(authData.Token);
                    break;
                case FulcrumEntity.LeaseHauler:
                    await SyncHaulerAsync(authData.Token);
                    break;

                default:
                    throw new UserFriendlyException(L("ApiRequestError"));
            }
        }

        #region Vehicle / Trucks

        [AbpAuthorize(AppPermissions.Pages_Trucks_SyncWithFulcrum)]
        public async Task ScheduleSyncFulcrumTrucks()
        {
            var userIdentifier = await Session.ToUserIdentifierAsync();
            await _settingManager.GetFulcrumSettingsOrThrowAsync();

            await _backgroundJobManager.EnqueueAsync<FulcrumSyncJob, FulcrumSyncJobArgs>(new FulcrumSyncJobArgs()
            {
                RequestorUser = userIdentifier,
                Entity = FulcrumEntity.Truck,
            });
        }

        private async Task SyncVehicleTypeAsync(string token)
        {
            fulcrumVehicleTypes = await _fulcrumHttpClient.GetAllPages<VehicleTypeDto>($"{FulcrumUrlPaths.VehicleType}?$top=500", null, token);
            var dtdVehicleCategories = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdOrNullAsync()))
                {
                    return await (await _vehicleCategoryRepository.GetQueryAsync())
                        .Select(x => new VehicleCategoryDto
                        {
                            Id = x.Id,
                            Name = x.Name,
                        }).ToListAsync();
                }
            });

            if (dtdVehicleCategories.Any())
            {
                foreach (var vehicleCategory in dtdVehicleCategories)
                {
                    var matchingFulcrumVehicleType = fulcrumVehicleTypes.FirstOrDefault(c => c.DtId == vehicleCategory.Id.ToString()) ?? fulcrumVehicleTypes.FirstOrDefault(c => c.Uiid == vehicleCategory.Name);

                    var returnVehicleTypeObject = matchingFulcrumVehicleType == null
                          ? await InsertOrUpdateVehicleTypeAsync(vehicleCategory, token)
                          : await InsertOrUpdateVehicleTypeAsync(vehicleCategory, token, matchingFulcrumVehicleType.Id);

                    if (returnVehicleTypeObject != null)
                    {
                        var existingVehicleType = fulcrumVehicleTypes.FirstOrDefault(c => c.Id == returnVehicleTypeObject.Id);

                        if (existingVehicleType != null)
                        {
                            existingVehicleType.Uiid = returnVehicleTypeObject.Uiid;
                            existingVehicleType.DtId = returnVehicleTypeObject.DtId;
                        }
                        else
                        {
                            fulcrumVehicleTypes.Add(returnVehicleTypeObject);
                        }
                    }

                }
            }
        }

        private async Task<VehicleTypeDto> InsertOrUpdateVehicleTypeAsync(VehicleCategoryDto model, string token, string id = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            var isInsert = string.IsNullOrEmpty(id);

            var vehicleTypeUri = FulcrumUrlPaths.VehicleType;
            vehicleTypeUri += isInsert ? string.Empty : $"/{id}";

            var vehicleTypeData = new VehicleTypeDto
            {
                Id = id,
                Uiid = model.Name,
                DtId = model.Id.ToString(),
                Inactive = false,
            };

            return await _fulcrumHttpClient.SendFulcrumRequest<VehicleTypeDto>(
                isInsert ? HttpMethod.Post : HttpMethod.Put,
                vehicleTypeUri,
                vehicleTypeData,
                token);
        }

        private async Task SyncVehicleAsync(string token)
        {
            var fulcrumVehicleList = await _fulcrumHttpClient.GetAllPages<VehicleDto>($"{FulcrumUrlPaths.Vehicle}?$top=500", null, token);
            var dtdTrucks = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdOrNullAsync()))
                {
                    return await (await _truckRepository.GetQueryAsync())
                        .Where(x => x.IsActive == true)
                        .Select(x => new VehicleDto
                        {
                            Uiid = x.TruckCode,
                            CustomerId = null,
                            DtId = x.Id.ToString(),
                            Registration = x.Plate,
                            AssetType = x.VehicleCategory.AssetType,
                            VehicleCategoryName = x.VehicleCategory.Name,
                            Inactive = false,
                        }).ToListAsync();
                }
            });

            if (dtdTrucks.Any())
            {
                foreach (var truck in dtdTrucks)
                {
                    var fulcrumVehicle = fulcrumVehicleList.FirstOrDefault(c => c.DtId == truck.DtId);

                    truck.Trailer = truck.AssetType == AssetType.DumpTruck ? "None" :
                                    truck.AssetType == AssetType.Trailer ? "Prompt" : "Required";
                    truck.VehicleTypeId = fulcrumVehicleTypes?.FirstOrDefault(v => v.Uiid == truck.VehicleCategoryName)?.Id ?? "";


                    if (fulcrumVehicle == null)
                    {
                        //For null DtId, insert only if the Uiid (Name) has no match in record, to avoid duplication error
                        if (!fulcrumVehicleList.Any(c => c.Uiid == truck.Uiid))
                        {
                            await InsertOrUpdateVehicleAsync(truck, token);
                        }
                    }
                    else
                    {
                        if (fulcrumVehicle.Uiid != truck.Uiid
                            || fulcrumVehicle.Registration != truck.Registration
                            || fulcrumVehicle.Trailer != truck.Trailer
                            || fulcrumVehicle.VehicleTypeId != truck.VehicleTypeId)
                        {
                            await InsertOrUpdateVehicleAsync(truck, token, fulcrumVehicle.Id);
                        }
                    }

                }
            }

        }

        private async Task<string> InsertOrUpdateVehicleAsync(VehicleDto model, string token, string id = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            var isInsert = string.IsNullOrEmpty(id);

            var vehicleUri = FulcrumUrlPaths.Vehicle;
            vehicleUri += isInsert ? string.Empty : $"/{id}";

            return await _fulcrumHttpClient.SendFulcrumRequest(
                isInsert ? HttpMethod.Post : HttpMethod.Put,
                vehicleUri,
                model,
                token);
        }

        #endregion

        #region Driver

        [AbpAuthorize(AppPermissions.Pages_Drivers_SyncWithFulcrum)]
        public async Task ScheduleSyncFulcrumDrivers()
        {
            var userIdentifier = await Session.ToUserIdentifierAsync();
            var settings = await _settingManager.GetFulcrumSettingsOrThrowAsync();

            await _backgroundJobManager.EnqueueAsync<FulcrumSyncJob, FulcrumSyncJobArgs>(new FulcrumSyncJobArgs()
            {
                RequestorUser = userIdentifier,
                Entity = FulcrumEntity.Driver,
            });
        }

        private async Task SyncDriverAsync(string token)
        {
            var fulcrumDrivers = await _fulcrumHttpClient.GetAllPages<FulcrumDriverDto>($"{FulcrumUrlPaths.Driver}?$top=500", null, token);
            var dtdDrivers = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdOrNullAsync()))
                {
                    return await (await _driverRepository.GetQueryAsync())
                        .Where(x => x.IsInactive == false)
                        .Select(x => new FulcrumDriverDto
                        {
                            DtId = x.Id.ToString(),
                            Uiid = $"{x.FirstName} {x.LastName}".Trim(),
                            Phone = x.CellPhoneNumber,
                            Email = x.EmailAddress,
                            Inactive = false,
                        }).ToListAsync();
                }
            });

            if (dtdDrivers.Any())
            {
                foreach (var driver in dtdDrivers)
                {
                    var fulcrumDriver = fulcrumDrivers.FirstOrDefault(c => c.DtId == driver.DtId);

                    if (fulcrumDriver == null)
                    {
                        //For null DtId, insert only if the Uiid (Name) has no match in record, to avoid duplication error
                        if (!fulcrumDrivers.Any(c => c.Uiid == driver.Uiid))
                        {
                            await InsertOrUpdateDriverAsync(driver, token);
                        }
                    }
                    else
                    {
                        if (fulcrumDriver.Uiid != driver.Uiid
                            || fulcrumDriver.Phone != driver.Phone
                            || fulcrumDriver.Email != driver.Email)
                        {
                            await InsertOrUpdateDriverAsync(driver, token, fulcrumDriver.Id);
                        }
                    }

                }
            }
        }

        private async Task<string> InsertOrUpdateDriverAsync(FulcrumDriverDto model, string token, string id = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            var isInsert = string.IsNullOrEmpty(id);
            var driverUri = FulcrumUrlPaths.Driver;
            driverUri += isInsert ? string.Empty : $"/{id}";

            return await _fulcrumHttpClient.SendFulcrumRequest(
                isInsert ? HttpMethod.Post : HttpMethod.Put,
                driverUri,
                model,
                token);
        }

        #endregion

        #region Customers

        [AbpAuthorize(AppPermissions.Pages_Customers_SyncWithFulcrum)]
        public async Task ScheduleSyncFulcrumCustomers()
        {
            var userIdentifier = await Session.ToUserIdentifierAsync();
            var settings = await _settingManager.GetFulcrumSettingsOrThrowAsync();

            await _backgroundJobManager.EnqueueAsync<FulcrumSyncJob, FulcrumSyncJobArgs>(new FulcrumSyncJobArgs()
            {
                RequestorUser = userIdentifier,
                Entity = FulcrumEntity.Customer,
            });
        }

        private async Task SyncCustomerAsync(string token)
        {
            var fulcrumCustomers = await _fulcrumHttpClient.GetAllPages<FulcrumCustomerDto>($"{FulcrumUrlPaths.Customer}?$top=500", null, token);
            var dtdCustomers = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdOrNullAsync()))
                {
                    return await (await _customerRepository.GetQueryAsync())
                        .Where(x => x.IsActive == true)
                        .Select(x => new FulcrumCustomerDto
                        {
                            Uiid = x.Name,
                            DtId = x.Id.ToString(),
                            Inactive = false,
                            Address1 = x.Address1,
                            Address2 = x.Address2,
                            City = x.City,
                            State = x.State,
                            Zip = x.ZipCode,
                            Country = x.CountryCode,
                            Notes = x.Notes,
                            Email = x.InvoiceEmail,
                            TaxExempt = x.IsTaxExempt,
                        }).ToListAsync();
                }
            });

            if (dtdCustomers.Any())
            {
                foreach (var customer in dtdCustomers)
                {
                    var matchingFulcrumCustomer = fulcrumCustomers.FirstOrDefault(c => c.DtId == customer.DtId) ?? fulcrumCustomers.FirstOrDefault(c => c.Uiid == customer.Uiid);

                    //Do not update any Customer in Fulcrum
                    if (matchingFulcrumCustomer == null)
                    {
                        await InsertCustomerAsync(customer, token);
                    }
                }
            }
        }

        private async Task<string> InsertCustomerAsync(FulcrumCustomerDto model, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            return await _fulcrumHttpClient.SendFulcrumRequest(
                HttpMethod.Post,
                FulcrumUrlPaths.Customer,
                model,
                token);
        }

        #endregion

        #region Products

        [AbpAuthorize(AppPermissions.Pages_Items_SyncWithFulcrum)]
        public async Task ScheduleSyncFulcrumProducts()
        {
            var userIdentifier = await Session.ToUserIdentifierAsync();
            var settings = await _settingManager.GetFulcrumSettingsOrThrowAsync();

            await _backgroundJobManager.EnqueueAsync<FulcrumSyncJob, FulcrumSyncJobArgs>(new FulcrumSyncJobArgs()
            {
                RequestorUser = userIdentifier,
                Entity = FulcrumEntity.Product,
            });
        }

        private async Task SyncProductAsync(string token)
        {
            var fulcrumProducts = await _fulcrumHttpClient.GetAllPages<ProductDto>($"{FulcrumUrlPaths.Product}?$top=500", null, token);
            var dtdProducts = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdOrNullAsync()))
                {
                    return await (await _itemRepository.GetQueryAsync())
                        .Where(x => x.IsActive == true)
                        .Select(x => new ProductDto
                        {
                            Uiid = x.Name,
                            DtId = x.Id.ToString(),
                            Inactive = false,
                        }).ToListAsync();
                }
            });

            if (dtdProducts.Any())
            {
                foreach (var product in dtdProducts)
                {
                    var matchingFulcrumProduct = fulcrumProducts.FirstOrDefault(c => c.DtId == product.DtId) ?? fulcrumProducts.FirstOrDefault(c => c.Uiid == product.Uiid);

                    //Do not update any Product in Fulcrum
                    if (matchingFulcrumProduct == null)
                    {
                        await InsertProductAsync(product, token);
                    }
                }
            }
        }

        private async Task<string> InsertProductAsync(ProductDto model, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            return await _fulcrumHttpClient.SendFulcrumRequest(
                HttpMethod.Post,
                FulcrumUrlPaths.Product,
                model,
                token);
        }

        #endregion

        #region TaxCode

        [AbpAuthorize(AppPermissions.Pages_Items_TaxRates_SyncWithFulcrum)]
        public async Task ScheduleSyncFulcrumTaxRate()
        {
            var userIdentifier = await Session.ToUserIdentifierAsync();
            var settings = await _settingManager.GetFulcrumSettingsOrThrowAsync();

            await _backgroundJobManager.EnqueueAsync<FulcrumSyncJob, FulcrumSyncJobArgs>(new FulcrumSyncJobArgs()
            {
                RequestorUser = userIdentifier,
                Entity = FulcrumEntity.TaxRate,
            });
        }

        private async Task SyncTaxRateAsync(string token)
        {
            var fulcrumTaxCodes = await _fulcrumHttpClient.GetAllPages<TaxCodeDto>($"{FulcrumUrlPaths.TaxCode}?$top=500", null, token);
            var dtdTaxRates = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdOrNullAsync()))
                {
                    return await (await _taxRateRepository.GetQueryAsync())
                        .Select(x => new TaxCodeDto
                        {
                            Uiid = x.Name,
                            DtId = x.Id.ToString(),
                            Inactive = false,
                            Details = new List<TaxCodeDetailDto>
                            {
                                new TaxCodeDetailDto
                                {
                                    Uiid = x.Name,
                                    Percentage = (double)x.Rate,
                                },
                            },
                        }).ToListAsync();
                }
            });

            if (dtdTaxRates.Any())
            {
                foreach (var taxRate in dtdTaxRates)
                {
                    var matchingFulcrumTaxCode = fulcrumTaxCodes.FirstOrDefault(c => c.DtId == taxRate.DtId) ?? fulcrumTaxCodes.FirstOrDefault(c => c.Uiid == taxRate.Uiid);

                    //Do not update any Tax Code in Fulcrum
                    if (matchingFulcrumTaxCode == null)
                    {
                        await InsertTaxCodeAsync(taxRate, token);
                    }
                }
            }
        }

        private async Task<string> InsertTaxCodeAsync(TaxCodeDto model, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            return await _fulcrumHttpClient.SendFulcrumRequest(
                HttpMethod.Post,
                $"{FulcrumUrlPaths.TaxCode}/full",
                model,
                token);
        }

        #endregion

        #region Lease Haulers

        [AbpAuthorize(AppPermissions.Pages_LeaseHaulers_SyncWithFulcrum)]
        public async Task ScheduleSyncFulcrumHaulers()
        {
            var userIdentifier = await Session.ToUserIdentifierAsync();
            var settings = await _settingManager.GetFulcrumSettingsOrThrowAsync();

            await _backgroundJobManager.EnqueueAsync<FulcrumSyncJob, FulcrumSyncJobArgs>(new FulcrumSyncJobArgs()
            {
                RequestorUser = userIdentifier,
                Entity = FulcrumEntity.LeaseHauler,
            });
        }

        private async Task SyncHaulerAsync(string token)
        {
            var fulcrumHaulers = await _fulcrumHttpClient.GetAllPages<HaulerDto>($"{FulcrumUrlPaths.Hauler}?$top=500", null, token);
            var dtdHaulers = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdOrNullAsync()))
                {
                    return await (await _leaseHaulerRepository.GetQueryAsync())
                        .Where(x => x.IsActive == true)
                        .Select(x => new HaulerDto
                        {
                            Uiid = x.Name,
                            DtId = x.Id.ToString(),
                            Inactive = false,
                            Address1 = x.StreetAddress1,
                            Address2 = x.StreetAddress2,
                            City = x.City,
                            State = x.State,
                            Zip = x.ZipCode,
                            Country = x.CountryCode,
                        }).ToListAsync();
                }
            });

            if (dtdHaulers.Any())
            {
                foreach (var hauler in dtdHaulers)
                {
                    var fulcrumHauler = fulcrumHaulers.FirstOrDefault(c => c.DtId == hauler.DtId);

                    if (fulcrumHauler == null)
                    {
                        //For null DtId, insert only if the Uiid (Name) has no match in record, to avoid duplication error
                        if (!fulcrumHaulers.Any(c => c.Uiid == hauler.Uiid))
                        {
                            await InsertOrUpdateHaulerAsync(hauler, token);
                        }
                    }
                    else
                    {
                        if (fulcrumHauler.Uiid != hauler.Uiid
                            || fulcrumHauler.Address1 != hauler.Address1
                            || fulcrumHauler.Address2 != hauler.Address2
                            || fulcrumHauler.City != hauler.City
                            || fulcrumHauler.State != hauler.State
                            || fulcrumHauler.Zip != hauler.Zip
                            || fulcrumHauler.Country != hauler.Country)
                        {
                            await InsertOrUpdateHaulerAsync(hauler, token, fulcrumHauler.Id);
                        }
                    }
                }
            }
        }

        private async Task<string> InsertOrUpdateHaulerAsync(HaulerDto model, string token, string id = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            var isInsert = string.IsNullOrEmpty(id);
            var haulerUri = FulcrumUrlPaths.Hauler;
            haulerUri += isInsert ? string.Empty : $"/{id}";

            return await _fulcrumHttpClient.SendFulcrumRequest(
                isInsert ? HttpMethod.Post : HttpMethod.Put,
                haulerUri,
                model,
                token);
        }

        #endregion

        #region DTD Ticket
        private async Task<List<DtdTicket>> GetAllDtdTicketsAsync(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            return await _fulcrumHttpClient.SendFulcrumRequest<List<DtdTicket>>(
                HttpMethod.Get,
                FulcrumUrlPaths.DtdTicket,
                null,
                token);
        }

        private async Task<DtdTicket> GetDtdTicketAsync(string token, string id = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            return await _fulcrumHttpClient.SendFulcrumRequest<DtdTicket>(
                HttpMethod.Get,
                $"{FulcrumUrlPaths.DtdTicket}/{id}",
                null,
                token);
        }

        private async Task<DtdTicket> AddDtdTicketAsync(DtdTicket model, string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            return await _fulcrumHttpClient.SendFulcrumRequest<DtdTicket>(
                HttpMethod.Post,
                FulcrumUrlPaths.DtdTicket,
                model,
                token);
        }

        private async Task<string> UpdateDtdTicketAsync(DtdTicket model, string token, Guid id)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            return await _fulcrumHttpClient.SendFulcrumRequest(
                HttpMethod.Put,
                $"{FulcrumUrlPaths.DtdTicket}/{id}",
                model,
                token);
        }

        private async Task<string> DeleteDtdTicketAsync(string token, Guid id)
        {
            if (string.IsNullOrEmpty(token))
            {
                throw new UserFriendlyException(L("FulcrumAuthError"));
            }

            return await _fulcrumHttpClient.SendFulcrumRequest(
                HttpMethod.Delete,
                $"{FulcrumUrlPaths.DtdTicket}/{id}",
                null,
                token);
        }

        #endregion

        #region Dispatch/Notification

        [AbpAllowAnonymous]
        [UnitOfWork(IsDisabled = true)]
        public async Task<string> CompleteDtdTicket(FulcrumTicket model)
        {
            //If the ticket is already in the system, update if there's changes, else add a new ticket in DTD
            return await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var dispatch = await FetchDispatchData(model.DispatchId);

                if (dispatch == null)
                {
                    return "Failed";
                }

                using (Session.Use(dispatch.TenantId, null))
                using (CurrentUnitOfWork.SetTenantId(dispatch.TenantId))
                {
                    Ticket ticket = await (await _ticketRepository.GetQueryAsync())
                        .Where(x => x.FulcrumDtdTicketGuid == dispatch.FulcrumDtdTicketGuid)
                        .FirstOrDefaultAsync(CancellationTokenProvider.Token);

                    if (ticket == null)
                    {
                        ticket = CreateTicketFromDispatchData(dispatch, model);
                    }

                    ticket.TicketDateTime = model.TicketTime;
                    ticket.TareWeight = model.TareWeight;
                    dispatch.MaterialQuantity = model.NetWeight;

                    await _ticketQuantityHelper.SetTicketQuantity(ticket, dispatch);
                    await _ticketRepository.InsertOrUpdateAsync(ticket);
                }

                return "Success";
            });
        }

        [RemoteService(false)]
        public async Task CreateDtdTicketToToFulcrum(int id)
        {
            var authData = await LoginToFulcrumApiAsync();
            if (authData == null) { return; }

            var dtdTicket = await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                var tenantId = await Session.GetTenantIdAsync();
                using (CurrentUnitOfWork.SetTenantId(tenantId))
                {
                    return await (await _dispatchRepository.GetQueryAsync())
                        .Where(x => x.Id == id)
                        .Select(x => new DtdTicket
                        {
                            TenantId = tenantId,
                            TruckId = x.TruckId,
                            DriverId = x.DriverId.ToString(),
                            DispatchId = x.Id,
                            MaterialId = (int)x.OrderLine.MaterialItemId,
                            FreightId = x.OrderLine.FreightItemId,
                            MaterialUnitOfMeasure = x.OrderLine.MaterialUom.Name.Replace(" ", "").Trim(),
                            FreightUnitOfMeasure = x.OrderLine.FreightUom.Name.Replace(" ", "").Trim(),
                            JobNumber = x.OrderLine.JobNumber ?? "N/A",
                            LeaseHaulerRate = x.OrderLine.LeaseHaulerRate ?? 0M,
                            MaterialPricePerUnit = x.OrderLine.MaterialPricePerUnit ?? 0M,
                            FreightPricePerUnit = x.OrderLine.FreightPricePerUnit ?? 0M,
                            IsFreightFlatRate = x.OrderLine.IsFreightPriceOverridden,
                            IsMaterialFlatRate = x.OrderLine.IsMaterialPriceOverridden,
                            TaxEntity = x.OrderLine.Order.SalesTaxEntity.Name ?? "N/A",
                            FlatRate = x.OrderLine.MaterialActualPrice,
                            MaterialPrice = x.OrderLine.MaterialActualPrice,
                            TaxRate = x.OrderLine.Order.SalesTaxRate,
                        })
                        .FirstOrDefaultAsync();
                }
            });

            if (dtdTicket != null)
            {
                var addedDtdTicket = await AddDtdTicketAsync(dtdTicket, authData.Token);
                if (addedDtdTicket != null)
                {
                    await UpdateDispatchWithFulcrumDtdTicketGuidValue(addedDtdTicket);
                }
            }

        }

        [RemoteService(false)]
        public async Task DeleteDtdTicketFromFulcrum(int id)
        {
            var authData = await LoginToFulcrumApiAsync();
            if (authData == null) { return; }

            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdOrNullAsync()))
                {
                    var dtdDispatchFulcrumTicketGuid = await (await _dispatchRepository.GetQueryAsync())
                        .Where(d => d.Id == id)
                        .Select(d => d.FulcrumDtdTicketGuid)
                        .FirstOrDefaultAsync();

                    if (dtdDispatchFulcrumTicketGuid != null)
                    {
                        await DeleteDtdTicketAsync(authData.Token, (Guid)dtdDispatchFulcrumTicketGuid);
                    }
                }
            });
        }

        private async Task UpdateDispatchWithFulcrumDtdTicketGuidValue(DtdTicket dtdTicket)
        {
            await UnitOfWorkManager.WithUnitOfWorkAsync(async () =>
            {
                using (CurrentUnitOfWork.SetTenantId(await Session.GetTenantIdOrNullAsync()))
                {
                    var dtdDispatch = await _dispatchRepository.GetAsync(dtdTicket.DispatchId);

                    if (dtdDispatch != null)
                    {
                        dtdDispatch.FulcrumDtdTicketGuid = dtdTicket.Id;
                    }
                }
            });
        }

        private async Task<FulcrumDispatchDto> FetchDispatchData(int id)
        {
            return await (await _dispatchRepository.GetQueryAsync())
                .Where(x => x.Id == id && x.FulcrumDtdTicketGuid != null)
                .Select(x => new FulcrumDispatchDto
                {
                    TenantId = x.TenantId,
                    OrderLineId = x.OrderLineId,
                    OfficeId = x.OrderLine.Order.OfficeId,
                    LoadAtId = x.OrderLine.LoadAtId,
                    DeliverToId = x.OrderLine.DeliverToId,
                    TruckId = x.TruckId,
                    TruckCode = x.Truck.TruckCode,
                    TrailerId = x.OrderLineTruck.TrailerId,
                    LeaseHaulerId = x.Truck.LeaseHaulerTruck.LeaseHaulerId,
                    CustomerId = x.OrderLine.Order.CustomerId,
                    FreightItemId = x.OrderLine.FreightItemId,
                    DriverId = x.DriverId,
                    Designation = x.OrderLine.Designation,
                    MaterialUomId = x.OrderLine.MaterialUomId,
                    FreightUomId = x.OrderLine.FreightUomId,
                    FulcrumDtdTicketGuid = x.FulcrumDtdTicketGuid,
                }).FirstOrDefaultAsync(CancellationTokenProvider.Token);
        }

        private Ticket CreateTicketFromDispatchData(FulcrumDispatchDto dispatchData, FulcrumTicket model)
        {
            return new Ticket
            {
                OrderLineId = dispatchData.OrderLineId,
                OfficeId = dispatchData.OfficeId,
                LoadAtId = dispatchData.LoadAtId,
                DeliverToId = dispatchData.DeliverToId,
                TruckId = dispatchData.TruckId,
                TruckCode = dispatchData.TruckCode,
                TrailerId = dispatchData.TrailerId,
                CarrierId = dispatchData.LeaseHaulerId,
                CustomerId = dispatchData.CustomerId,
                DriverId = dispatchData.DriverId,
                TenantId = dispatchData.TenantId,
                TicketDateTime = model.TicketTime,
                NonbillableMaterial = !dispatchData.Designation.HasMaterial(),
                NonbillableFreight = !dispatchData.Designation.HasFreight(),
                TicketNumber = model.TicketNumber.ToString().TruncateWithPostfix(EntityStringFieldLengths.Ticket.TicketNumber),
                TareWeight = model.TareWeight,
                FulcrumDtdTicketGuid = dispatchData.FulcrumDtdTicketGuid,
            };
        }

        #endregion
    }
}
