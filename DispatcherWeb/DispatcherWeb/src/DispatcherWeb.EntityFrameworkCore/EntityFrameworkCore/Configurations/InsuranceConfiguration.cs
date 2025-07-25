using DispatcherWeb.Insurances;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DispatcherWeb.EntityFrameworkCore.Configurations
{
    public class InsuranceConfiguration : IEntityTypeConfiguration<Insurance>
    {
        public void Configure(EntityTypeBuilder<Insurance> builder)
        {
            builder
                .HasOne(x => x.InsuranceType)
                .WithMany(x => x.Insurances)
                .HasForeignKey(x => x.InsuranceTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(x => x.LeaseHauler)
                .WithMany(x => x.LeaseHaulerInsurances)
                .HasForeignKey(x => x.LeaseHaulerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
