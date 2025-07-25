using DispatcherWeb.LeaseHaulers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class LeaseHaulerConfiguration : IEntityTypeConfiguration<LeaseHauler>
    {
        public void Configure(EntityTypeBuilder<LeaseHauler> builder)
        {
            builder
                .HasMany(e => e.LeaseHaulerContacts)
                .WithOne(e => e.LeaseHauler)
                .HasForeignKey(e => e.LeaseHaulerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
