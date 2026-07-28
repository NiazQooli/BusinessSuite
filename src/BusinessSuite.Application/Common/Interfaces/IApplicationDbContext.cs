using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<User> Users { get; }
    DbSet<AdmUser> AdmUsers { get; }
    DbSet<AdmUserPrivilege> AdmUserPrivileges { get; }
    DbSet<SftMenuItem> SftMenuItems { get; }
    DbSet<SalSalesEntry> SalSalesEntries { get; }
    DbSet<SalSalesEntryDetail> SalSalesEntryDetails { get; }
    DbSet<SalCustomerInfo> SalCustomerInfos { get; }
    DbSet<InvItemFg> InvItemFgs { get; }
    DbSet<InvItemFgBrand> InvItemFgBrands { get; }
    DbSet<InvItemFgUom> InvItemFgUoms { get; }
    DbSet<SftFinancialYearCompany> SftFinancialYearCompanies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
