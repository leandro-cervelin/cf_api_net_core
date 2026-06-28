using CF.Customer.Infrastructure.DbContext;
using CF.IntegrationTest.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CF.IntegrationTest.Factories;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly string _databaseName = $"Test_{Guid.NewGuid():N}";
    private string _connectionString = string.Empty;

    public async ValueTask InitializeAsync()
    {
        _connectionString = await SqlServerContainer.GetConnectionStringAsync(_databaseName);

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerContext>();
        await dbContext.Database.MigrateAsync();
        await CustomerSeed.PopulateAsync(dbContext);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var registrationsTypeToRemove = new List<Type>
            {
                typeof(DbContextOptions<CustomerContext>),
                typeof(CustomerContext)
            };

            RemoveRegistrations(services, registrationsTypeToRemove);

            services.AddDbContext<CustomerContext>(options =>
                options.UseSqlServer(_connectionString,
                    sql => sql.MigrationsAssembly("CF.Migrations")));
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
