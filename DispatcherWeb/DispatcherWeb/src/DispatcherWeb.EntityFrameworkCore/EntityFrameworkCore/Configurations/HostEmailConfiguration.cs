using DispatcherWeb.HostEmails;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class HostEmailConfiguration : IEntityTypeConfiguration<HostEmail>
    {
        public void Configure(EntityTypeBuilder<HostEmail> builder)
        {
            builder
                .HasMany(e => e.Tenants)
                .WithOne(e => e.HostEmail)
                .HasForeignKey(e => e.HostEmailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.Roles)
                .WithOne(e => e.HostEmail)
                .HasForeignKey(e => e.HostEmailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.Editions)
                .WithOne(e => e.HostEmail)
                .HasForeignKey(e => e.HostEmailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasMany(e => e.Receivers)
                .WithOne(e => e.HostEmail)
                .HasForeignKey(e => e.HostEmailId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.CreatorUser)
                .WithMany()
                .HasForeignKey(e => e.CreatorUserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
