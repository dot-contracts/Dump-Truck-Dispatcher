using DispatcherWeb.Authorization.Users;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            //builder
            //    .Property(e => e.PhoneNumber)
            //    .HasMaxLength(User.MaxPhoneNumberLength);
        }
    }
}
