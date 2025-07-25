using DispatcherWeb.VehicleMaintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class VehicleServiceConfiguration : IEntityTypeConfiguration<VehicleService>
    {
        public void Configure(EntityTypeBuilder<VehicleService> builder)
        {
            builder
                .Property(e => e.RecommendedHourInterval)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);

            builder
                .Property(e => e.WarningHours)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);
        }
    }
}
