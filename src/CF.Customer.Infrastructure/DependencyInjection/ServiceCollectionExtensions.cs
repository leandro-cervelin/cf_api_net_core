using CF.Customer.Application.Facades;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Application.Mappers;
using CF.Customer.Domain.Repositories;
using CF.Customer.Domain.Services;
using CF.Customer.Domain.Services.Interfaces;
using CF.Customer.Infrastructure.DbContext;
using CF.Customer.Infrastructure.Mappers;
using CF.Customer.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CF.Customer.Infrastructure.DependencyInjection;

/// <summary>
///     Registers the reusable Customer application core (facade, domain services, persistence) so any host
///     - the Web API, a console app, a worker, etc. - can wire it up with a single call.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerCore(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<CustomerContext>(options =>
            options.UseSqlServer(connectionString, sql => sql.MigrationsAssembly("CF.Migrations")));

        services.AddTransient<ICustomerFacade, CustomerFacade>();
        services.AddTransient<ICustomerService, CustomerService>();
        services.AddTransient<ICustomerRepository, CustomerRepository>();
        services.AddTransient<IPasswordHasherService, PasswordHasherService>();
        services.AddSingleton<ICustomerMapper, CustomerMapper>();

        return services;
    }
}
