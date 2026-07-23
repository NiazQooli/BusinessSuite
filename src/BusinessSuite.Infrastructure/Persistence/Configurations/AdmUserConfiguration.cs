using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessSuite.Infrastructure.Persistence.Configurations;

public class AdmUserConfiguration : IEntityTypeConfiguration<AdmUser>
{
    public void Configure(EntityTypeBuilder<AdmUser> builder)
    {
        builder.ToTable("tblAdmUser", t => t.ExcludeFromMigrations());

        builder.HasKey(u => new { u.CompanyId, u.UserId });

        builder.Property(u => u.CompanyId)
            .HasColumnName("CompanyID")
            .HasColumnType("tinyint");

        builder.Property(u => u.UserId)
            .HasColumnName("UserID")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(u => u.Password)
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(u => u.UserName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.Enabled).IsRequired();
        builder.Property(u => u.Administrator).IsRequired();

        builder.Property(u => u.LocationId)
            .HasColumnName("LocationID")
            .HasColumnType("smallint");
    }
}
