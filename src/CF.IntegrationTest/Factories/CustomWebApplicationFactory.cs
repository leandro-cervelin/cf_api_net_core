using CF.Api.Authentication;
using CF.Customer.Infrastructure.DbContext;
using CF.IntegrationTest.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace CF.IntegrationTest.Factories;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string SigningKey = "integration-test-signing-key-of-at-least-32-bytes";

    // Each factory gets a fresh database, so the seed customer is always the first row.
    public const long AdminCustomerId = 1;

    private readonly string _databaseName = $"Test_{Guid.NewGuid():N}";
    private string _connectionString = string.Empty;

    public async ValueTask InitializeAsync()
    {
        _connectionString = await SqlServerContainer.GetConnectionStringAsync(_databaseName);

        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<CustomerContext>();
        await dbContext.Database.MigrateAsync();
        var seedCustomerId = await CustomerSeed.PopulateAsync(dbContext);

        if (seedCustomerId != AdminCustomerId)
            throw new InvalidOperationException(
                $"Expected the seed customer to have id {AdminCustomerId} (configured admin) but got {seedCustomerId}.");
    }

    /// <summary>A client authenticated as the seed customer, who is configured as an admin.</summary>
    public HttpClient CreateAdminClient()
    {
        return CreateClientFor(AdminCustomerId, CustomerSeed.Email, CustomerSeed.SecurityStamp, [Roles.Admin]);
    }

    /// <summary>A client carrying a token minted by the app's own token service, for the given customer.</summary>
    public HttpClient CreateClientFor(long customerId, string email, string securityStamp, string[] roles)
    {
        var token = Services.GetRequiredService<ITokenService>().CreateToken(customerId, email, securityStamp, roles);
        return CreateClientWithToken(token.Token);
    }

    public HttpClient CreateClientWithToken(string accessToken)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.UseSetting("Jwt:AdminCustomerIds:0", AdminCustomerId.ToString(CultureInfo.InvariantCulture));

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
