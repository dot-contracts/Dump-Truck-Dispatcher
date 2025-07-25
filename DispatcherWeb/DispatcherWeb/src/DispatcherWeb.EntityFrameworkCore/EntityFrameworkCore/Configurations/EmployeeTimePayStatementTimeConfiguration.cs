using DispatcherWeb.Drivers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class EmployeeTimePayStatementTimeConfiguration : IEntityTypeConfiguration<EmployeeTimePayStatementTime>
    {
        public void Configure(EntityTypeBuilder<EmployeeTimePayStatementTime> builder)
        {
            builder
                .HasOne(e => e.PayStatementTime)
                .WithMany(e => e.EmployeeTimeRecords)
                .HasForeignKey(e => e.PayStatementTimeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.EmployeeTime)
                .WithOne(e => e.PayStatementTime)
                .HasForeignKey<EmployeeTimePayStatementTime>(e => e.EmployeeTimeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
