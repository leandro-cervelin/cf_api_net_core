using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CF.Api;
using CF.Customer.Infrastructure.DbContext;
using CF.IntegrationTest.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace CF.IntegrationTest.Factories
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<Startup>
    {
        private readonly string _connectionString = $"DataSource={Guid.NewGuid()}.db";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Removing Existing Registrations
                var registrationsTypeToRemove = new List<Type>
                {
                    typeof(DbContextOptions<CustomerContext>)
                };

                RemoveRegistrations(services, registrationsTypeToRemove);

                // CreateAsync a new service provider.
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkSqlite()
                    .BuildServiceProvider();

                // Add a database context (AppDbContext) using an in-memory database for testing.
                services.AddDbContext<CustomerContext>(options =>
                {
                    options.UseSqlite(_connectionString);
                    options.UseInternalServiceProvider(serviceProvider);
                });

                // Build the service provider.
                var buildServiceProvider = services.BuildServiceProvider();

                // CreateAsync a scope to obtain a reference to the database contexts
                using var scope = buildServiceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var dbContext = scopedServices.GetRequiredService<CustomerContext>();

                var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory<TStartup>>>();

                // Ensure the database is created.
                dbContext.Database.EnsureCreated();

                try
                {
                    // Seed the database with some specific test data.
                    Task.FromResult(CustomerSeed.Populate(dbContext));
                }
                catch (Exception ex)
                {
                    logger.LogError(
                        "An error occurred seeding the database with test messages. Error: {ex.Message}, {ex}",
                        ex.Message, ex);
                }
            });
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
}