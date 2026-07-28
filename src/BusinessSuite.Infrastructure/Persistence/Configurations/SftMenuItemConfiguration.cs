using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessSuite.Infrastructure.Persistence.Configurations;

public class SftMenuItemConfiguration : IEntityTypeConfiguration<SftMenuItem>
{
    public void Configure(EntityTypeBuilder<SftMenuItem> builder)
    {
        builder.ToTable("tblSftMenuItems", t => t.ExcludeFromMigrations());

        builder.HasKey(m => new { m.CompanyId, m.MenuName });

        builder.Property(m => m.CompanyId)
            .HasColumnName("CompanyID")
            .HasColumnType("tinyint");

        builder.Property(m => m.MenuName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.MenuSrNo)
            .HasMaxLength(6)
            .IsRequired();

        builder.Property(m => m.MainMenuName)
            .HasMaxLength(50);

        builder.Property(m => m.MenuCaption)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.HasRights)
            .HasMaxLength(17);

        builder.Property(m => m.MenuLevel)
            .HasColumnType("tinyint");

        builder.Property(m => m.CompleteMenuName)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(m => m.ModuleName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(m => m.IsReportActive).IsRequired();

        builder.Property(m => m.MenuPriority)
            .HasColumnType("tinyint")
            .IsRequired();

        builder.Property(m => m.InactiveMenu).IsRequired();
    }
}
