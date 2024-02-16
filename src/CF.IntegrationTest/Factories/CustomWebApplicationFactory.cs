using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CF.Customer.Infrastructure.DbContext;
using CF.IntegrationTest.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CF.IntegrationTest.Factories;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _connectionString = $"DataSource={Guid.NewGuid()}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var registrationsTypeToRemove = new List<Type>
            {
                typeof(DbContextOptions<CustomerContext>)
            };

            RemoveRegistrations(services, registrationsTypeToRemove);

            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkSqlite()
                .BuildServiceProvider();

            services.AddDbContext<CustomerContext>(options =>
            {
                options.UseSqlite(_connectionString);
                options.UseInternalServiceProvider(serviceProvider);
            });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            using var dbContext = scope.ServiceProvider.GetRequiredService<CustomerContext>();
            dbContext.Database.EnsureCreated();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            try
            {
                Task.FromResult(CustomerSeed.Populate(dbContext));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while seeding the database with test data.");
            }
        });

        builder.UseEnvironment("IntegrationTest");
    }

    protected static void RemoveRegistration(IServiceCollection services, Type type)
    {
        var currentRegistration = services.FirstOrDefault(c => c.ServiceType == type);
        if (currentRegistration != null) services.Remove(currentRegistration);
    }

    protected static void RemoveRegistrations(IServiceCollection services, IEnumerable<Type> types)
    {
        foreach (var type in types) RemoveRegistration(services, type);
    }
}