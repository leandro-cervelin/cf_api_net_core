using CF.Customer.Application.Dtos;
using CF.Customer.Application.Facades.Interfaces;
using CF.Customer.Infrastructure.DbContext;
using CF.Customer.Infrastructure.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// This console host reuses the exact same application core as CF.Api. The only host-specific
// concern is configuration; the whole service graph is wired with a single call: AddCustomerCore(...).
var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DbConnection")
    ?? throw new InvalidOperationException(
        "Missing connection string 'DbConnection'. Provide it via appsettings.json, user-secrets, " +
        "or the ConnectionStrings__DbConnection environment variable.");

builder.Services.AddCustomerCore(connectionString);

using var host = builder.Build();
var cancellationToken = CancellationToken.None;

// All work runs inside a DI scope. The EF Core DbContext is scoped, so it (and the facade
// graph that depends on it) must not be resolved from the root provider - that throws under
// the scope validation Host.CreateApplicationBuilder enables in the Development environment.
await using var scope = host.Services.CreateAsyncScope();
var services = scope.ServiceProvider;

// Ensure the schema exists (no-op when already migrated).
var context = services.GetRequiredService<CustomerContext>();
await context.Database.MigrateAsync(cancellationToken);

var facade = services.GetRequiredService<ICustomerFacade>();

Console.WriteLine("== Customer core demo (console host) ==");

// 1. Create a customer through the shared facade.
var email = $"console_{DateTime.UtcNow:yyyyMMdd_HHmmssfff}@test.com";
var id = await facade.CreateAsync(new CustomerRequestDto
{
    FirstName = "Console",
    Surname = "Demo",
    Email = email,
    Password = "Password1@",
    ConfirmPassword = "Password1@"
}, cancellationToken);
Console.WriteLine($"Created customer #{id} <{email}>.");

// 2. Read it back by id.
var created = await facade.GetByFilterAsync(new CustomerFilterDto { Id = id }, cancellationToken);
Console.WriteLine($"Fetched: {created?.FullName} <{created?.Email}>");

// 3. List the first page of customers, ordered by email.
var page = await facade.GetListByFilterAsync(
    new CustomerFilterDto { CurrentPage = 1, PageSize = 10, OrderBy = "email", SortBy = "asc" },
    cancellationToken);

Console.WriteLine($"Total customers: {page.Count}. Page {page.CurrentPage} ({page.Result.Count} rows):");
foreach (var customer in page.Result)
    Console.WriteLine($"  #{customer.Id,-4} {customer.FullName} <{customer.Email}>");
