using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessSuite.Infrastructure.Persistence.Configurations;

public class AdmUserPrivilegeConfiguration : IEntityTypeConfiguration<AdmUserPrivilege>
{
    public void Configure(EntityTypeBuilder<AdmUserPrivilege> builder)
    {
        builder.ToTable("tblAdmUserPrivilege", t => t.ExcludeFromMigrations());

        builder.HasKey(p => new { p.CompanyId, p.UserId, p.MenuName });

        builder.Property(p => p.CompanyId)
            .HasColumnName("CompanyID")
            .HasColumnType("tinyint");

        builder.Property(p => p.UserId)
            .HasColumnName("UserID")
            .HasMaxLength(25)
            .IsRequired();

        builder.Property(p => p.MenuName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.HasRights)
            .HasMaxLength(17);

        builder.Property(p => p.CanAdd).IsRequired();
        builder.Property(p => p.CanEdit).IsRequired();
        builder.Property(p => p.CanDelete).IsRequired();
        builder.Property(p => p.CanPost).IsRequired();
        builder.Property(p => p.CanPrint).IsRequired();
        builder.Property(p => p.CanView).IsRequired();
        builder.Property(p => p.CanExport).IsRequired();
        builder.Property(p => p.CanUnpost).IsRequired();
        builder.Property(p => p.CanCheck).IsRequired();
    }
}
