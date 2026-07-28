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
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.Property(e => e.DiscCategory).HasMaxLength(50);
        builder.Property(e => e.IsDistShop).HasMaxLength(1);
    }
}

public class InvItemFgConfiguration : IEntityTypeConfiguration<InvItemFg>
{
    public void Configure(EntityTypeBuilder<InvItemFg> builder)
    {
        builder.ToTable("tblInvItemFG", t => t.ExcludeFromMigrations());
        builder.HasKey(e => new { e.CompanyId, e.FgId });

        builder.Property(e => e.CompanyId).HasColumnName("CompanyID").HasColumnType("tinyint");
        builder.Property(e => e.FgId).HasColumnName("FG_ID");
        builder.Property(e => e.BrandId).HasColumnName("Brand_ID").HasColumnType("smallint");
        builder.Property(e => e.FgName).HasColumnName("FGName").HasMaxLength(100).IsRequired();
        builder.Property(e => e.UomId).HasColumnName("UOM_ID").HasColumnType("smallint");
        builder.Property(e => e.NoOfUnitsInPack).HasColumnType("money");
        builder.Property(e => e.UnitWeight).HasColumnType("money");
        builder.Property(e => e.ForTonsConversion).HasColumnType("money");
        builder.Property(e => e.StPercentage).HasColumnName("STPercentage").HasColumnType("money");
        builder.Property(e => e.EmptyCartonPercentage).HasColumnType("money");
        builder.Property(e => e.DistributorMargin).HasColumnType("money");
        builder.Property(e => e.RetailerMargin).HasColumnType("money");
        builder.Property(e => e.SedPercentage).HasColumnName("SEDPercentage").HasColumnType("money");
        builder.Property(e => e.SgId).HasColumnName("SG_ID").HasColumnType("smallint");
    }
}

public class InvItemFgBrandConfiguration : IEntityTypeConfiguration<InvItemFgBrand>
{
    public void Configure(EntityTypeBuilder<InvItemFgBrand> builder)
    {
        builder.ToTable("tblInvItemFGBrand", t => t.ExcludeFromMigrations());
        builder.HasKey(e => e.BrandId);
        builder.Property(e => e.BrandId).HasColumnName("Brand_ID").HasColumnType("smallint");
        builder.Property(e => e.BrandTitle).HasColumnName("Brand_Title").HasMaxLength(100).IsRequired();
    }
}

public class InvItemFgUomConfiguration : IEntityTypeConfiguration<InvItemFgUom>
{
    public void Configure(EntityTypeBuilder<InvItemFgUom> builder)
    {
        builder.ToTable("tblInvItemFGUOM", t => t.ExcludeFromMigrations());
        builder.HasKey(e => e.UomId);
        builder.Property(e => e.UomId).HasColumnName("UOM_ID").HasColumnType("smallint");
        builder.Property(e => e.UomTitle).HasColumnName("UOM_Title").HasMaxLength(50).IsRequired();
    }
}

public class SftFinancialYearCompanyConfiguration : IEntityTypeConfiguration<SftFinancialYearCompany>
{
    public void Configure(EntityTypeBuilder<SftFinancialYearCompany> builder)
    {
        builder.ToTable("tblSftFinancialYearCompany", t => t.ExcludeFromMigrations());
        builder.HasKey(e => new { e.CompanyId, e.FinancialYear });
        builder.Property(e => e.CompanyId).HasColumnName("CompanyID").HasColumnType("tinyint");
        builder.Property(e => e.FinancialYear).HasMaxLength(7).IsRequired();
        builder.Property(e => e.IsLocked).HasMaxLength(1).IsFixedLength();
    }
}
