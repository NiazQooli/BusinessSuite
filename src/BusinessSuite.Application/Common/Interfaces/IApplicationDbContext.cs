using BusinessSuite.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BusinessSuite.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Customer> Customers { get; }
    DbSet<User> Users { get; }
    DbSet<AdmUser> AdmUsers { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
