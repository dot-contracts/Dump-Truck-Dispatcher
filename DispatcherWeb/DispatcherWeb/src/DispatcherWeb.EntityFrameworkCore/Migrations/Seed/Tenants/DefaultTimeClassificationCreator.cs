using System.Linq;
using System.Threading.Tasks;
using Abp.Timing;
using DispatcherWeb.Configuration;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.TimeClassifications;
using Microsoft.EntityFrameworkCore;

namespace DispatcherWeb.Migrations.Seed.Tenants
{
    public class DefaultTimeClassificationCreator
    {
        private readonly DispatcherWebDbContext _context;
        private readonly int _tenantId;

        public DefaultTimeClassificationCreator(DispatcherWebDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            if (!_context.TimeClassifications.Any(x => x.TenantId == _tenantId))
            {
                var driveTruck = GetDriveTruckTimeClassification();
                _context.TimeClassifications.Add(driveTruck);
                _context.SaveChanges();

                _context.Settings.Add(GetDefaultTimeClassificationSetting(driveTruck.Id));

                foreach (var timeClassification in GetRemainingTimeClassifications())
                {
                    _context.TimeClassifications.Add(timeClassification);
                }
                _context.SaveChanges();
            }
        }

        public async Task CreateAsync()
        {
            if (!await _context.TimeClassifications.AnyAsync(x => x.TenantId == _tenantId))
            {
                var driveTruck = GetDriveTruckTimeClassification();
                _context.TimeClassifications.Add(driveTruck);
                await _context.SaveChangesAsync();

                _context.Settings.Add(GetDefaultTimeClassificationSetting(driveTruck.Id));

                foreach (var timeClassification in GetRemainingTimeClassifications())
                {
                    _context.TimeClassifications.Add(timeClassification);
                }
                await _context.SaveChangesAsync();
            }
        }

        private TimeClassification GetDriveTruckTimeClassification()
        {
            return new TimeClassification { TenantId = _tenantId, Name = "Drive Truck" };
        }

        private TimeClassification[] GetRemainingTimeClassifications()
        {
            return new[]
            {
                new TimeClassification { TenantId = _tenantId, Name = "Training" },
                new TimeClassification { TenantId = _tenantId, Name = "Vacation" },
                new TimeClassification { TenantId = _tenantId, Name = "Production Pay", IsProductionBased = true },
            };
        }

        private Abp.Configuration.Setting GetDefaultTimeClassificationSetting(int driveTruckId)
        {
            return new Abp.Configuration.Setting
            {
                Name = AppSettings.TimeAndPay.TimeTrackingDefaultTimeClassificationId,
                TenantId = _tenantId,
                Value = driveTruckId.ToString(),
                CreationTime = Clock.Now,
            };
        }
    }
}
