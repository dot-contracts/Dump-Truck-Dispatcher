using DispatcherWeb.VehicleMaintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class VehicleServiceTypeConfiguration : IEntityTypeConfiguration<VehicleServiceType>
    {
        public void Configure(EntityTypeBuilder<VehicleServiceType> builder)
        {
            builder
                .HasIndex(e => e.Name)
                .HasDatabaseName("IX_Name");
        }
    }
}
