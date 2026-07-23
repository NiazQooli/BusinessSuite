using BusinessSuite.Domain.Entities;
using BusinessSuite.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BusinessSuite.Infrastructure.Persistence;

public static class ApplicationDbContextSeed
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();

            if (!await context.Users.IgnoreQueryFilters().AnyAsync())
            {
                var admin = User.Create(
                    "admin@fresheressential.com",
                    passwordHasher.Hash("Fresher@123"),
                    "Fresher Admin");

                context.Users.Add(admin);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded default Fresher Essential admin user.");
            }

            if (await context.Customers.IgnoreQueryFilters().AnyAsync())
            {
                return;
            }

            context.Customers.AddRange(
                Customer.Create("Acme Trading Ltd", "billing@acmetrading.co.uk", "+44 20 7946 0958", "Acme Trading Ltd"),
                Customer.Create("Northern Supplies", "orders@northernsupplies.co.uk", "+44 161 496 0123", "Northern Supplies"),
                Customer.Create("Coastal Services", "hello@coastalservices.co.uk", null, "Coastal Services Ltd"));

            await context.SaveChangesAsync();
            logger.LogInformation("Database seeded with sample customers.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }
}
