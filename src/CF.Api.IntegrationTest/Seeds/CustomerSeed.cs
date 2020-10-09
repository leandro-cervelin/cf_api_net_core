using System;
using System.Threading.Tasks;
using CF.CustomerMngt.Infrastructure.DbContext;

namespace CF.Api.IntegrationTest.Seeds
{
    public class CustomerSeed
    {
        public static async Task Populate(CustomerMngtContext dbContext)
        {
            await dbContext.Customers.AddAsync(new CustomerMngt.Domain.Entities.Customer
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
}