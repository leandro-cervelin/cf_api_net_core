using CF.Api.Authentication;
using CF.Api.Filters;
using CF.Api.Startup;
using CF.Customer.Infrastructure.DependencyInjection;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

// NLog writes every log line (configured in appsettings.json); the default providers would print each one twice.
builder.Logging.ClearProviders();
builder.Host.UseNLog();
builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);

// The customer core: business rules and database access, the same registration the console app uses.
builder.Services.AddCustomerCore(builder.Configuration.GetConnectionString("DbConnection")!);

// Controllers. ExceptionFilter turns domain exceptions (validation, not found) into HTTP responses.
builder.Services.AddControllers(x => x.Filters.Add<ExceptionFilter>());
builder.Services.AddProblemDetails();

builder.Services.AddJwtAuthentication(builder.Configuration);
builder.Services.AddApiDocumentation();
builder.Services.AddProductionSetup(builder.Configuration);

await using var app = builder.Build();

app.MigrateDatabaseIfEnabled();

app.UseProductionSetup();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapApiDocumentation();
app.MapControllers();
app.MapHealthEndpoints();

await app.RunAsync();
