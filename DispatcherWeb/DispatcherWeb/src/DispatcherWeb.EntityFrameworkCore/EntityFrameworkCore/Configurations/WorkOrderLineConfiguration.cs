using DispatcherWeb.VehicleMaintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class WorkOrderLineConfiguration : IEntityTypeConfiguration<WorkOrderLine>
    {
        public void Configure(EntityTypeBuilder<WorkOrderLine> builder)
        {
            builder
                .Property(e => e.LaborTime)
                .HasColumnType("decimal(8, 2)");
            builder
                .Property(e => e.LaborCost)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
            builder
                .Property(e => e.LaborRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
            builder
                .Property(e => e.PartsCost)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);
        }
    }
}
