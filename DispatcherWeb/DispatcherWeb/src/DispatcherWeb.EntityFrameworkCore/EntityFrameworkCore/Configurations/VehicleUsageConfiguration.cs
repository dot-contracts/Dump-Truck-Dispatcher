using DispatcherWeb.Trucks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class VehicleUsageConfiguration : IEntityTypeConfiguration<VehicleUsage>
    {
        public void Configure(EntityTypeBuilder<VehicleUsage> builder)
        {
            builder
                .Property(e => e.Reading)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);
        }
    }
}
