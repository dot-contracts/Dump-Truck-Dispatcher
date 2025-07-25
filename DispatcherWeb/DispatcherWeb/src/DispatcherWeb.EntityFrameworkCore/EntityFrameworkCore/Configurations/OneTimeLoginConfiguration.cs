using DispatcherWeb.Authorization.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class OneTimeLoginConfiguration : IEntityTypeConfiguration<OneTimeLogin>
    {
        public void Configure(EntityTypeBuilder<OneTimeLogin> builder)
        {
            builder
                .HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}