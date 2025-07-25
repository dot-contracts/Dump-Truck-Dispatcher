using DispatcherWeb.LeaseHaulers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations;

public class LeaseHaulerUserConfiguration : IEntityTypeConfiguration<LeaseHaulerUser>
{
    public void Configure(EntityTypeBuilder<LeaseHaulerUser> builder)
    {
        builder
            .HasOne(x => x.LeaseHauler)
            .WithMany(x => x.LeaseHaulerUsers)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasOne(x => x.User)
            .WithOne(x => x.LeaseHaulerUser)
            .HasForeignKey<LeaseHaulerUser>(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}