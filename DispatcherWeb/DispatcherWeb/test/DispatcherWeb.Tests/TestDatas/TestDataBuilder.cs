using System.Linq;
using DispatcherWeb.EntityFrameworkCore;
using DispatcherWeb.Migrations.Seed.Host;
using DispatcherWeb.Migrations.Seed.Tenants;
using DispatcherWeb.UnitsOfMeasure;

namespace DispatcherWeb.Tests.TestDatas
{
    public class TestDataBuilder
    {
        private readonly DispatcherWebDbContext _context;
        private readonly int _tenantId;

        public TestDataBuilder(DispatcherWebDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            new InitialHostDbBuilder(_context).Create();
            CreateTestUomBases();

            new TestEditionsBuilder(_context).Create();

            var tenantId = new TestTenantBuilder(_context).Create();

            new TestOrganizationUnitsBuilder(_context, tenantId).Create();
            new TestSubscriptionPaymentBuilder(_context, tenantId).Create();
            new DefaultLanguagesCreator(_context).Create(tenantId);

            new DefaultLocationsCreator(_context, tenantId).Create();
            new DefaultServiceCreator(_context, tenantId).Create();
            new DefaultUnitOfMeasureCreator(_context, tenantId).Create();
            new DefaultTimeClassificationCreator(_context, tenantId).Create();

            _context.SaveChanges();
        }

        private void CreateTestUomBases()
        {
            if (!_context.UnitOfMeasureBases.Any())
            {
                var uoms = new[]
                {
                    UnitOfMeasureBaseEnum.Hours,
                    UnitOfMeasureBaseEnum.Tons,
                    UnitOfMeasureBaseEnum.Loads,
                    UnitOfMeasureBaseEnum.CubicYards,
                    UnitOfMeasureBaseEnum.Each,
                    UnitOfMeasureBaseEnum.CubicMeters,
                    UnitOfMeasureBaseEnum.Miles,
                    UnitOfMeasureBaseEnum.DriveMiles,
                    UnitOfMeasureBaseEnum.AirMiles,
                    UnitOfMeasureBaseEnum.DriveKMs,
                    UnitOfMeasureBaseEnum.AirKMs,
                };

                foreach (var uom in uoms)
                {
                    _context.UnitOfMeasureBases.Add(new UnitOfMeasureBase
                    {
                        Id = (int)uom,
                        Name = uom.ToString(),
                    });
                }

                _context.SaveChanges();
            }
        }
    }
}
