using BusinessSuite.Application.Common.Interfaces;
using BusinessSuite.Domain.Common;
using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTimeProvider dateTimeProvider)
        : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<User> Users => Set<User>();
    public DbSet<AdmUser> AdmUsers => Set<AdmUser>();
    public DbSet<AdmUserPrivilege> AdmUserPrivileges => Set<AdmUserPrivilege>();
    public DbSet<SftMenuItem> SftMenuItems => Set<SftMenuItem>();
    public DbSet<SalSalesEntry> SalSalesEntries => Set<SalSalesEntry>();
    public DbSet<SalSalesEntryDetail> SalSalesEntryDetails => Set<SalSalesEntryDetail>();
    public DbSet<SalCustomerInfo> SalCustomerInfos => Set<SalCustomerInfo>();
    public DbSet<InvItemFg> InvItemFgs => Set<InvItemFg>();
    public DbSet<InvItemFgBrand> InvItemFgBrands => Set<InvItemFgBrand>();
    public DbSet<InvItemFgUom> InvItemFgUoms => Set<InvItemFgUom>();
    public DbSet<SftFinancialYearCompany> SftFinancialYearCompanies => Set<SftFinancialYearCompany>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAtUtc = _dateTimeProvider.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAtUtc = _dateTimeProvider.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
