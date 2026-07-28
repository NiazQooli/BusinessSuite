using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessSuite.Infrastructure.Persistence.Configurations;

public class SalSalesEntryDetailConfiguration : IEntityTypeConfiguration<SalSalesEntryDetail>
{
    public void Configure(EntityTypeBuilder<SalSalesEntryDetail> builder)
    {
        builder.ToTable("tblSalSalesEntryDetail", t => t.ExcludeFromMigrations());
        builder.HasKey(e => new { e.CompanyId, e.FinYear, e.BillType, e.SalesEntryType, e.SalesEntryId, e.FgId });

        builder.Property(e => e.CompanyId).HasColumnName("CompanyID").HasColumnType("tinyint");
        builder.Property(e => e.FinYear).HasMaxLength(7).IsRequired();
        builder.Property(e => e.BillType).HasMaxLength(2).IsRequired();
        builder.Property(e => e.SalesEntryType).HasMaxLength(2).IsRequired();
        builder.Property(e => e.SalesEntryId).HasColumnName("SalesEntryID");
        builder.Property(e => e.FgId).HasColumnName("FG_ID");
        builder.Property(e => e.NoOfPacks).HasColumnType("money");
        builder.Property(e => e.Weight).HasColumnType("money");
        builder.Property(e => e.BasicRate).HasColumnType("money");
        builder.Property(e => e.SpecialDiscount).HasColumnType("money");
        builder.Property(e => e.ExcStRate).HasColumnName("ExcSTRate").HasColumnType("money");
        builder.Property(e => e.IncStValue).HasColumnName("IncSTValue").HasColumnType("money");
        builder.Property(e => e.SedRate).HasColumnName("SEDRate").HasColumnType("money");
        builder.Property(e => e.SedAmount).HasColumnName("SEDAmount").HasColumnType("money");
        builder.Property(e => e.StRate).HasColumnName("STRate").HasColumnType("money");
        builder.Property(e => e.StAmount).HasColumnName("STAmount").HasColumnType("money");
        builder.Property(e => e.StAmount17).HasColumnName("STAmount_17").HasColumnType("money");
        builder.Property(e => e.StAmount03).HasColumnName("STAmount_03").HasColumnType("money");
        builder.Property(e => e.ExcStValue).HasColumnName("ExcSTValue").HasColumnType("money");
        builder.Property(e => e.NetRate).HasColumnType("money");
        builder.Property(e => e.FreightCharges).HasColumnType("money");
        builder.Property(e => e.SoapCharges).HasColumnType("money");
        builder.Property(e => e.EmptyCartons).HasColumnType("money");
        builder.Property(e => e.DistributorMargin).HasColumnType("money");
        builder.Property(e => e.RetailerMargin).HasColumnType("money");
        builder.Property(e => e.LeakageRate).HasColumnType("money");
        builder.Property(e => e.Discount1Percentage).HasColumnName("Discount1_Percentage").HasColumnType("money");
        builder.Property(e => e.Discount1PercentageValue).HasColumnName("Discount1_Percentage_Value").HasColumnType("money");
        builder.Property(e => e.Discount2PerPack).HasColumnName("Discount2_PerPack").HasColumnType("money");
        builder.Property(e => e.Discount2PerPackValue).HasColumnName("Discount2_PerPack_Value").HasColumnType("money");
        builder.Property(e => e.IsIncludeInIncentiveRso).HasColumnName("IsIncludeInIncentiveRSO");
        builder.Property(e => e.VatPercentage).HasColumnName("VATPercentage").HasColumnType("smallmoney");
        builder.Property(e => e.VatValue).HasColumnName("VATValue").HasColumnType("money");
    }
}
