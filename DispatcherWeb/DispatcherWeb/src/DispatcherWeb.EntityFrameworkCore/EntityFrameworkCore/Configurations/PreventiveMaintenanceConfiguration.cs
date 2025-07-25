using DispatcherWeb.VehicleMaintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class PreventiveMaintenanceConfiguration : IEntityTypeConfiguration<PreventiveMaintenance>
    {
        public void Configure(EntityTypeBuilder<PreventiveMaintenance> builder)
        {
            builder
                .Property(e => e.DueHour)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);

            builder
                .Property(e => e.LastHour)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);

            builder
                .Property(e => e.WarningHour)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);
        }
    }
}
