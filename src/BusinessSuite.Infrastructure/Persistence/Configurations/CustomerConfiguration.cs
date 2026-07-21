using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessSuite.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(c => c.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.CompanyName).HasMaxLength(200);

        builder.HasQueryFilter(c => !c.IsDeleted);
    }
}
