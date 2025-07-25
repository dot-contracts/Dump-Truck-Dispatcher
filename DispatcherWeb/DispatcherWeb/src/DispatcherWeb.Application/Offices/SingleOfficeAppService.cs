using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Domain.Repositories;
using Abp.Runtime.Caching;
using DispatcherWeb.Features;
using DispatcherWeb.Offices.Dto;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Offices
{
    public class SingleOfficeAppService : DispatcherWebDomainServiceBase, ISingleOfficeAppService
    {
        private const string SingleOfficeCache = "SingleOfficeCache";
        private const string KeyName = "SingleOfficeItem";
        private readonly ICacheManager _cacheManager;
        private readonly IRepository<Office> _officeRepository;

        public SingleOfficeAppService(
            ICacheManager cacheManager,
            IRepository<Office> officeRepository
        )
        {
            _cacheManager = cacheManager;
            _officeRepository = officeRepository;
        }

        public async Task<bool> IsSingleOffice()
        {
            var singleOfficeItem = await GetSingleOfficeItem();
            return singleOfficeItem.HasValue;
        }

        public async Task<KeyValuePair<int, string>> GetSingleOfficeIdName()
        {
            var singleOffice = await GetSingleOfficeItem();
            if (!singleOffice.HasValue)
            {
                throw new ApplicationException("There is no value!");
            }
            return singleOffice.Value;
        }

        private async Task<KeyValuePair<int, string>?> GetSingleOfficeItem()
        {
            var singleOfficeItem = await _cacheManager
                .GetCache<string, KeyValuePair<int, string>?>(SingleOfficeCache)
                .GetAsync(await GetKeyWithUserAndTenantAsync(), async () =>
                {
                    if (await Session.GetTenantIdOrNullAsync() == null)
                    {
                        return null;
                    }

                    if (!await FeatureChecker.IsEnabledAsync(AppFeatures.AllowMultiOfficeFeature))
                    {
                        var office = await (await _officeRepository.GetQueryAsync())
                            .Select(o => new { o.Id, o.Name })
                            .OrderBy(o => o.Id)
                            .FirstOrDefaultAsync();
                        return office == null ? null : new KeyValuePair<int, string>(office.Id, office.Name);
                    }
                    else
                    {
                        if (await _officeRepository.CountAsync() == 1)
                        {
                            var office = await (await _officeRepository.GetQueryAsync())
                                .Select(o => new { o.Id, o.Name })
                                .SingleAsync();
                            return (KeyValuePair<int, string>?)new KeyValuePair<int, string>(office.Id, office.Name);
                        }
                    }

                    return null;
                });
            return singleOfficeItem;
        }

        public async Task Reset()
        {
            await _cacheManager.GetCache(SingleOfficeCache).ClearAsync();
        }

        public async Task FillSingleOffice(IOfficeIdNameDto officeIdNameDto)
        {
            if (await IsSingleOffice())
            {
                if (officeIdNameDto.OfficeId == 0)
                {
                    var singleOfficeIdName = await GetSingleOfficeIdName();
                    officeIdNameDto.OfficeId = singleOfficeIdName.Key;
                    officeIdNameDto.OfficeName = singleOfficeIdName.Value;
                }
                officeIdNameDto.IsSingleOffice = true;
            }
        }

        private async Task<string> GetKeyWithUserAndTenantAsync()
        {
            return $"{await Session.GetTenantIdOrNullAsync()}_{KeyName}";
        }
    }
}
