using DispatcherWeb.VehicleMaintenance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class WorkOrderConfiguration : IEntityTypeConfiguration<WorkOrder>
    {
        public void Configure(EntityTypeBuilder<WorkOrder> builder)
        {
            builder
                .Property(e => e.TotalLaborCost)
                .HasColumnType("decimal(12, 2)");

            builder
                .Property(e => e.TotalPartsCost)
                .HasColumnType("decimal(12, 2)");

            builder
                .Property(e => e.TotalCost)
                .HasColumnType("decimal(12, 2)");

            builder
                .Property(e => e.Tax)
                .HasColumnType("decimal(4, 2)");

            builder
                .Property(e => e.Discount)
                .HasColumnType("decimal(5, 2)");

            builder
                .Property(e => e.Hours)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal18_2);
        }
    }
}
