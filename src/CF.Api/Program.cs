using CF.Api.Authentication;
using CF.Api.Filters;
using CF.Api.Startup;
using CF.Customer.Infrastructure.DependencyInjection;
using Microsoft.AspNetCore.HttpLogging;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// NLog writes every log line (configured in appsettings.json); the default providers would print each one twice.
// RemoveLoggerFactoryFilter = false makes NLog respect the usual Logging:LogLevel settings in appsettings.json.
builder.Logging.ClearProviders();
builder.Host.UseNLog(new NLogAspNetCoreOptions { RemoveLoggerFactoryFilter = false });
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// The customer core: business rules and database access, the same registration the console app uses.
builder.Services.AddCustomerCore(builder.Configuration.GetConnectionString("DbConnection")!);

// Controllers. ExceptionFilter turns domain exceptions (validation, not found) into HTTP responses.
builder.Services.AddControllers(x => x.Filters.Add<ExceptionFilter>());
builder.Services.AddProblemDetails();

// One log entry per request with only the method, path, status and duration: query strings such as ?email= and the
// request and response bodies carry personal data, so they're never logged.
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath |
                            HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
    options.CombineLogs = true;
});

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiDocumentation();
builder.Services.AddProductionSetup(builder.Configuration);

await using var app = builder.Build();

app.MigrateDatabaseIfEnabled();

app.UseHttpLogging(); // first, so it logs the final status code of every request, including errors
app.UseProductionSetup();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapApiDocumentation();
app.MapControllers();
app.MapHealthEndpoints();

await app.RunAsync();
