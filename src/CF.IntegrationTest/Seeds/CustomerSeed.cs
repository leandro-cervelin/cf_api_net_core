using System;
using System.Threading.Tasks;
using CF.Customer.Domain.Services;
using CF.Customer.Infrastructure.DbContext;

namespace CF.IntegrationTest.Seeds;

public class CustomerSeed
{
    public const string Email = "seed.record@test.com";
    public const string Password = "Rgrtgr#$543gfregeg";

    // Fixed so tests can mint tokens for the seed customer. No test changes its credentials, so it never rotates.
    public const string SecurityStamp = "seedcustomersecuritystamp";

    /// <summary>Inserts the seed customer and returns its id.</summary>
    public static async Task<long> PopulateAsync(CustomerContext dbContext)
    {
        var customer = new Customer.Domain.Entities.Customer
        {
            Email = Email,
            // Stored hashed, like real records, so the seed customer can log in through the API.
            Password = new PasswordHasherService().Hash(Password),
            SecurityStamp = SecurityStamp,
            FirstName = "Seed",
            Surname = "Seed",
            Created = DateTime.Now,
            Updated = DateTime.Now
        };

        await dbContext.Customers.AddAsync(customer);
        await dbContext.SaveChangesAsync();

        return customer.Id;
    }
}