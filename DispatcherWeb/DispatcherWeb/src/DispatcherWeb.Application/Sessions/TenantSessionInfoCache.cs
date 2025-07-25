using System;
using System.Linq;
using System.Threading.Tasks;
using Abp.Application.Editions;
using Abp.Application.Features;
using Abp.Application.Services.Dto;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Events.Bus.Entities;
using Abp.Events.Bus.Handlers;
using Abp.Localization;
using Abp.Runtime.Caching;
using DispatcherWeb.Editions;
using DispatcherWeb.Features;
using DispatcherWeb.MultiTenancy;
using DispatcherWeb.MultiTenancy.Payments;
using DispatcherWeb.Sessions.Dto;
using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Sessions
{
    public class TenantSessionInfoCache : ITenantSessionInfoCache, ITransientDependency,
        IAsyncEventHandler<EntityUpdatedEventData<Tenant>>,
        IAsyncEventHandler<EntityChangedEventData<Truck>>,
        IAsyncEventHandler<EntityUpdatedEventData<Edition>>,
        IAsyncEventHandler<EntityChangedEventData<SubscriptionPayment>>
    {
        public const string TenantSessionInfoCacheName = "TenantSessionInfo-Cache";

        private readonly ICacheManager _cacheManager;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly TenantManager _tenantManager;
        private readonly IFeatureManager _featureManager;
        private readonly EditionManager _editionManager;
        private readonly ILocalizationContext _localizationContext;
        private readonly IRepository<Truck> _truckRepository;
        private readonly ISubscriptionPaymentRepository _subscriptionPaymentRepository;

        public TenantSessionInfoCache(
            ICacheManager cacheManager,
            IUnitOfWorkManager unitOfWorkManager,
            TenantManager tenantManager,
            IFeatureManager featureManager,
            EditionManager editionManager,
            ILocalizationContext localizationContext,
            IRepository<Truck> truckRepository,
            ISubscriptionPaymentRepository subscriptionPaymentRepository
        )
        {
            _cacheManager = cacheManager;
            _unitOfWorkManager = unitOfWorkManager;
            _tenantManager = tenantManager;
            _featureManager = featureManager;
            _editionManager = editionManager;
            _localizationContext = localizationContext;
            _truckRepository = truckRepository;
            _subscriptionPaymentRepository = subscriptionPaymentRepository;
        }

        public async Task<TenantLoginInfoDto> GetTenantSessionInfoFromCacheOrSource(int tenantId)
        {
            var cache = GetTenantSessionInfoCache();
            var cacheItem = await cache.GetOrDefaultAsync(tenantId.ToString());

            if (cacheItem == null)
            {
                await _unitOfWorkManager.WithUnitOfWorkAsync(async () =>
                {
                    //Normally, we would map to DTO with Select() but here Edition may be SubscribableEdition. 
                    var tenant = await (await _tenantManager.GetQueryAsync())
                        .Include(x => x.Edition)
                        .AsNoTracking()
                        .FirstOrDefaultAsync(t => t.Id == tenantId);

                    if (tenant == null)
                    {
                        throw new Exception("Tenant not found!");
                    }

                    cacheItem = new TenantLoginInfoDto
                    {
                        Id = tenant.Id,
                        TenancyName = tenant.TenancyName,
                        Name = tenant.Name,
                        LogoId = tenant.LogoId,
                        LogoFileType = tenant.LogoFileType,
                        CustomCssId = tenant.CustomCssId,
                        SubscriptionEndDateUtc = tenant.SubscriptionEndDateUtc,
                        SubscriptionEndDateString = tenant.SubscriptionEndDateUtc.HasValue
                            ? tenant.SubscriptionEndDateUtc.Value.ToString("d")
                            : "Unlimited",
                        IsInTrialPeriod = tenant.IsInTrialPeriod,
                        SubscriptionPaymentType = tenant.SubscriptionPaymentType,
                        Edition = tenant.Edition switch
                        {
                            null => null,
                            SubscribableEdition se => new EditionInfoDto
                            {
                                Id = se.Id,
                                DisplayName = se.DisplayName,
                                TrialDayCount = se.TrialDayCount,
                                MonthlyPrice = se.MonthlyPrice,
                                AnnualPrice = se.AnnualPrice,
                                IsFree = se.IsFree,
                            },
                            _ => new EditionInfoDto
                            {
                                Id = tenant.Edition.Id,
                                DisplayName = tenant.Edition.DisplayName,
                            },
                        },
                        CreationTime = tenant.CreationTime,
                        CreatorUserId = tenant.CreatorUserId,
                    };

                    if (cacheItem.Edition != null)
                    {
                        var features = _featureManager.GetAll()
                            .Where(feature => (feature[FeatureMetadata.CustomFeatureKey] as FeatureMetadata)?.IsVisibleOnPricingTable ?? false);

                        var featureDictionary = features.ToDictionary(feature => feature.Name, f => f);

                        cacheItem.FeatureValues = (await _editionManager.GetFeatureValuesAsync(cacheItem.Edition.Id))
                            .Where(featureValue => featureDictionary.ContainsKey(featureValue.Name))
                            .Select(fv => new NameValueDto(
                                featureDictionary[fv.Name].DisplayName.Localize(_localizationContext),
                                featureDictionary[fv.Name].GetValueText(fv.Value, _localizationContext)
                            ))
                            .ToList();

                        cacheItem.NumberOfTrucks = await (await _truckRepository.GetQueryAsync())
                            .CountAsync(t => t.OfficeId != null && t.VehicleCategory.IsPowered);

                        var lastPayment = await (await _subscriptionPaymentRepository.GetLastCompletedPaymentQueryAsync(tenant.Id, null, null))
                            .Select(x => new
                            {
                                x.DayCount,
                            })
                            .FirstOrDefaultAsync();

                        if (lastPayment != null)
                        {
                            cacheItem.Edition.IsHighestEdition = await IsEditionHighestAsync(cacheItem.Edition.Id,
                                SubscriptionPayment.GetPaymentPeriodType(lastPayment.DayCount));
                        }
                    }


                    await cache.SetAsync(tenantId.ToString(), cacheItem);
                }, new UnitOfWorkOptions { IsTransactional = false });
            }

            return cacheItem;
        }

        public async Task InvalidateCache()
        {
            await GetTenantSessionInfoCache().ClearAsync();
        }

        public async Task InvalidateCache(int tenantId)
        {
            await GetTenantSessionInfoCache().RemoveAsync(tenantId.ToString());
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<Tenant> eventData)
        {
            await InvalidateCache(eventData.Entity.Id);
        }

        public async Task HandleEventAsync(EntityChangedEventData<Truck> eventData)
        {
            await InvalidateCache(eventData.Entity.TenantId);
        }

        public async Task HandleEventAsync(EntityUpdatedEventData<Edition> eventData)
        {
            await InvalidateCache();
        }

        public async Task HandleEventAsync(EntityChangedEventData<SubscriptionPayment> eventData)
        {
            await InvalidateCache(eventData.Entity.TenantId);
        }

        private ITypedCache<string, TenantLoginInfoDto> GetTenantSessionInfoCache()
        {
            return _cacheManager
                .GetCache(TenantSessionInfoCacheName)
                .AsTyped<string, TenantLoginInfoDto>();
        }

        private async Task<bool> IsEditionHighestAsync(int editionId, PaymentPeriodType paymentPeriodType)
        {
            var topEdition = await GetHighestEditionOrNullByPaymentPeriodTypeAsync(paymentPeriodType);
            if (topEdition == null)
            {
                return false;
            }

            return editionId == topEdition.Id;
        }

        private async Task<SubscribableEdition> GetHighestEditionOrNullByPaymentPeriodTypeAsync(PaymentPeriodType paymentPeriodType)
        {
            var editions = await _tenantManager.EditionManager.GetQueryAsync();
            if (editions == null || !await editions.AnyAsync())
            {
                return null;
            }

            var query = editions.Cast<SubscribableEdition>();

            query = paymentPeriodType switch
            {
                PaymentPeriodType.Daily => query.OrderByDescending(e => e.DailyPrice ?? 0),
                PaymentPeriodType.Weekly => query.OrderByDescending(e => e.WeeklyPrice ?? 0),
                PaymentPeriodType.Monthly => query.OrderByDescending(e => e.MonthlyPrice ?? 0),
                PaymentPeriodType.Annual => query.OrderByDescending(e => e.AnnualPrice ?? 0),
                _ => query,
            };

            return await query.FirstOrDefaultAsync();
        }
    }
}
