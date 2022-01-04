using System;
using System.Threading.Tasks;
using CF.Customer.Infrastructure.DbContext;

namespace CF.IntegrationTest.Seeds;

public class CustomerSeed
{
    public static async Task Populate(CustomerContext dbContext)
    {
        await dbContext.Customers.AddAsync(new Customer.Domain.Entities.Customer
        {
            Email = "seed.record@test.com",
            Password = "Rgrtgr#$543gfregeg",
            FirstName = "Seed",
            Surname = "Seed",
            Created = DateTime.Now,
            Updated = DateTime.Now
        });

        await dbContext.SaveChangesAsync();
    }
}