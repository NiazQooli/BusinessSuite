using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BusinessSuite.Infrastructure.Persistence.Configurations;

public class SalSalesEntryConfiguration : IEntityTypeConfiguration<SalSalesEntry>
{
    public void Configure(EntityTypeBuilder<SalSalesEntry> builder)
    {
        builder.ToTable("tblSalSalesEntry", t => t.ExcludeFromMigrations());
        builder.HasKey(e => new { e.CompanyId, e.FinYear, e.BillType, e.SalesEntryType, e.SalesEntryId });

        builder.Property(e => e.CompanyId).HasColumnName("CompanyID").HasColumnType("tinyint");
        builder.Property(e => e.FinYear).HasMaxLength(7).IsRequired();
        builder.Property(e => e.BillType).HasMaxLength(2).IsRequired();
        builder.Property(e => e.SalesEntryType).HasMaxLength(2).IsRequired();
        builder.Property(e => e.SalesEntryId).HasColumnName("SalesEntryID");
        builder.Property(e => e.SalesEntryDate).HasColumnType("smalldatetime");
        builder.Property(e => e.OrderId).HasColumnName("OrderID");
        builder.Property(e => e.VehicleRegNo).HasMaxLength(50).IsRequired();
        builder.Property(e => e.CustomerId).HasColumnName("CustomerID").HasMaxLength(40);
        builder.Property(e => e.TotalExcStValue).HasColumnName("TotalExcSTValue").HasColumnType("money");
        builder.Property(e => e.TotalVatValue).HasColumnName("TotalVATValue").HasColumnType("money");
        builder.Property(e => e.TotalSedValue).HasColumnName("TotalSEDValue").HasColumnType("money");
        builder.Property(e => e.TotalStValue).HasColumnName("TotalSTValue").HasColumnType("money");
        builder.Property(e => e.TotalStValue17).HasColumnName("TotalSTValue_17").HasColumnType("money");
        builder.Property(e => e.TotalStValue03).HasColumnName("TotalSTValue_03").HasColumnType("money");
        builder.Property(e => e.TotalIncStValue).HasColumnName("TotalIncSTValue").HasColumnType("money");
        builder.Property(e => e.TotalDiscount1PercentageValue).HasColumnName("TotalDiscount1_Percentage_Value").HasColumnType("money");
        builder.Property(e => e.TotalDiscount2PerPackValue).HasColumnName("TotalDiscount2_PerPack_Value").HasColumnType("money");
        builder.Property(e => e.NetAmount).HasColumnType("money");
        builder.Property(e => e.TotalWeightInTons).HasColumnType("money");
        builder.Property(e => e.TotalBGheenCOilWeight).HasColumnType("money");
        builder.Property(e => e.TotalFreightCharges).HasColumnType("money");
        builder.Property(e => e.TotalSoapCharges).HasColumnType("money");
        builder.Property(e => e.CashAccId).HasColumnName("CashAccID").HasMaxLength(40);
        builder.Property(e => e.StockLocationId).HasColumnName("StockLocationID").HasColumnType("smallint");
        builder.Property(e => e.DeliveredTo).HasMaxLength(300).IsRequired();
        builder.Property(e => e.LastModifiedBy).HasMaxLength(25).IsRequired();
        builder.Property(e => e.CheckedBy).HasMaxLength(25).IsRequired();
        builder.Property(e => e.CheckedDate).HasColumnType("smalldatetime");
        builder.Property(e => e.AuthorizedBy).HasMaxLength(25).IsRequired();
        builder.Property(e => e.AuthorizedDate).HasColumnType("smalldatetime");
        builder.Property(e => e.Station).HasMaxLength(2);
        builder.Property(e => e.VouType).HasMaxLength(2);
        builder.Property(e => e.VouNo).HasMaxLength(13);
        builder.Property(e => e.Station1).HasMaxLength(2);
        builder.Property(e => e.VouType1).HasMaxLength(2);
        builder.Property(e => e.VouNo1).HasMaxLength(13);
        builder.Property(e => e.IsIncludeInIncentiveRso).HasColumnName("IsIncludeInIncentiveRSO");
        builder.Property(e => e.CreditDays).HasColumnType("tinyint");
        builder.Property(e => e.TotalCommission).HasColumnType("money");
        builder.Property(e => e.IncomeTaxPercentage).HasColumnType("smallmoney");
        builder.Property(e => e.IncomeTax).HasColumnType("money");
        builder.Property(e => e.IncomeTaxIncValue).HasColumnType("money");
        builder.Property(e => e.DespatchDate).HasColumnType("smalldatetime");
        builder.Property(e => e.SalesDiscounts).HasMaxLength(2).IsRequired();
        builder.Property(e => e.CustCodeSalesProm).HasColumnName("CustCode_SalesProm").HasMaxLength(40);
        builder.Property(e => e.Is19LtrInvoice).HasMaxLength(1).IsRequired().IsFixedLength();
        builder.Property(e => e.SkStPercentage).HasColumnName("SK_STPercentage").HasColumnType("smallmoney");
        builder.Property(e => e.Warehouse).HasMaxLength(25).IsRequired();
        builder.Property(e => e.CarriageCharges).HasColumnType("money");
        builder.Property(e => e.Currency).HasMaxLength(25).IsRequired();
        builder.Property(e => e.CurrencyRate).HasColumnType("smallmoney");
    }
}
