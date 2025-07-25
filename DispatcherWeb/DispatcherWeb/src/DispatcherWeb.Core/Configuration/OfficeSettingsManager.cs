using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abp.Configuration;
using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Abp.Encryption;
using DispatcherWeb.Offices;
using DispatcherWeb.Runtime.Session;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Configuration
{
    public class OfficeSettingsManager : IOfficeSettingsManager, ISingletonDependency
    {
        private readonly ISettingManager _settingManager;
        private readonly IRepository<Office> _officeRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IExtendedAbpSession _session;

        public OfficeSettingsManager(
            ISettingManager settingManager,
            IRepository<Office> officeRepository,
            IEncryptionService encryptionService,
            IExtendedAbpSession session
            )
        {
            _settingManager = settingManager;
            _officeRepository = officeRepository;
            _encryptionService = encryptionService;
            _session = session;
        }

        [UnitOfWork]
        public async Task<string> GetHeartlandPublicKeyAsync()
        {
            var office = await (await _officeRepository.GetQueryAsync())
                .Where(x => x.Id == _session.OfficeId)
                .Select(x => new
                {
                    x.HeartlandPublicKey,
                }).FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(office?.HeartlandPublicKey))
            {
                return office.HeartlandPublicKey;
            }

            var heartlandPublicKey = await _settingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey);

            return heartlandPublicKey;
        }

        [UnitOfWork]
        public async Task<string> GetHeartlandSecretKeyAsync()
        {
            var office = await (await _officeRepository.GetQueryAsync())
                .Where(x => x.Id == _session.OfficeId)
                .Select(x => new
                {
                    x.HeartlandSecretKey,
                }).FirstOrDefaultAsync();

            var officeHeartlandSecretKey = _encryptionService.DecryptIfNotEmpty(office?.HeartlandSecretKey);

            if (!string.IsNullOrEmpty(officeHeartlandSecretKey))
            {
                return officeHeartlandSecretKey;
            }

            var heartlandSecretKey = await _settingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey);

            return heartlandSecretKey;
        }

        [UnitOfWork]
        public async Task<List<OfficeHeartlandKeys>> GetHeartlandKeysForOffices()
        {
            var officeIds = await (await _officeRepository.GetQueryAsync()).Select(x => x.Id).ToListAsync();

            return await GetHeartlandKeysForOffices(officeIds);
        }

        [UnitOfWork]
        public async Task<List<OfficeHeartlandKeys>> GetHeartlandKeysForOffices(IEnumerable<int> officeIds)
        {
            var officeIdList = officeIds.ToList();

            var result = officeIdList.Select(x => new OfficeHeartlandKeys { OfficeId = x }).ToList();

            var officeSpecificKeys = await (await _officeRepository.GetQueryAsync())
                .Where(x => officeIdList.Contains(x.Id))
                .Select(x => new
                {
                    x.Id,
                    x.HeartlandPublicKey,
                    x.HeartlandSecretKey,
                }).ToListAsync();

            foreach (var key in officeSpecificKeys)
            {
                var resultItem = result.First(x => x.OfficeId == key.Id);

                if (!string.IsNullOrEmpty(key.HeartlandPublicKey))
                {
                    resultItem.PublicKey = key.HeartlandPublicKey;
                }

                var secretKey = _encryptionService.DecryptIfNotEmpty(key.HeartlandSecretKey);
                if (!string.IsNullOrEmpty(key.HeartlandSecretKey))
                {
                    resultItem.SecretKey = secretKey;
                }
            }

            if (result.All(x => !string.IsNullOrEmpty(x.PublicKey) && !string.IsNullOrEmpty(x.SecretKey)))
            {
                return result;
            }

            var tenantPublicKey = await _settingManager.GetSettingValueAsync(AppSettings.Heartland.PublicKey);
            var tenantSecretKey = await _settingManager.GetSettingValueAsync(AppSettings.Heartland.SecretKey);

            foreach (var resultItem in result)
            {
                if (string.IsNullOrEmpty(resultItem.PublicKey))
                {
                    resultItem.PublicKey = tenantPublicKey;
                }

                if (string.IsNullOrEmpty(resultItem.SecretKey))
                {
                    resultItem.SecretKey = tenantSecretKey;
                }
            }

            return result;
        }
    }
}
