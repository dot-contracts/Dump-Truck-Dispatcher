using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class EmployeeTimeClassificationConfiguration : IEntityTypeConfiguration<EmployeeTimeClassification>
    {
        public void Configure(EntityTypeBuilder<EmployeeTimeClassification> builder)
        {
            builder
                .Property(e => e.PayRate)
                .HasColumnType(DispatcherWebConsts.DbTypeDecimal19_4);

            builder
                .HasOne(e => e.Driver)
                .WithMany(e => e.EmployeeTimeClassifications)
                .HasForeignKey(e => e.DriverId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.TimeClassification)
                .WithMany(e => e.EmployeeTimeClassifications)
                .HasForeignKey(e => e.TimeClassificationId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
