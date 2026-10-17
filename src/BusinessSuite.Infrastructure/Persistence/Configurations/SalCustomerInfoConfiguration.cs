using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessSuite.Infrastructure.Persistence.Configurations;

public class SalCustomerInfoConfiguration : IEntityTypeConfiguration<SalCustomerInfo>
{
    public void Configure(EntityTypeBuilder<SalCustomerInfo> builder)
    {
        builder.ToTable("tblSalCustomerInfo", t => t.ExcludeFromMigrations());
        builder.HasKey(e => new { e.CompanyId, e.CustomerId });

        builder.Property(e => e.CompanyId).HasColumnName("CompanyID").HasColumnType("tinyint");
        builder.Property(e => e.CustomerId).HasColumnName("CustomerID").HasMaxLength(40).IsRequired();
        builder.Property(e => e.CustomerCoName).HasMaxLength(300).IsRequired();
        builder.Property(e => e.CustomerShortName).HasMaxLength(50).IsRequired();
        builder.Property(e => e.Phone1).HasMaxLength(15).IsRequired();
        builder.Property(e => e.Phone2).HasMaxLength(15).IsRequired();
        builder.Property(e => e.Phone3).HasMaxLength(15).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(75).IsRequired();
        builder.Property(e => e.Url).HasColumnName("URL").HasMaxLength(75).IsRequired();
        builder.Property(e => e.Address).HasMaxLength(300).IsRequired();
        builder.Property(e => e.Inactive);
        builder.Property(e => e.DiscCategory).HasMaxLength(10);
        builder.Property(e => e.IsDistShop).HasMaxLength(1).IsFixedLength();
    }
}
